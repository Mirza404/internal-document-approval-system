import type { DocumentType } from '../api/admin';

interface DocTypeTableProps {
  documentTypes: DocumentType[];
  isLoading?: boolean;
  onEdit: (docType: DocumentType) => void;
  onDelete: (docType: DocumentType) => void;
}

export const DocTypeTable = ({
  documentTypes,
  isLoading = false,
  onEdit,
  onDelete,
}: DocTypeTableProps) => {
  if (isLoading) {
    return (
      <div className="flex h-32 items-center justify-center rounded-2xl bg-white ring-1 ring-slate-900/5">
        <p className="text-sm text-slate-500">Loading document types...</p>
      </div>
    );
  }

  if (documentTypes.length === 0) {
    return (
      <div className="flex h-32 flex-col items-center justify-center rounded-2xl bg-white ring-1 ring-slate-900/5">
        <p className="text-sm text-slate-500">No document types found</p>
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
              Description
            </th>
            <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Created
            </th>
            <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
              Actions
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {documentTypes.map((docType) => (
            <tr
              key={docType.id}
              className="transition hover:bg-slate-50"
            >
              <td className="px-6 py-4">
                <p className="font-medium text-slate-900">{docType.name}</p>
              </td>
              <td className="px-6 py-4">
                <p className="text-sm text-slate-600">
                  {docType.description || '—'}
                </p>
              </td>
              <td className="px-6 py-4">
                <p className="text-sm text-slate-600">
                  {new Date(docType.createdAt).toLocaleDateString()}
                </p>
              </td>
              <td className="px-6 py-4 text-right">
                <div className="flex justify-end gap-2">
                  <button
                    onClick={() => onEdit(docType)}
                    className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-medium text-slate-600 transition hover:border-brand-400 hover:text-brand-600"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => onDelete(docType)}
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
