import { useState } from 'react';
import type { User } from '../api/admin';

interface UserFormModalProps {
  isOpen: boolean;
  user?: User;
  onClose: () => void;
  onSubmit: (data: {
    fullName: string;
    email: string;
    password?: string;
    role: 'Admin' | 'Employee' | 'Approver';
    department?: string;
  }) => Promise<void>;
  isLoading?: boolean;
}

export const UserFormModal = ({
  isOpen,
  user,
  onClose,
  onSubmit,
  isLoading = false,
}: UserFormModalProps) => {
  const [formData, setFormData] = useState({
    fullName: user?.fullName || '',
    email: user?.email || '',
    password: '',
    role: (user?.role || 'Employee') as 'Admin' | 'Employee' | 'Approver',
    department: user?.department || '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const newErrors: Record<string, string> = {};
    if (!formData.fullName.trim()) newErrors.fullName = 'Full name is required';
    if (!formData.email.trim()) newErrors.email = 'Email is required';
    if (!user && !formData.password) newErrors.password = 'Password is required for new users';
    if (!formData.role) newErrors.role = 'Role is required';
    return newErrors;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const newErrors = validate();
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    try {
      await onSubmit(formData);
      setFormData({
        fullName: '',
        email: '',
        password: '',
        role: 'Employee',
        department: '',
      });
      setErrors({});
      onClose();
    } catch (err) {
      // Error is handled by the parent component
    }
  };

  if (!isOpen) return null;

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="w-full max-w-md rounded-2xl bg-white shadow-lg ring-1 ring-slate-900/5">
          <div className="border-b border-slate-200 px-6 py-4">
            <h2 className="text-lg font-semibold text-slate-900">
              {user ? 'Edit User' : 'Create New User'}
            </h2>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4 px-6 py-4">
            <div>
              <label className="block text-sm font-medium text-slate-700">
                Full Name
              </label>
              <input
                type="text"
                value={formData.fullName}
                onChange={(e) =>
                  setFormData({ ...formData, fullName: e.target.value })
                }
                className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder-slate-400 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                placeholder="John Doe"
              />
              {errors.fullName && (
                <p className="mt-1 text-xs text-red-600">{errors.fullName}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700">
                Email
              </label>
              <input
                type="email"
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
                className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder-slate-400 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                placeholder="john@example.com"
              />
              {errors.email && (
                <p className="mt-1 text-xs text-red-600">{errors.email}</p>
              )}
            </div>

            {!user && (
              <div>
                <label className="block text-sm font-medium text-slate-700">
                  Password
                </label>
                <input
                  type="password"
                  value={formData.password}
                  onChange={(e) =>
                    setFormData({ ...formData, password: e.target.value })
                  }
                  className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder-slate-400 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                  placeholder="••••••••"
                />
                {errors.password && (
                  <p className="mt-1 text-xs text-red-600">{errors.password}</p>
                )}
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-slate-700">
                Role
              </label>
              <select
                value={formData.role}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    role: e.target.value as 'Admin' | 'Employee' | 'Approver',
                  })
                }
                className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
              >
                <option value="Employee">Employee</option>
                <option value="Approver">Approver</option>
                <option value="Admin">Admin</option>
              </select>
              {errors.role && (
                <p className="mt-1 text-xs text-red-600">{errors.role}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700">
                Department (Optional)
              </label>
              <input
                type="text"
                value={formData.department}
                onChange={(e) =>
                  setFormData({ ...formData, department: e.target.value })
                }
                className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder-slate-400 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                placeholder="Engineering"
              />
            </div>

            <div className="flex gap-3 border-t border-slate-200 pt-4">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isLoading}
                className="flex-1 rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-brand-700 disabled:opacity-50"
              >
                {isLoading ? 'Saving...' : user ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </>
  );
};
