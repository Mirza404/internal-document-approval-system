import { useEffect, useMemo, useState } from "react";
import heroImage from "./assets/hero.png";
import { microsoftLogin, microsoftRegister, type AuthResponse } from "./api/auth";
import Pill from "./components/ui/Pill";
import { reviewQueue } from "./mockData/reviewQueue";
import { activityFeed } from "./mockData/activityFeed";
import { stats } from "./mockData/stats";
import { automations } from "./mockData/automations";
import { stageStyles } from "./components/styles/StageStyles";
import { priorityStyles } from "./components/styles/PriorityStyles";
import { filterOptions } from "./components/utils/FilterOptions";

const oauthStateKey = "microsoftOAuthState";
const authUserKey = "authUser";

function App() {
  const [authUser, setAuthUser] = useState<AuthResponse | null>(() => {
    const token = localStorage.getItem("token");
    const storedUser = localStorage.getItem(authUserKey);

    if (!token || !storedUser) {
      return null;
    }

    try {
      return JSON.parse(storedUser) as AuthResponse;
    } catch {
      localStorage.removeItem("token");
      localStorage.removeItem(authUserKey);
      return null;
    }
  });
  const [authStatus, setAuthStatus] = useState("idle");
  const [authError, setAuthError] = useState<string | null>(null);

  useEffect(() => {
    const hash = new URLSearchParams(window.location.hash.slice(1));
    const microsoftAccessToken = hash.get("access_token");

    if (!microsoftAccessToken) {
      return;
    }

    const returnedState = hash.get("state");
    const expectedState = sessionStorage.getItem(oauthStateKey);
    sessionStorage.removeItem(oauthStateKey);
    window.history.replaceState(null, document.title, window.location.pathname);

    if (!returnedState || returnedState !== expectedState) {
      setAuthError("Microsoft sign-in could not be verified. Please try again.");
      return;
    }

    setAuthStatus("loading");
    setAuthError(null);

    microsoftLogin(microsoftAccessToken)
      .catch((error: unknown) => {
        if (
          typeof error === "object" &&
          error !== null &&
          "response" in error &&
          (error as { response?: { status?: number } }).response?.status === 404
        ) {
          return microsoftRegister(microsoftAccessToken);
        }

        throw error;
      })
      .then((response) => {
        localStorage.setItem("token", response.accessToken);
        localStorage.setItem(authUserKey, JSON.stringify(response));
        setAuthUser(response);
      })
      .catch((error: unknown) => {
        setAuthError(getAuthErrorMessage(error));
      })
      .finally(() => {
        setAuthStatus("idle");
      });
  }, []);

  const handleMicrosoftLogin = () => {
    const clientId = import.meta.env.VITE_MICROSOFT_CLIENT_ID;
    const tenantId = import.meta.env.VITE_MICROSOFT_TENANT_ID;
    const redirectUri =
      import.meta.env.VITE_MICROSOFT_REDIRECT_URI || window.location.origin;

    if (!clientId || !tenantId) {
      setAuthError(
        "Microsoft sign-in is not configured. Set VITE_MICROSOFT_CLIENT_ID and VITE_MICROSOFT_TENANT_ID.",
      );
      return;
    }

    const state = crypto.randomUUID();
    sessionStorage.setItem(oauthStateKey, state);

    const authorizeUrl = new URL(
      `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize`,
    );
    authorizeUrl.searchParams.set("client_id", clientId);
    authorizeUrl.searchParams.set("response_type", "token");
    authorizeUrl.searchParams.set("redirect_uri", redirectUri);
    authorizeUrl.searchParams.set("response_mode", "fragment");
    authorizeUrl.searchParams.set("scope", "openid profile email User.Read");
    authorizeUrl.searchParams.set("state", state);
    authorizeUrl.searchParams.set("prompt", "select_account");

    window.location.assign(authorizeUrl.toString());
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem(authUserKey);
    setAuthUser(null);
  };

  if (!authUser) {
    return (
      <LandingPage
        authError={authError}
        authStatus={authStatus}
        onLogin={handleMicrosoftLogin}
      />
    );
  }

  return <Dashboard authUser={authUser} onLogout={handleLogout} />;
}

function getAuthErrorMessage(error: unknown) {
  if (
    typeof error === "object" &&
    error !== null &&
    "response" in error
  ) {
    const response = (error as { response?: { data?: unknown; status?: number } })
      .response;

    if (typeof response?.data === "string" && response.data.trim()) {
      return response.data;
    }

    if (response?.status) {
      return `Microsoft sign-in failed with status ${response.status}.`;
    }
  }

  return "Microsoft sign-in failed. Make sure the backend is running and your account is active.";
}

