import { useState, useMemo, type FormEvent } from "react";
import axios from "axios";
import type { AuthUser } from "../auth/authStorage";
import Pill from "../components/ui/Pill";
import {
  useAdminUsers,
  useUpdateAdminUserRole,
  useUpdateAdminUserStatus,
} from "../hooks/useAdminUsers";
import {
  useDocumentTypes,
  useDocumentCategories,
  useCreateDocumentType,
  useUpdateDocumentType,
  useDeleteDocumentType,
} from "../hooks/useDocumentCatalog";

interface AdminDashboardProps {
  authUser: AuthUser;
  onLogout: () => void;
}

interface DocumentTypeFormState {
  name: string;
  description: string;
  categoryId: string;
  requiresApproval: boolean;
}

const initialFormState: DocumentTypeFormState = {
  name: "",
  description: "",
  categoryId: "",
  requiresApproval: true,
};

const fieldClass =
  "w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-2xs outline-none transition placeholder:text-muted-foreground focus:border-primary/60 focus:ring-2 focus:ring-primary/15";

const labelClass = "text-xs font-semibold uppercase text-muted-foreground";

const getErrorMessage = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data === "string") {
      return data;
    }
  }
  return error instanceof Error ? error.message : "Request failed.";
};

const AdminDashboard = ({ authUser, onLogout }: AdminDashboardProps) => {
  const [activeTab, setActiveTab] = useState<"users" | "document-types">(
    "users",
  );
  const [form, setForm] = useState<DocumentTypeFormState>(initialFormState);
  const [editingTypeId, setEditingTypeId] = useState<string | null>(null);
  const [formMessage, setFormMessage] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [adminMessage, setAdminMessage] = useState<string | null>(null);
  const [adminError, setAdminError] = useState<string | null>(null);

  const adminUsersQuery = useAdminUsers();
  const documentTypesQuery = useDocumentTypes();
  const categoriesQuery = useDocumentCategories();
  const createDocType = useCreateDocumentType();
  const updateDocType = useUpdateDocumentType();
  const deleteDocType = useDeleteDocumentType();
  const updateUserRole = useUpdateAdminUserRole();
  const updateUserStatus = useUpdateAdminUserStatus();

  const categoryOptions = useMemo(
    () => categoriesQuery.data ?? [],
    [categoriesQuery.data],
  );

  const updateField = (
    field: keyof DocumentTypeFormState,
    value: string | boolean,
  ) => {
    setForm((current) => ({ ...current, [field]: value }));
  };

  const handleDocumentTypeSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();
    setFormMessage(null);
    setFormError(null);

    try {
      if (editingTypeId) {
        await updateDocType.mutateAsync({
          id: editingTypeId,
          data: form,
        });
        setFormMessage("Document type updated successfully.");
        setEditingTypeId(null);
      } else {
        await createDocType.mutateAsync(form);
        setFormMessage("Document type created successfully.");
      }
      setForm(initialFormState);
    } catch (error) {
      setFormError(getErrorMessage(error));
    }
  };

  const handleDeleteDocumentType = async (id: string) => {
    setFormMessage(null);
    setFormError(null);

    if (!confirm("Are you sure you want to delete this document type?")) {
      return;
    }

    try {
      await deleteDocType.mutateAsync(id);
      setFormMessage("Document type deleted successfully.");
    } catch (error) {
      setFormError(getErrorMessage(error));
    }
  };

  const handleEditDocumentType = (id: string) => {
    const docType = documentTypesQuery.data?.find((dt) => dt.id === id);
    if (docType) {
      setForm({
        name: docType.name,
        description: docType.description,
        categoryId: docType.categoryId,
        requiresApproval: docType.requiresApproval,
      });
      setEditingTypeId(id);
    }
  };

  const handleCancelEdit = () => {
    setForm(initialFormState);
    setEditingTypeId(null);
    setFormMessage(null);
    setFormError(null);
  };

  const handleRoleChange = async (id: string, role: string) => {
    setAdminMessage(null);
    setAdminError(null);

    try {
      await updateUserRole.mutateAsync({ id, role });
      setAdminMessage("Role updated successfully.");
    } catch (error) {
      setAdminError(getErrorMessage(error));
    }
  };

  const handleStatusChange = async (id: string, isActive: boolean) => {
    setAdminMessage(null);
    setAdminError(null);

    try {
      await updateUserStatus.mutateAsync({ id, isActive });
      setAdminMessage(isActive ? "User activated." : "User deactivated.");
    } catch (error) {
      setAdminError(getErrorMessage(error));
    }
  };

  return (
    <div className="min-h-screen bg-muted/40 pb-16">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:px-6 lg:px-8">
        <header className="rounded-lg border border-border/60 bg-card/80 px-8 py-10 shadow-sm backdrop-blur">
          <p className="text-xs font-medium uppercase tracking-[0.35em] text-muted-foreground">
            Administration
          </p>
          <div className="mt-6 flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-3">
              <h1 className="text-3xl font-semibold text-foreground sm:text-4xl">
                Admin Management
              </h1>
              <p className="max-w-2xl text-base text-muted-foreground">
                Manage users and document types to keep your document approval
                workflow running smoothly.
              </p>
            </div>
            <div className="flex flex-wrap gap-3">
              <div className="rounded-md border border-border/60 bg-background/70 px-4 py-2 text-sm font-medium text-foreground/80">
                {authUser.fullName} · {authUser.role}
              </div>
              <button
                type="button"
                onClick={onLogout}
                className="rounded-md border border-border/60 bg-background/80 px-5 py-2 text-sm font-semibold text-foreground/80 transition hover:border-primary/40 hover:text-foreground"
              >
                Sign out
              </button>
            </div>
          </div>
        </header>

        <div className="rounded-lg border border-border/60 bg-card shadow-2xs">
          <div className="flex border-b border-border/60">
            <button
              onClick={() => setActiveTab("users")}
              className={`flex-1 px-6 py-4 text-sm font-semibold transition ${
                activeTab === "users"
                  ? "border-b-2 border-primary text-primary"
                  : "text-muted-foreground hover:text-foreground"
              }`}
            >
              User Management
            </button>
            <button
              onClick={() => setActiveTab("document-types")}
              className={`flex-1 px-6 py-4 text-sm font-semibold transition ${
                activeTab === "document-types"
                  ? "border-b-2 border-primary text-primary"
                  : "text-muted-foreground hover:text-foreground"
              }`}
            >
              Document Types
            </button>
          </div>

          <div className="p-6">
            {activeTab === "users" && (
              <section className="space-y-6">
                <div>
                  <h2 className="text-lg font-semibold text-foreground">
                    User Management
                  </h2>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Manage user roles and active status.
                  </p>
                </div>

                {adminError && (
                  <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                    {adminError}
                  </p>
                )}
                {adminMessage && (
                  <p className="rounded-md bg-emerald-100 px-3 py-2 text-sm text-emerald-700">
                    {adminMessage}
                  </p>
                )}

                {adminUsersQuery.isLoading && (
                  <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                    Loading users...
                  </p>
                )}

                {!adminUsersQuery.isLoading &&
                  adminUsersQuery.isError && (
                    <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                      {getErrorMessage(adminUsersQuery.error)}
                    </p>
                  )}

                {!adminUsersQuery.isLoading &&
                  (adminUsersQuery.data?.length ?? 0) === 0 && (
                    <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                      No users found.
                    </p>
                  )}

                {adminUsersQuery.data && adminUsersQuery.data.length > 0 && (
                  <div className="overflow-x-auto rounded-lg border border-border/60">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-border/60 bg-muted/40">
                          <th className="px-4 py-3 text-left font-semibold text-foreground">
                            Name
                          </th>
                          <th className="px-4 py-3 text-left font-semibold text-foreground">
                            Email
                          </th>
                          <th className="px-4 py-3 text-left font-semibold text-foreground">
                            Role
                          </th>
                          <th className="px-4 py-3 text-left font-semibold text-foreground">
                            Status
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        {adminUsersQuery.data.map((user) => (
                          <tr
                            key={user.id}
                            className="border-b border-border/60 transition hover:bg-muted/20"
                          >
                            <td className="px-4 py-3">
                              <p className="font-medium text-foreground">
                                {user.fullName}
                              </p>
                            </td>
                            <td className="px-4 py-3">
                              <p className="text-muted-foreground">
                                {user.email}
                              </p>
                            </td>
                            <td className="px-4 py-3">
                              <select
                                className="rounded-md border border-border/60 bg-background px-2 py-1 text-xs font-semibold text-foreground/80"
                                value={user.role}
                                onChange={(event) =>
                                  handleRoleChange(user.id, event.target.value)
                                }
                              >
                                <option value="Employee">Employee</option>
                                <option value="Approver">Approver</option>
                                <option value="Admin">Admin</option>
                              </select>
                            </td>
                            <td className="px-4 py-3">
                              <button
                                type="button"
                                onClick={() =>
                                  handleStatusChange(user.id, !user.isActive)
                                }
                                className={`rounded-md border px-3 py-1 text-xs font-semibold transition ${
                                  user.isActive
                                    ? "border-emerald-200 bg-emerald-50 text-emerald-700 hover:border-emerald-300"
                                    : "border-rose-200 bg-rose-50 text-rose-700 hover:border-rose-300"
                                }`}
                              >
                                {user.isActive ? "Active" : "Inactive"}
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </section>
            )}

            {activeTab === "document-types" && (
              <section className="space-y-6">
                <div className="grid gap-6 lg:grid-cols-3">
                  <div className="lg:col-span-1">
                    <div className="rounded-lg border border-border/60 bg-background/40 p-6">
                      <h3 className="font-semibold text-foreground">
                        {editingTypeId
                          ? "Edit Document Type"
                          : "Create Document Type"}
                      </h3>
                      <form
                        onSubmit={handleDocumentTypeSubmit}
                        className="mt-4 space-y-4"
                      >
                        <div>
                          <label className={labelClass}>Name</label>
                          <input
                            type="text"
                            className={fieldClass}
                            value={form.name}
                            onChange={(e) =>
                              updateField("name", e.target.value)
                            }
                            placeholder="Enter document type name"
                            required
                          />
                        </div>

                        <div>
                          <label className={labelClass}>Description</label>
                          <textarea
                            className={fieldClass}
                            rows={3}
                            value={form.description}
                            onChange={(e) =>
                              updateField("description", e.target.value)
                            }
                            placeholder="Enter description"
                          />
                        </div>

                        <div>
                          <label className={labelClass}>Category</label>
                          <select
                            className={fieldClass}
                            value={form.categoryId}
                            onChange={(e) =>
                              updateField("categoryId", e.target.value)
                            }
                            required
                          >
                            <option value="">Select a category</option>
                            {categoryOptions.map((category) => (
                              <option key={category.id} value={category.id}>
                                {category.name}
                              </option>
                            ))}
                          </select>
                        </div>

                        <div className="flex items-center gap-3">
                          <input
                            type="checkbox"
                            id="requires-approval"
                            className="h-4 w-4 rounded border-border/60"
                            checked={form.requiresApproval}
                            onChange={(e) =>
                              updateField("requiresApproval", e.target.checked)
                            }
                          />
                          <label
                            htmlFor="requires-approval"
                            className={`${labelClass} cursor-pointer`}
                          >
                            Requires Approval
                          </label>
                        </div>

                        {formError && (
                          <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                            {formError}
                          </p>
                        )}
                        {formMessage && (
                          <p className="rounded-md bg-emerald-100 px-3 py-2 text-sm text-emerald-700">
                            {formMessage}
                          </p>
                        )}

                        <div className="flex gap-2">
                          <button
                            type="submit"
                            disabled={
                              createDocType.isPending ||
                              updateDocType.isPending
                            }
                            className="flex-1 rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                          >
                            {createDocType.isPending ||
                            updateDocType.isPending
                              ? "Saving..."
                              : editingTypeId
                                ? "Update"
                                : "Create"}
                          </button>
                          {editingTypeId && (
                            <button
                              type="button"
                              onClick={handleCancelEdit}
                              className="flex-1 rounded-md border border-border/60 px-3 py-2 text-sm font-semibold text-muted-foreground transition hover:text-foreground"
                            >
                              Cancel
                            </button>
                          )}
                        </div>
                      </form>
                    </div>
                  </div>

                  <div className="lg:col-span-2">
                    <div className="space-y-3">
                      <div>
                        <h3 className="font-semibold text-foreground">
                          Document Types
                        </h3>
                        <p className="mt-1 text-sm text-muted-foreground">
                          Manage available document types in the system.
                        </p>
                      </div>

                      {documentTypesQuery.isLoading && (
                        <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                          Loading document types...
                        </p>
                      )}

                      {!documentTypesQuery.isLoading &&
                        (documentTypesQuery.data?.length ?? 0) === 0 && (
                          <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                            No document types found.
                          </p>
                        )}

                      {documentTypesQuery.data &&
                        documentTypesQuery.data.length > 0 && (
                          <div className="space-y-2">
                            {documentTypesQuery.data.map((docType) => (
                              <article
                                key={docType.id}
                                className="rounded-lg border border-border/60 bg-background/40 p-4"
                              >
                                <div className="flex items-start justify-between gap-4">
                                  <div>
                                    <h4 className="font-semibold text-foreground">
                                      {docType.name}
                                    </h4>
                                    <p className="mt-1 text-sm text-muted-foreground">
                                      {docType.description}
                                    </p>
                                    <div className="mt-2 flex flex-wrap gap-2">
                                      <Pill className="bg-secondary/20 text-secondary">
                                        {docType.categoryName}
                                      </Pill>
                                      {docType.requiresApproval && (
                                        <Pill className="bg-sky-100 text-sky-700">
                                          Requires Approval
                                        </Pill>
                                      )}
                                    </div>
                                  </div>
                                  <div className="flex gap-2">
                                    <button
                                      type="button"
                                      onClick={() =>
                                        handleEditDocumentType(docType.id)
                                      }
                                      className="rounded-md border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-primary"
                                    >
                                      Edit
                                    </button>
                                    <button
                                      type="button"
                                      onClick={() =>
                                        handleDeleteDocumentType(docType.id)
                                      }
                                      disabled={deleteDocType.isPending}
                                      className="rounded-md border border-destructive/40 px-3 py-1 text-xs font-semibold text-destructive transition hover:border-destructive hover:bg-destructive/10 disabled:cursor-not-allowed disabled:opacity-60"
                                    >
                                      Delete
                                    </button>
                                  </div>
                                </div>
                              </article>
                            ))}
                          </div>
                        )}
                    </div>
                  </div>
                </div>
              </section>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
