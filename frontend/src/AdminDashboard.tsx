import { useUsers } from './hooks/useUsers';
import { useDocumentTypes } from './hooks/useDocumentTypes';

export const AdminDashboard = ({ onNavigate }: { onNavigate: (page: 'users' | 'doctypes') => void }) => {
  const { users } = useUsers();
  const { documentTypes } = useDocumentTypes();

  return (
    <div className="min-h-screen bg-slate-100 pb-16">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:px-6 lg:px-8">
        <header className="rounded-3xl bg-gradient-to-br from-white via-white to-slate-50 px-8 py-10 shadow-sm ring-1 ring-slate-900/5">
          <p className="text-xs font-medium uppercase tracking-[0.35em] text-slate-500">
            Administration
          </p>
          <div className="mt-6">
            <div className="space-y-3">
              <h1 className="text-3xl font-semibold text-slate-900 sm:text-4xl">
                Admin Dashboard
              </h1>
              <p className="max-w-2xl text-base text-slate-600">
                Manage users, document types, and system settings. Control access
                permissions and configure document workflows.
              </p>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-2">
          <article className="rounded-2xl bg-white px-6 py-5 shadow-sm ring-1 ring-slate-900/5">
            <p className="text-sm text-slate-500">Total Users</p>
            <p className="mt-3 text-3xl font-semibold text-slate-900">
              {users.length}
            </p>
            <p className="text-xs font-medium uppercase tracking-wide text-slate-400">
              Active accounts
            </p>
          </article>

          <article className="rounded-2xl bg-white px-6 py-5 shadow-sm ring-1 ring-slate-900/5">
            <p className="text-sm text-slate-500">Document Types</p>
            <p className="mt-3 text-3xl font-semibold text-slate-900">
              {documentTypes.length}
            </p>
            <p className="text-xs font-medium uppercase tracking-wide text-slate-400">
              Configured types
            </p>
          </article>
        </section>

        <section className="grid gap-4 md:grid-cols-2">
          <button
            onClick={() => onNavigate('users')}
            className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-900/5 transition hover:ring-brand-500 hover:shadow-md"
          >
            <div className="space-y-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-brand-50">
                <svg
                  className="h-6 w-6 text-brand-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 4.354a4 4 0 110 5.292M15 19H9a6 6 0 016-6v0a6 6 0 016 6v0a2 2 0 01-2 2h-2a2 2 0 01-2-2v-5.5A2.5 2.5 0 0012 9.5h0a2.5 2.5 0 00-2.5 2.5V13"
                  />
                </svg>
              </div>
              <div className="text-left">
                <h3 className="font-semibold text-slate-900">User Management</h3>
                <p className="text-sm text-slate-600">
                  Create accounts, manage roles, and control access
                </p>
              </div>
            </div>
          </button>

          <button
            onClick={() => onNavigate('doctypes')}
            className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-900/5 transition hover:ring-brand-500 hover:shadow-md"
          >
            <div className="space-y-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-brand-50">
                <svg
                  className="h-6 w-6 text-brand-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                  />
                </svg>
              </div>
              <div className="text-left">
                <h3 className="font-semibold text-slate-900">
                  Document Types
                </h3>
                <p className="text-sm text-slate-600">
                  Define document categories and workflows
                </p>
              </div>
            </div>
          </button>
        </section>
      </div>
    </div>
  );
};