interface LandingPageProps {
  authError: string | null;
  authStatus: string;
  onLogin: () => void;
}

function LandingPage({ authError, authStatus, onLogin }: LandingPageProps) {
  return (
    <main className="min-h-screen bg-slate-950 text-white">
      <section className="mx-auto grid min-h-screen max-w-6xl gap-10 px-4 py-8 sm:px-6 lg:grid-cols-[1.05fr_0.95fr] lg:items-center lg:px-8">
        <div className="py-12">
          <p className="text-sm font-semibold uppercase tracking-[0.35em] text-cyan-300">
            InternalDocs
          </p>
          <h1 className="mt-6 max-w-2xl text-4xl font-semibold leading-tight sm:text-5xl">
            Secure document approvals for IUS teams.
          </h1>
          <p className="mt-5 max-w-xl text-base leading-7 text-slate-300">
            Review queues, approval history, and internal workflow actions stay
            behind Microsoft sign-in.
          </p>

          <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:items-center">
            <button
              type="button"
              onClick={onLogin}
              disabled={authStatus === "loading"}
              className="inline-flex min-h-12 items-center justify-center rounded-lg bg-cyan-400 px-5 text-sm font-semibold text-slate-950 shadow-lg shadow-cyan-950/30 transition hover:bg-cyan-300 disabled:cursor-not-allowed disabled:opacity-70"
            >
              {authStatus === "loading" ? "Signing in..." : "Sign in with Microsoft"}
            </button>
          </div>

          {authError ? (
            <p className="mt-5 max-w-xl rounded-lg border border-red-400/30 bg-red-500/10 px-4 py-3 text-sm text-red-100">
              {authError}
            </p>
          ) : null}
        </div>

        <div className="relative min-h-[360px] overflow-hidden rounded-lg border border-white/10 bg-slate-900 shadow-2xl shadow-slate-950/60">
          <img
            src={heroImage}
            alt="Document workflow dashboard preview"
            className="h-full min-h-[360px] w-full object-cover"
          />
          <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-slate-950/95 via-slate-950/50 to-transparent p-6">
            <p className="text-sm font-medium text-slate-200">
              Approval status, ownership, and recent activity in one workspace.
            </p>
          </div>
        </div>
      </section>
    </main>
  );
}

interface DashboardProps {
  authUser: AuthResponse;
  onLogout: () => void;
}

