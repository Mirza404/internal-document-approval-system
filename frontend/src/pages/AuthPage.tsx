import { useEffect, useMemo, useRef, useState, type FormEvent } from "react";
import { InteractionStatus } from "@azure/msal-browser";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { useNavigate } from "react-router-dom";
import { localLogin, microsoftLogin } from "../api/auth";
import { getGraphAccessToken, graphLoginRequest } from "../auth/msal";
import { useAuth } from "../hooks/useAuth";

type AuthStatus = "idle" | "loading";

const roleRedirect = (role?: string) => {
  switch ((role ?? "").toLowerCase()) {
    case "admin":
      return "/admin";
    case "approver":
    case "reviewer":
      return "/reviews";
    default:
      return "/dashboard";
  }
};

const getAuthErrorMessage = (error: unknown) => {
  if (typeof error === "object" && error !== null && "response" in error) {
    const response = (
      error as { response?: { data?: unknown; status?: number } }
    ).response;

    if (typeof response?.data === "string" && response.data.trim()) {
      return response.data;
    }

    if (response?.status) {
      if (typeof response.data === "object" && response.data !== null) {
        const detail =
          "detail" in response.data && typeof response.data.detail === "string"
            ? response.data.detail
            : null;
        const title =
          "title" in response.data && typeof response.data.title === "string"
            ? response.data.title
            : null;

        if (detail || title) {
          return (
            detail ?? title ?? `Microsoft sign-in failed (${response.status}).`
          );
        }
      }

      return `Microsoft sign-in failed (${response.status}).`;
    }
  }

  return "Sign-in failed. Make sure the backend is running and your account is active.";
};

const fieldClass =
  "w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-2xs outline-none transition placeholder:text-muted-foreground focus:border-primary/60 focus:ring-2 focus:ring-primary/15";

const labelClass = "text-xs font-semibold uppercase text-muted-foreground";

const AuthLoadingScreen = () => (
  <main className="grid min-h-screen place-items-center bg-background px-4 text-foreground">
    <section className="w-full max-w-md rounded-2xl border border-border/60 bg-card p-8 text-center shadow-lg shadow-primary/10">
      <span
        aria-hidden="true"
        className="mx-auto block h-10 w-10 animate-spin rounded-full border-4 border-primary/20 border-t-primary"
      />
      <h1 className="mt-6 text-2xl font-semibold">Signing you in</h1>
      <p className="mt-3 text-sm leading-6 text-muted-foreground">
        We are verifying your Microsoft account and preparing your workspace.
      </p>
    </section>
  </main>
);

