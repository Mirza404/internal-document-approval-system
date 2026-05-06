import { useEffect, useMemo, useRef, useState } from "react";
import { InteractionStatus } from "@azure/msal-browser";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { useNavigate } from "react-router-dom";
import { microsoftLogin } from "../api/auth";
import { getGraphAccessToken, graphLoginRequest } from "../auth/msal";
import { useAuth } from "../hooks/useAuth";

type AuthStatus = "idle" | "loading";

const roleRedirect = (role?: string) => {
  switch ((role ?? "").toLowerCase()) {
    case "admin":
      return "/admin";
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

  return "Microsoft sign-in failed. Make sure the backend is running and your account is active.";
};

const AuthPage = () => {
  const { instance, inProgress } = useMsal();
  const isMicrosoftAuthenticated = useIsAuthenticated();
  const { isAuthenticated, user, setSession } = useAuth();
  const navigate = useNavigate();
  const [status, setStatus] = useState<AuthStatus>("idle");
  const [error, setError] = useState<string | null>(null);
  const isProcessingRef = useRef(false);

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

    isProcessingRef.current = true;
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

        navigate(roleRedirect(response.role), { replace: true });
      } catch (authError: unknown) {
        setError(getAuthErrorMessage(authError));
      } finally {
        setStatus("idle");
        isProcessingRef.current = false;
      }
    };

    void finalizeLogin();
  }, [inProgress, isMicrosoftAuthenticated, navigate, setSession]);

  const handleMicrosoftAuth = () => {
    setStatus("loading");
    setError(null);
    void instance.loginRedirect({
      ...graphLoginRequest,
      prompt: "select_account",
    });
  };

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
                Login
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
              disabled={status === "loading"}
              className="inline-flex min-h-12 w-full items-center justify-center gap-2 rounded-full bg-primary px-6 text-sm font-semibold text-primary-foreground shadow-md shadow-primary/20 transition hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-70"
            >
              {status === "loading" ? (
                <>
                  <span
                    aria-hidden="true"
                    className="h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground/30 border-t-primary-foreground"
                  />
                  Signing in...
                </>
              ) : (
                "Sign in with Microsoft"
              )}
            </button>
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
