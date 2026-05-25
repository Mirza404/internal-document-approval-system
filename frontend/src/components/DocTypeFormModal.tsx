import { useState } from 'react';
import type { DocumentType } from '../api/admin';

interface DocTypeFormModalProps {
  isOpen: boolean;
  docType?: DocumentType;
  onClose: () => void;
  onSubmit: (data: {
    name: string;
    description?: string;
  }) => Promise<void>;
  isLoading?: boolean;
}

export const DocTypeFormModal = ({
  isOpen,
  docType,
  onClose,
  onSubmit,
  isLoading = false,
}: DocTypeFormModalProps) => {
  const [formData, setFormData] = useState({
    name: docType?.name || '',
    description: docType?.description || '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const newErrors: Record<string, string> = {};
    if (!formData.name.trim()) newErrors.name = 'Name is required';
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
        name: '',
        description: '',
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
              {docType ? 'Edit Document Type' : 'Create New Document Type'}
            </h2>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4 px-6 py-4">
            <div>
              <label className="block text-sm font-medium text-slate-700">
                Name
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
                className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder-slate-400 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                placeholder="Leave Request"
              />
              {errors.name && (
                <p className="mt-1 text-xs text-red-600">{errors.name}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700">
                Description (Optional)
              </label>
              <textarea
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder-slate-400 transition focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
                placeholder="Description for this document type..."
                rows={3}
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
                {isLoading ? 'Saving...' : docType ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </>
  );
};