const AuthPage = () => {
  const { accounts, instance, inProgress } = useMsal();
  const isMicrosoftAuthenticated = useIsAuthenticated();
  const { isAuthenticated, user, setSession } = useAuth();
  const navigate = useNavigate();
  const [status, setStatus] = useState<AuthStatus>("idle");
  const [error, setError] = useState<string | null>(null);
  const [localStatus, setLocalStatus] = useState<AuthStatus>("idle");
  const [localError, setLocalError] = useState<string | null>(null);
  const [localEmail, setLocalEmail] = useState("");
  const [localPassword, setLocalPassword] = useState("");
  const [showAdminLogin, setShowAdminLogin] = useState(false);
  const isProcessingRef = useRef(false);
  const failedAccountRef = useRef<string | null>(null);

  const copy = useMemo(
    () => ({
      title: "Welcome back to InternalDocs.",
      subtitle:
        "Use your Microsoft account to continue managing approvals and review queues.",
      action: "Sign in",
    }),
    [],
  );

  useEffect(() => {
    if (isAuthenticated && user) {
      navigate(roleRedirect(user.role), { replace: true });
    }
  }, [isAuthenticated, navigate, user]);

  useEffect(() => {
    if (!isMicrosoftAuthenticated || inProgress !== InteractionStatus.None) {
      return;
    }

    if (isProcessingRef.current) {
      return;
    }

    const accountKey =
      instance.getActiveAccount()?.homeAccountId ??
      accounts[0]?.homeAccountId ??
      "active-account";
    if (failedAccountRef.current === accountKey) {
      return;
    }

    isProcessingRef.current = true;
    failedAccountRef.current = accountKey;
    setStatus("loading");
    setError(null);

    const finalizeLogin = async () => {
      try {
        const microsoftToken = await getGraphAccessToken();
        if (!microsoftToken) {
          throw new Error("Microsoft access token not available.");
        }

        const response = await microsoftLogin(microsoftToken);

        setSession(response.accessToken, {
          userId: response.userId,
          email: response.email,
          fullName: response.fullName,
          role: response.role,
        });

        failedAccountRef.current = null;
        navigate(roleRedirect(response.role), { replace: true });
      } catch (authError: unknown) {
        setError(getAuthErrorMessage(authError));
      } finally {
        setStatus("idle");
        isProcessingRef.current = false;
      }
    };

    void finalizeLogin();
  }, [
    accounts,
    inProgress,
    instance,
    isMicrosoftAuthenticated,
    navigate,
    setSession,
  ]);

  const handleMicrosoftAuth = () => {
    failedAccountRef.current = null;
    setStatus("loading");
    setError(null);
    void instance.loginRedirect({
      ...graphLoginRequest,
      prompt: "select_account",
    });
  };

  const handleLocalLogin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!localEmail.trim() || !localPassword.trim()) {
      setLocalError("Email and password are required.");
      return;
    }

    setLocalStatus("loading");
    setLocalError(null);

    try {
      const response = await localLogin(localEmail.trim(), localPassword);
      setSession(response.accessToken, {
        userId: response.userId,
        email: response.email,
        fullName: response.fullName,
        role: response.role,
      });
      navigate(roleRedirect(response.role), { replace: true });
    } catch (authError: unknown) {
      setLocalError(getAuthErrorMessage(authError));
    } finally {
      setLocalStatus("idle");
    }
  };

  const isAuthenticating =
    inProgress !== InteractionStatus.None ||
    status === "loading" ||
    (isMicrosoftAuthenticated && !error && !isAuthenticated);

  if (isAuthenticating) {
    return <AuthLoadingScreen />;
  }

  return (
    <main className="min-h-screen bg-background text-foreground">
      <section className="mx-auto grid min-h-screen max-w-6xl items-center gap-10 px-4 py-10 sm:px-6 lg:grid-cols-[1.05fr_0.95fr] lg:px-8">
        <div className="space-y-6">
          <p className="text-xs font-semibold uppercase tracking-[0.35em] text-primary/80">
            InternalDocs
          </p>
          <h1 className="max-w-xl text-4xl font-semibold leading-tight sm:text-5xl">
            {copy.title}
          </h1>
          <p className="max-w-xl text-base leading-7 text-muted-foreground">
            {copy.subtitle}
          </p>
          <div className="rounded-2xl border border-border/60 bg-card/70 p-5 shadow-2xs">
            <p className="text-sm font-medium text-foreground">
              Access is restricted to active IUS accounts.
            </p>
            <p className="mt-2 text-sm text-muted-foreground">
              We will use your Microsoft profile to verify your identity and
              assign the correct role.
            </p>
          </div>
        </div>

        <div className="rounded-3xl border border-border/60 bg-card p-8 shadow-xl shadow-primary/10">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.35em] text-muted-foreground">
                Secure access
              </p>
              <h2 className="mt-2 text-2xl font-semibold text-foreground">
                Sign in
              </h2>
            </div>
          </div>

          <div className="mt-8 space-y-4">
            <button
              type="button"
              onClick={handleMicrosoftAuth}
              disabled={inProgress !== InteractionStatus.None}
              className="inline-flex min-h-12 w-full items-center justify-center gap-2 rounded-full bg-primary px-6 text-sm font-semibold text-primary-foreground shadow-md shadow-primary/20 transition hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-70"
            >
              Sign in with Microsoft
            </button>
          </div>

          <div className="mt-8 border-t border-border/60 pt-6">
            <button
              type="button"
              onClick={() => setShowAdminLogin((current) => !current)}
              className="inline-flex w-full items-center justify-between rounded-full border border-border/60 bg-background px-6 py-3 text-[13px] font-medium uppercase tracking-[0.25em] text-foreground shadow-2xs transition hover:border-primary/50 hover:text-primary"
            >
              {showAdminLogin ? "Hide admin login" : "Login as admin"}
              <span aria-hidden="true">{showAdminLogin ? "-" : "+"}</span>
            </button>

            {showAdminLogin ? (
              <div className="mt-4 space-y-4">
                <p className="text-xs font-semibold uppercase tracking-[0.3em] text-muted-foreground">
                  Admin access
                </p>
                <form className="space-y-4" onSubmit={handleLocalLogin}>
                  <label className="block space-y-2">
                    <span className={labelClass}>Email</span>
                    <input
                      className={fieldClass}
                      type="email"
                      autoComplete="username"
                      value={localEmail}
                      onChange={(event) => setLocalEmail(event.target.value)}
                      placeholder="admin@internaldocs.local"
                    />
                  </label>
                  <label className="block space-y-2">
                    <span className={labelClass}>Password</span>
                    <input
                      className={fieldClass}
                      type="password"
                      autoComplete="current-password"
                      value={localPassword}
                      onChange={(event) => setLocalPassword(event.target.value)}
                      placeholder="Enter your admin password"
                    />
                  </label>
                  <button
                    type="submit"
                    disabled={localStatus === "loading"}
                    className="inline-flex min-h-11 w-full items-center justify-center rounded-full border border-border/60 bg-background px-6 text-sm font-semibold text-foreground shadow-2xs transition hover:border-primary/50 hover:text-primary disabled:cursor-not-allowed disabled:opacity-70"
                  >
                    {localStatus === "loading" ? "Signing in" : "Sign in as admin"}
                  </button>
                </form>
                {localError ? (
                  <div className="rounded-2xl border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive">
                    {localError}
                  </div>
                ) : null}
              </div>
            ) : null}
          </div>

          {error ? (
            <div className="mt-6 rounded-2xl border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive">
              {error}
            </div>
          ) : null}
        </div>
      </section>
    </main>
  );
};

export default AuthPage;
