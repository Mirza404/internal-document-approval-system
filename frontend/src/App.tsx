import { useMemo, useState } from "react";
import Pill from "./components/ui/Pill";
import { reviewQueue } from "./mockData/reviewQueue";
import { activityFeed } from "./mockData/activityFeed";
import { stats } from "./mockData/stats";
import { automations } from "./mockData/automations";
import { stageStyles } from "./components/styles/StageStyles";
import { priorityStyles } from "./components/styles/PriorityStyles";
import { filterOptions } from "./components/utils/FilterOptions";
import { AdminDashboard } from "./AdminDashboard";
import { AdminUsersPage } from "./AdminUsersPage";
import { AdminDocTypesPage } from "./AdminDocTypesPage";

function App() {
  const [filter, setFilter] = useState<(typeof filterOptions)[number]>("All");
  const [adminPage, setAdminPage] = useState<"dashboard" | "users" | "doctypes" | null>(null);

  const filteredQueue = useMemo(() => {
    if (filter === "All") {
      return reviewQueue;
    }

    return reviewQueue.filter((item) => item.stage === filter);
  }, [filter]);

  if (adminPage === "dashboard") {
    return <AdminDashboard onNavigate={(page) => setAdminPage(page)} />;
  }

  if (adminPage === "users") {
    return (
      <div>
        <button
          onClick={() => setAdminPage("dashboard")}
          className="fixed top-4 left-4 z-50 rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 bg-white shadow-sm"
        >
          ← Back to Admin
        </button>
        <AdminUsersPage />
      </div>
    );
  }

  if (adminPage === "doctypes") {
    return (
      <div>
        <button
          onClick={() => setAdminPage("dashboard")}
          className="fixed top-4 left-4 z-50 rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 bg-white shadow-sm"
        >
          ← Back to Admin
        </button>
        <AdminDocTypesPage />
      </div>
    );
  }

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
              <button className="rounded-full bg-brand-600 px-5 py-2 text-sm font-semibold text-white shadow-card transition hover:bg-brand-700">
                New approval flow
              </button>
              <button className="rounded-full border border-slate-200 px-5 py-2 text-sm font-semibold text-slate-700 hover:border-slate-400">
                Share weekly digest
              </button>
              <button
                onClick={() => setAdminPage("dashboard")}
                className="rounded-full border border-slate-200 px-5 py-2 text-sm font-semibold text-slate-700 hover:border-slate-400"
              >
                ⚙️ Admin
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
