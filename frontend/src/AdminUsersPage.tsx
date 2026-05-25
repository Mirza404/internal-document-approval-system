import { useState } from 'react';
import { useUsers } from './hooks/useUsers';
import { UserTable } from './components/UserTable';
import { UserFormModal } from './components/UserFormModal';
import type { User } from './api/admin';

export const AdminUsersPage = () => {
  const { users, isLoading, isCreating, isUpdating, isDeleting, createUser, updateUser, deleteUser, error } = useUsers();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | undefined>();
  const [deleteConfirm, setDeleteConfirm] = useState<User | null>(null);
  const [showError, setShowError] = useState<string | null>(null);

  const handleEdit = (user: User) => {
    setSelectedUser(user);
    setIsFormOpen(true);
  };

  const handleDelete = (user: User) => {
    setDeleteConfirm(user);
  };

  const confirmDelete = async () => {
    if (deleteConfirm) {
      try {
        await deleteUser(deleteConfirm.id);
        setDeleteConfirm(null);
      } catch (err) {
        setShowError(error || 'Failed to delete user');
      }
    }
  };

  const handleFormSubmit = async (data: any) => {
    try {
      if (selectedUser) {
        await updateUser(selectedUser.id, data);
      } else {
        await createUser(data);
      }
      setSelectedUser(undefined);
      setIsFormOpen(false);
    } catch (err) {
      setShowError(error || 'Failed to save user');
    }
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setSelectedUser(undefined);
  };

  return (
    <div className="min-h-screen bg-slate-100 pb-16">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:px-6 lg:px-8">
        <header className="rounded-3xl bg-gradient-to-br from-white via-white to-slate-50 px-8 py-10 shadow-sm ring-1 ring-slate-900/5">
          <p className="text-xs font-medium uppercase tracking-[0.35em] text-slate-500">
            Administration
          </p>
          <div className="mt-6 flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-3">
              <h1 className="text-3xl font-semibold text-slate-900 sm:text-4xl">
                User Management
              </h1>
              <p className="max-w-2xl text-base text-slate-600">
                Create and manage user accounts. Assign roles, departments, and
                control access permissions.
              </p>
            </div>
            <button
              onClick={() => {
                setSelectedUser(undefined);
                setIsFormOpen(true);
              }}
              className="rounded-full bg-brand-600 px-5 py-2 text-sm font-semibold text-white shadow-card transition hover:bg-brand-700"
            >
              + Create User
            </button>
          </div>
        </header>

        {showError && (
          <div className="rounded-2xl bg-red-50 p-4 text-sm text-red-700 ring-1 ring-red-200">
            {showError}
            <button
              onClick={() => setShowError(null)}
              className="ml-auto text-xs font-semibold text-red-600 hover:text-red-700"
            >
              Dismiss
            </button>
          </div>
        )}

        <UserTable
          users={users}
          isLoading={isLoading}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />

        <UserFormModal
          isOpen={isFormOpen}
          user={selectedUser}
          onClose={handleCloseForm}
          onSubmit={handleFormSubmit}
          isLoading={isCreating || isUpdating}
        />

        {deleteConfirm && (
          <>
            <div className="fixed inset-0 z-40 bg-black/50" />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
              <div className="w-full max-w-sm rounded-2xl bg-white shadow-lg ring-1 ring-slate-900/5">
                <div className="border-b border-slate-200 px-6 py-4">
                  <h2 className="text-lg font-semibold text-slate-900">
                    Delete User
                  </h2>
                </div>
                <div className="px-6 py-4">
                  <p className="text-sm text-slate-600">
                    Are you sure you want to delete{' '}
                    <strong>{deleteConfirm.fullName}</strong>? This action cannot
                    be undone.
                  </p>
                </div>
                <div className="flex gap-3 border-t border-slate-200 px-6 py-4">
                  <button
                    onClick={() => setDeleteConfirm(null)}
                    className="flex-1 rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={confirmDelete}
                    disabled={isDeleting}
                    className="flex-1 rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700 disabled:opacity-50"
                  >
                    {isDeleting ? 'Deleting...' : 'Delete'}
                  </button>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};
