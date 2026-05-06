import { useMemo, useState } from "react";
import type { AuthUser } from "../auth/authStorage";
import Pill from "../components/ui/Pill";
import { reviewQueue } from "../mockData/reviewQueue";
import { activityFeed } from "../mockData/activityFeed";
import { stats } from "../mockData/stats";
import { automations } from "../mockData/automations";
import { stageStyles } from "../components/styles/StageStyles";
import { priorityStyles } from "../components/styles/PriorityStyles";
import { filterOptions } from "../components/utils/FilterOptions";

interface DashboardPageProps {
  authUser: AuthUser;
  onLogout: () => void;
}

const DashboardPage = ({ authUser, onLogout }: DashboardPageProps) => {
  const [filter, setFilter] = useState<(typeof filterOptions)[number]>("All");

  const filteredQueue = useMemo(() => {
    if (filter === "All") {
      return reviewQueue;
    }

    return reviewQueue.filter((item) => item.stage === filter);
  }, [filter]);

  return (
    <div className="min-h-screen bg-muted/40 pb-16">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:px-6 lg:px-8">
        <header className="rounded-3xl border border-border/60 bg-card/80 px-8 py-10 shadow-sm backdrop-blur">
          <p className="text-xs font-medium uppercase tracking-[0.35em] text-muted-foreground">
            Internal workflows
          </p>
          <div className="mt-6 flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-3">
              <h1 className="text-3xl font-semibold text-foreground sm:text-4xl">
                Document approvals, orchestrated end-to-end
              </h1>
              <p className="max-w-2xl text-base text-muted-foreground">
                Track where every policy, contract, and playbook sits in the
                pipeline. Tailwind-powered components keep styling consistent so
                teams ship compliant docs without hand-written CSS.
              </p>
            </div>
            <div className="flex flex-wrap gap-3">
              <div className="rounded-full border border-border/60 bg-background/70 px-4 py-2 text-sm font-medium text-foreground/80">
                {authUser.fullName} · {authUser.role}
              </div>
              <button
                type="button"
                onClick={onLogout}
                className="rounded-full border border-border/60 bg-background/80 px-5 py-2 text-sm font-semibold text-foreground/80 transition hover:border-primary/40 hover:text-foreground"
              >
                Sign out
              </button>
              <button className="rounded-full bg-primary px-5 py-2 text-sm font-semibold text-primary-foreground shadow-md shadow-primary/20 transition hover:bg-primary/90">
                New approval flow
              </button>
              <button className="rounded-full border border-border/60 bg-card px-5 py-2 text-sm font-semibold text-foreground/70 transition hover:border-primary/40 hover:text-foreground">
                Share weekly digest
              </button>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          {stats.map((stat) => (
            <article
              key={stat.label}
              className="rounded-2xl border border-border/60 bg-card px-6 py-5 shadow-2xs"
            >
              <p className="text-sm text-muted-foreground">{stat.label}</p>
              <p className="mt-3 text-3xl font-semibold text-foreground">
                {stat.value}
              </p>
              <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                {stat.helper}
              </p>
            </article>
          ))}
        </section>

        <section className="grid gap-6 lg:grid-cols-3">
          <div className="rounded-3xl border border-border/60 bg-card p-6 shadow-2xs lg:col-span-2">
            <div className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Review queue
                </p>
                <h2 className="text-xl font-semibold text-foreground">
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
                        ? "border-primary/40 bg-primary/10 text-primary"
                        : "border-border/60 bg-background text-muted-foreground hover:border-primary/30 hover:text-foreground"
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
                  className="flex flex-col gap-4 rounded-2xl border border-border/60 bg-background/40 px-4 py-4 transition hover:border-primary/30 hover:bg-card/80 sm:flex-row sm:items-center"
                >
                  <div className="flex-1">
                    <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                      {item.id}
                    </p>
                    <h3 className="mt-1 text-lg font-semibold text-foreground">
                      {item.title}
                    </h3>
                    <p className="text-sm text-muted-foreground">
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
                    <div className="text-right text-sm text-muted-foreground">
                      <p>{item.due}</p>
                      <p className="text-xs">Updated {item.updated}</p>
                    </div>
                    <button className="rounded-full border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-primary">
                      Open
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </div>

          <div className="space-y-6">
            <section className="rounded-3xl border border-border/60 bg-card p-6 shadow-2xs">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Live activity
                  </p>
                  <h2 className="text-xl font-semibold text-foreground">
                    Today
                  </h2>
                </div>
                <button className="text-xs font-semibold text-primary hover:text-primary/80">
                  View log
                </button>
              </div>
              <div className="mt-6 space-y-5">
                {activityFeed.map((activity) => (
                  <div key={activity.id} className="flex gap-3">
                    <div className="relative mt-1 h-2 w-2">
                      <span
                        className="absolute inset-0 rounded-full bg-primary"
                        aria-hidden="true"
                      />
                    </div>
                    <div className="flex-1">
                      <p className="text-sm font-medium text-foreground">
                        {activity.summary}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {activity.owner} · {activity.channel}
                      </p>
                    </div>
                    <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                      {activity.time}
                    </p>
                  </div>
                ))}
              </div>
            </section>

            <section className="rounded-3xl border border-border/60 bg-card p-6 shadow-2xs">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Automations
                  </p>
                  <h2 className="text-xl font-semibold text-foreground">
                    Stay proactive
                  </h2>
                </div>
                <button className="rounded-full border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-primary">
                  Configure
                </button>
              </div>
              <div className="mt-6 space-y-4">
                {automations.map((flow) => (
                  <article
                    key={flow.title}
                    className="rounded-2xl border border-border/60 bg-background/40 p-4"
                  >
                    <div className="flex items-center justify-between">
                      <h3 className="text-base font-semibold text-foreground">
                        {flow.title}
                      </h3>
                      <Pill
                        className={
                          flow.status === "Active"
                            ? "bg-secondary/20 text-secondary"
                            : "bg-muted text-muted-foreground"
                        }
                      >
                        {flow.status}
                      </Pill>
                    </div>
                    <p className="mt-2 text-sm text-muted-foreground">
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
};

export default DashboardPage;
