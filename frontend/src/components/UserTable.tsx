import Pill from './ui/Pill';
import type { User } from '../api/admin';

interface UserTableProps {
  users: User[];
  isLoading?: boolean;
  onEdit: (user: User) => void;
  onDelete: (user: User) => void;
}

const roleStyles: Record<string, string> = {
  Admin: 'bg-rose-50 text-rose-700',
  Employee: 'bg-blue-50 text-blue-700',
  Approver: 'bg-amber-50 text-amber-800',
};

export const UserTable = ({
  users,
  isLoading = false,
  onEdit,
  onDelete,
}: UserTableProps) => {
  if (isLoading) {
    return (
      <div className="flex h-32 items-center justify-center rounded-2xl bg-white ring-1 ring-slate-900/5">
        <p className="text-sm text-slate-500">Loading users...</p>
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="flex h-32 flex-col items-center justify-center rounded-2xl bg-white ring-1 ring-slate-900/5">
        <p className="text-sm text-slate-500">No users found</p>
        <p className="text-xs text-slate-400">Create one to get started</p>
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-2xl bg-white ring-1 ring-slate-900/5">
      <table className="w-full">
        <thead>
          <tr className="border-b border-slate-200 bg-slate-50">
            <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Name
            </th>
            <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Email
            </th>
            <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Role
            </th>
            <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Department
            </th>
            <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Status
            </th>
            <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
              Actions
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {users.map((user) => (
            <tr
              key={user.id}
              className="transition hover:bg-slate-50"
            >
              <td className="px-6 py-4">
                <p className="font-medium text-slate-900">{user.fullName}</p>
              </td>
              <td className="px-6 py-4">
                <p className="text-sm text-slate-600">{user.email}</p>
              </td>
              <td className="px-6 py-4">
                <Pill className={roleStyles[user.role]}>
                  {user.role}
                </Pill>
              </td>
              <td className="px-6 py-4">
                <p className="text-sm text-slate-600">
                  {user.department || '—'}
                </p>
              </td>
              <td className="px-6 py-4">
                <Pill
                  className={
                    user.isActive
                      ? 'bg-emerald-50 text-emerald-700'
                      : 'bg-slate-100 text-slate-600'
                  }
                >
                  {user.isActive ? 'Active' : 'Inactive'}
                </Pill>
              </td>
              <td className="px-6 py-4 text-right">
                <div className="flex justify-end gap-2">
                  <button
                    onClick={() => onEdit(user)}
                    className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-medium text-slate-600 transition hover:border-brand-400 hover:text-brand-600"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => onDelete(user)}
                    className="rounded-lg border border-red-200 px-3 py-1 text-xs font-medium text-red-600 transition hover:border-red-400 hover:bg-red-50"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