function Dashboard({ authUser, onLogout }: DashboardProps) {
  const [filter, setFilter] = useState<(typeof filterOptions)[number]>("All");

  const filteredQueue = useMemo(() => {
    if (filter === "All") {
      return reviewQueue;
    }

    return reviewQueue.filter((item) => item.stage === filter);
  }, [filter]);

  return (
    <div className="min-h-screen bg-slate-100 pb-16">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:px-6 lg:px-8">
        <header className="rounded-3xl bg-gradient-to-br from-white via-white to-slate-50 px-8 py-10 shadow-sm ring-1 ring-slate-900/5">
          <p className="text-xs font-medium uppercase tracking-[0.35em] text-slate-500">
            Internal workflows
          </p>
          <div className="mt-6 flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-3">
              <h1 className="text-3xl font-semibold text-slate-900 sm:text-4xl">
                Document approvals, orchestrated end-to-end
              </h1>
              <p className="max-w-2xl text-base text-slate-600">
                Track where every policy, contract, and playbook sits in the
                pipeline. Tailwind-powered components keep styling consistent so
                teams ship compliant docs without hand-written CSS.
              </p>
            </div>
            <div className="flex flex-wrap gap-3">
              <div className="rounded-full bg-slate-100 px-4 py-2 text-sm font-medium text-slate-700">
                {authUser.fullName} · {authUser.role}
              </div>
              <button
                type="button"
                onClick={onLogout}
                className="rounded-full border border-slate-200 px-5 py-2 text-sm font-semibold text-slate-700 hover:border-slate-400"
              >
                Sign out
              </button>
              <button className="rounded-full bg-brand-600 px-5 py-2 text-sm font-semibold text-white shadow-card transition hover:bg-brand-700">
                New approval flow
              </button>
              <button className="rounded-full border border-slate-200 px-5 py-2 text-sm font-semibold text-slate-700 hover:border-slate-400">
                Share weekly digest
              </button>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          {stats.map((stat) => (
            <article
              key={stat.label}
              className="rounded-2xl bg-white px-6 py-5 shadow-sm ring-1 ring-slate-900/5"
            >
              <p className="text-sm text-slate-500">{stat.label}</p>
              <p className="mt-3 text-3xl font-semibold text-slate-900">
                {stat.value}
              </p>
              <p className="text-xs font-medium uppercase tracking-wide text-slate-400">
                {stat.helper}
              </p>
            </article>
          ))}
        </section>

        <section className="grid gap-6 lg:grid-cols-3">
          <div className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-900/5 lg:col-span-2">
            <div className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <p className="text-sm font-medium text-slate-500">
                  Review queue
                </p>
                <h2 className="text-xl font-semibold text-slate-900">
                  {filter === "All" ? "All documents" : `${filter} review`}
                </h2>
              </div>
              <div className="flex flex-wrap gap-2">
                {filterOptions.map((option) => (
                  <button
                    key={option}
                    onClick={() => setFilter(option)}
                    className={`rounded-full border px-3 py-1 text-xs font-medium transition ${
                      option === filter
                        ? "border-brand-500 bg-brand-50 text-brand-700"
                        : "border-slate-200 bg-white text-slate-500 hover:border-slate-300"
                    }`}
                  >
                    {option}
                  </button>
                ))}
              </div>
            </div>

            <div className="mt-6 space-y-3">
              {filteredQueue.map((item) => (
                <article
                  key={item.id}
                  className="flex flex-col gap-4 rounded-2xl border border-slate-100 px-4 py-4 transition hover:border-slate-200 hover:bg-slate-50 sm:flex-row sm:items-center"
                >
                  <div className="flex-1">
                    <p className="text-xs font-semibold uppercase tracking-widest text-slate-400">
                      {item.id}
                    </p>
                    <h3 className="mt-1 text-lg font-semibold text-slate-900">
                      {item.title}
                    </h3>
                    <p className="text-sm text-slate-500">
                      Owner · {item.owner} · {item.department}
                    </p>
                  </div>
                  <div className="flex flex-wrap items-center gap-3">
                    <Pill className={stageStyles[item.stage]}>
                      {item.stage}
                    </Pill>
                    <Pill className={priorityStyles[item.priority]}>
                      {item.priority}
                    </Pill>
                    <div className="text-right text-sm text-slate-500">
                      <p>{item.due}</p>
                      <p className="text-xs">Updated {item.updated}</p>
                    </div>
                    <button className="rounded-full border border-slate-200 px-3 py-1 text-xs font-semibold text-slate-600 hover:border-brand-400 hover:text-brand-600">
                      Open
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </div>

          <div className="space-y-6">
            <section className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-900/5">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-slate-500">
                    Live activity
                  </p>
                  <h2 className="text-xl font-semibold text-slate-900">
                    Today
                  </h2>
                </div>
                <button className="text-xs font-semibold text-brand-600 hover:text-brand-700">
                  View log
                </button>
              </div>
              <div className="mt-6 space-y-5">
                {activityFeed.map((activity) => (
                  <div key={activity.id} className="flex gap-3">
                    <div className="relative mt-1 h-2 w-2">
                      <span
                        className="absolute inset-0 rounded-full bg-brand-500"
                        aria-hidden="true"
                      />
                    </div>
                    <div className="flex-1">
                      <p className="text-sm font-medium text-slate-900">
                        {activity.summary}
                      </p>
                      <p className="text-xs text-slate-500">
                        {activity.owner} · {activity.channel}
                      </p>
                    </div>
                    <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                      {activity.time}
                    </p>
                  </div>
                ))}
              </div>
            </section>

            <section className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-900/5">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-slate-500">
                    Automations
                  </p>
                  <h2 className="text-xl font-semibold text-slate-900">
                    Stay proactive
                  </h2>
                </div>
                <button className="rounded-full border border-slate-200 px-3 py-1 text-xs font-semibold text-slate-600 hover:border-brand-400 hover:text-brand-600">
                  Configure
                </button>
              </div>
              <div className="mt-6 space-y-4">
                {automations.map((flow) => (
                  <article
                    key={flow.title}
                    className="rounded-2xl border border-slate-100 p-4"
                  >
                    <div className="flex items-center justify-between">
                      <h3 className="text-base font-semibold text-slate-900">
                        {flow.title}
                      </h3>
                      <Pill
                        className={
                          flow.status === "Active"
                            ? "bg-emerald-50 text-emerald-700"
                            : "bg-slate-100 text-slate-600"
                        }
                      >
                        {flow.status}
                      </Pill>
                    </div>
                    <p className="mt-2 text-sm text-slate-500">
                      {flow.description}
                    </p>
                  </article>
                ))}
              </div>
            </section>
          </div>
        </section>
      </div>
    </div>
  );
}

export default App;
