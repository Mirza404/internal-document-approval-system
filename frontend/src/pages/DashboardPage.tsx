import { useMemo, useState, type FormEvent } from "react";
import axios from "axios";
import type { AuthUser } from "../auth/authStorage";
import Pill from "../components/ui/Pill";
import { activityFeed } from "../mockData/activityFeed";
import { stats } from "../mockData/stats";
import { automations } from "../mockData/automations";
import { useDocumentTypes } from "../hooks/useDocumentCatalog";
import {
  useAdminUsers,
  useUpdateAdminUserRole,
  useUpdateAdminUserStatus,
} from "../hooks/useAdminUsers";
import {
  useCreateDocument,
  useDocuments,
  useUpdateDocument,
} from "../hooks/useDocuments";
import { usePendingApprovals } from "../hooks/useApprovals";
import type {
  ApprovalHistoryItem,
  CreateDocumentRequest,
  Document,
  UpdateDocumentRequest,
} from "../api/documents";
import { getApprovalHistory } from "../api/documents";

interface DashboardPageProps {
  authUser: AuthUser;
  onLogout: () => void;
}

interface SubmissionFormState {
  title: string;
  description: string;
  documentTypeId: string;
  priority: string;
  leaveType: string;
  leaveStartDate: string;
  leaveEndDate: string;
  amount: string;
  budgetCode: string;
  counterparty: string;
  attachmentNote: string;
}

const initialFormState: SubmissionFormState = {
  title: "",
  description: "",
  documentTypeId: "",
  priority: "Normal",
  leaveType: "",
  leaveStartDate: "",
  leaveEndDate: "",
  amount: "",
  budgetCode: "",
  counterparty: "",
  attachmentNote: "",
};

const statusClasses: Record<string, string> = {
  Draft: "bg-muted text-muted-foreground",
  PendingApproval: "bg-sky-100 text-sky-700",
  UnderReview: "bg-indigo-100 text-indigo-700",
  ChangesRequested: "bg-amber-100 text-amber-800",
  Approved: "bg-emerald-100 text-emerald-700",
  Rejected: "bg-rose-100 text-rose-700",
};

const fieldClass =
  "w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-2xs outline-none transition placeholder:text-muted-foreground focus:border-primary/60 focus:ring-2 focus:ring-primary/15";

const labelClass = "text-xs font-semibold uppercase text-muted-foreground";

const statusLabels: Record<string, string> = {
  PendingApproval: "Pending Approval",
  UnderReview: "Under Review",
  ChangesRequested: "Changes Requested",
};

const formatDocumentStatus = (status: string) => statusLabels[status] ?? status;

const formatDate = (value?: string | null) => {
  if (!value) {
    return "Not set";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
  }).format(new Date(value));
};

const getDocumentDateValue = (document: Document) =>
  new Date(document.updatedAt ?? document.createdAt).getTime();

const getErrorMessage = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data === "string") {
      return data;
    }
  }

  return error instanceof Error ? error.message : "Request failed.";
};

const buildCreatePayload = (
  form: SubmissionFormState,
): CreateDocumentRequest => ({
  title: form.title,
  description: form.description || null,
  documentTypeId: form.documentTypeId || null,
  priority: form.priority || null,
  leaveType: form.leaveType || null,
  leaveStartDate: form.leaveStartDate || null,
  leaveEndDate: form.leaveEndDate || null,
  amount: form.amount ? Number(form.amount) : null,
  budgetCode: form.budgetCode || null,
  counterparty: form.counterparty || null,
  attachmentNote: form.attachmentNote || null,
});

const buildUpdatePayload = (
  form: SubmissionFormState,
): UpdateDocumentRequest => ({
  ...buildCreatePayload(form),
});

const buildFormFromDocument = (document: Document): SubmissionFormState => ({
  title: document.title,
  description: document.description,
  documentTypeId: document.documentTypeId,
  priority: document.priority,
  leaveType: document.leaveType ?? "",
  leaveStartDate: document.leaveStartDate ?? "",
  leaveEndDate: document.leaveEndDate ?? "",
  amount: document.amount == null ? "" : String(document.amount),
  budgetCode: document.budgetCode ?? "",
  counterparty: document.counterparty ?? "",
  attachmentNote: document.attachmentNote ?? "",
});

const clearDocumentMetadata = (
  state: SubmissionFormState,
): SubmissionFormState => ({
  ...state,
  leaveType: "",
  leaveStartDate: "",
  leaveEndDate: "",
  amount: "",
  budgetCode: "",
  counterparty: "",
  attachmentNote: "",
});

const getDocumentMetadataKind = (documentType?: {
  categoryName: string;
  name: string;
}) => {
  const typeName = documentType?.name.trim().toLowerCase();
  const categoryName = documentType?.categoryName.trim().toLowerCase();

  if (typeName === "payment procedure" || categoryName === "payments") {
    return "payment";
  }

  if (typeName === "internship submission" || categoryName === "internships") {
    return "internship";
  }

  if (categoryName === "hr") {
    return "leave";
  }

  if (categoryName === "finance") {
    return "payment";
  }

  if (categoryName === "contract") {
    return "internship";
  }

  return "none";
};

const EmployeeDashboard = ({ authUser, onLogout }: DashboardPageProps) => {
  const [form, setForm] = useState<SubmissionFormState>(initialFormState);
  const [resubmitForm, setResubmitForm] = useState<SubmissionFormState | null>(
    null,
  );
  const [resubmitNotes, setResubmitNotes] = useState("");
  const [formMessage, setFormMessage] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [resubmitMessage, setResubmitMessage] = useState<string | null>(null);
  const [resubmitError, setResubmitError] = useState<string | null>(null);
  const [approvalHistory, setApprovalHistory] = useState<
    Record<string, ApprovalHistoryItem[]>
  >({});

  const documentTypesQuery = useDocumentTypes();
  const documentsQuery = useDocuments();
  const createDocument = useCreateDocument();
  const updateDocument = useUpdateDocument();
  const documentTypeOptions = useMemo(
    () => documentTypesQuery.data ?? [],
    [documentTypesQuery.data],
  );

  const documentTypeLabels = useMemo(
    () =>
      new Map(
        documentTypeOptions.map((type) => [
          type.id,
          [type.categoryName, type.name].filter(Boolean).join(" · "),
        ]),
      ),
    [documentTypeOptions],
  );

  const selectedType = useMemo(
    () => documentTypeOptions.find((type) => type.id === form.documentTypeId),
    [documentTypeOptions, form.documentTypeId],
  );

  const resubmitType = useMemo(
    () =>
      documentTypeOptions.find(
        (type) => type.id === resubmitForm?.documentTypeId,
      ),
    [documentTypeOptions, resubmitForm?.documentTypeId],
  );

  const myDocuments = useMemo(
    () =>
      (documentsQuery.data ?? []).filter(
        (document) => document.createdByUserId === authUser.userId,
      ),
    [authUser.userId, documentsQuery.data],
  );
  const sortedDocuments = useMemo(
    () =>
      [...myDocuments].sort(
        (left, right) =>
          getDocumentDateValue(right) - getDocumentDateValue(left),
      ),
    [myDocuments],
  );
  const latestDocument = sortedDocuments[0];

  const pendingCount = myDocuments.filter(
    (document) => document.status === "PendingApproval",
  ).length;
  const changesCount = myDocuments.filter(
    (document) => document.status === "ChangesRequested",
  ).length;
  const approvedCount = myDocuments.filter(
    (document) => document.status === "Approved",
  ).length;

  const updateField = (field: keyof SubmissionFormState, value: string) => {
    setForm((current) => {
      const next = { ...current, [field]: value };
      return field === "documentTypeId" ? clearDocumentMetadata(next) : next;
    });
  };

  const updateResubmitField = (
    field: keyof SubmissionFormState,
    value: string,
  ) => {
    setResubmitForm((current) =>
      current
        ? field === "documentTypeId"
          ? clearDocumentMetadata({ ...current, [field]: value })
          : { ...current, [field]: value }
        : current,
    );
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFormMessage(null);
    setFormError(null);

    try {
      await createDocument.mutateAsync(buildCreatePayload(form));
      setForm(initialFormState);
      setFormMessage("Submitted for approval.");
    } catch (error) {
      setFormError(getErrorMessage(error));
    }
  };

  const handleStartResubmit = (document: Document) => {
    setResubmitForm(buildFormFromDocument(document));
    setResubmitNotes("");
    setResubmitMessage(null);
    setResubmitError(null);
  };

  const handleResubmit = async (
    event: FormEvent<HTMLFormElement>,
    document: Document,
  ) => {
    event.preventDefault();
    if (!resubmitForm) {
      return;
    }

    setResubmitMessage(null);
    setResubmitError(null);

    try {
      await updateDocument.mutateAsync({
        id: document.id,
        data: {
          ...buildUpdatePayload(resubmitForm),
          status: "PendingApproval",
        },
      });
      setResubmitForm(null);
      setResubmitNotes("");
      setResubmitMessage("Document resubmitted.");
    } catch (error) {
      setResubmitError(getErrorMessage(error));
    }
  };

  const loadDocumentApprovalHistory = async (documentId: string) => {
    if (approvalHistory[documentId]) {
      return;
    }

    try {
      const result = await getApprovalHistory(documentId);
      setApprovalHistory((current) => ({
        ...current,
        [documentId]: result,
      }));
    } catch {
      setApprovalHistory((current) => ({
        ...current,
        [documentId]: [],
      }));
    }
  };

  const renderCategoryFields = (
    state: SubmissionFormState,
    update: (field: keyof SubmissionFormState, value: string) => void,
    documentType = selectedType,
  ) => {
    const metadataKind = getDocumentMetadataKind(documentType);

    if (metadataKind === "leave") {
      return (
        <div className="grid gap-4 md:grid-cols-3">
          <label className="space-y-2">
            <span className={labelClass}>Leave type</span>
            <input
              className={fieldClass}
              required
              value={state.leaveType}
              onChange={(event) => update("leaveType", event.target.value)}
            />
          </label>
          <label className="space-y-2">
            <span className={labelClass}>Start date</span>
            <input
              className={fieldClass}
              required
              type="date"
              value={state.leaveStartDate}
              onChange={(event) => update("leaveStartDate", event.target.value)}
            />
          </label>
          <label className="space-y-2">
            <span className={labelClass}>End date</span>
            <input
              className={fieldClass}
              required
              type="date"
              value={state.leaveEndDate}
              onChange={(event) => update("leaveEndDate", event.target.value)}
            />
          </label>
        </div>
      );
    }

    if (metadataKind === "payment") {
      return (
        <div className="grid gap-4 md:grid-cols-2">
          <label className="space-y-2">
            <span className={labelClass}>Amount</span>
            <input
              className={fieldClass}
              min="0"
              required
              step="0.01"
              type="number"
              value={state.amount}
              onChange={(event) => update("amount", event.target.value)}
            />
          </label>
          <label className="space-y-2">
            <span className={labelClass}>Payment reference</span>
            <input
              className={fieldClass}
              required
              value={state.budgetCode}
              onChange={(event) => update("budgetCode", event.target.value)}
            />
          </label>
        </div>
      );
    }

    if (metadataKind === "internship") {
      return (
        <div className="grid gap-4 md:grid-cols-2">
          <label className="space-y-2">
            <span className={labelClass}>Organization</span>
            <input
              className={fieldClass}
              value={state.counterparty}
              onChange={(event) => update("counterparty", event.target.value)}
            />
          </label>
          <label className="space-y-2">
            <span className={labelClass}>Supporting note</span>
            <input
              className={fieldClass}
              value={state.attachmentNote}
              onChange={(event) => update("attachmentNote", event.target.value)}
            />
          </label>
        </div>
      );
    }

    return null;
  };

  return (
    <div className="min-h-screen bg-muted/40 pb-10">
      <div className="mx-auto flex max-w-7xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-lg border border-border/60 bg-card px-5 py-4 shadow-2xs">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <p className="text-xs font-semibold uppercase text-muted-foreground">
                Employee workspace
              </p>
              <h1 className="mt-1 text-2xl font-semibold text-foreground">
                My document submissions
              </h1>
            </div>
            <div className="flex flex-wrap gap-2">
              <div className="rounded-md border border-border/60 bg-background px-3 py-2 text-sm text-foreground/80">
                {authUser.fullName} · {authUser.role}
              </div>
              <button
                type="button"
                onClick={onLogout}
                className="rounded-md border border-border/60 bg-background px-4 py-2 text-sm font-semibold text-foreground/80 transition hover:border-primary/40 hover:text-foreground"
              >
                Sign out
              </button>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          <div className="rounded-lg border border-border/60 bg-card px-5 py-4">
            <p className="text-sm text-muted-foreground">Pending approval</p>
            <p className="mt-2 text-3xl font-semibold">{pendingCount}</p>
          </div>
          <div className="rounded-lg border border-border/60 bg-card px-5 py-4">
            <p className="text-sm text-muted-foreground">Changes requested</p>
            <p className="mt-2 text-3xl font-semibold">{changesCount}</p>
          </div>
          <div className="rounded-lg border border-border/60 bg-card px-5 py-4">
            <p className="text-sm text-muted-foreground">Approved</p>
            <p className="mt-2 text-3xl font-semibold">{approvedCount}</p>
          </div>
        </section>

        <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_420px]">
          <div className="space-y-6">
            <section className="rounded-lg border border-border/60 bg-card p-5 shadow-2xs">
              <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-muted-foreground">
                    Latest submission
                  </p>
                  <h2 className="mt-1 truncate text-lg font-semibold text-foreground">
                    {latestDocument?.title ?? "No submissions yet"}
                  </h2>
                  {latestDocument && (
                    <p className="mt-1 text-sm text-muted-foreground">
                      {documentTypeLabels.get(latestDocument.documentTypeId) ||
                        "Document type not set"}
                    </p>
                  )}
                </div>
                {latestDocument && (
                  <Pill
                    className={
                      statusClasses[latestDocument.status] ??
                      "bg-muted text-muted-foreground"
                    }
                  >
                    {formatDocumentStatus(latestDocument.status)}
                  </Pill>
                )}
              </div>

              {documentsQuery.isLoading && (
                <p className="mt-4 rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Loading latest submission...
                </p>
              )}

              {!documentsQuery.isLoading && latestDocument && (
                <div className="mt-4 grid gap-3 text-sm sm:grid-cols-3">
                  <div>
                    <p className="text-xs font-semibold uppercase text-muted-foreground">
                      Priority
                    </p>
                    <p className="mt-1 font-medium text-foreground">
                      {latestDocument.priority}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs font-semibold uppercase text-muted-foreground">
                      Submitted
                    </p>
                    <p className="mt-1 font-medium text-foreground">
                      {formatDate(latestDocument.createdAt)}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs font-semibold uppercase text-muted-foreground">
                      Updated
                    </p>
                    <p className="mt-1 font-medium text-foreground">
                      {formatDate(
                        latestDocument.updatedAt ?? latestDocument.createdAt,
                      )}
                    </p>
                  </div>
                </div>
              )}

              {!documentsQuery.isLoading && !latestDocument && (
                <p className="mt-4 text-sm text-muted-foreground">
                  Submit a document to start building your history.
                </p>
              )}

              {latestDocument?.latestVersionNumber != null && (
                <p className="mt-4 rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Latest version{" "}
                  {latestDocument.latestVersionLabel ??
                    `v${latestDocument.latestVersionNumber}`}
                  {latestDocument.latestVersionCreatedAt
                    ? ` · ${formatDate(latestDocument.latestVersionCreatedAt)}`
                    : ""}
                  {latestDocument.latestVersionChangeNotes
                    ? ` · ${latestDocument.latestVersionChangeNotes}`
                    : ""}
                </p>
              )}

              {latestDocument?.status === "ChangesRequested" &&
                (resubmitForm ? (
                  <form
                    className="mt-5 space-y-4 rounded-lg border border-border/60 bg-background/50 p-4"
                    onSubmit={(event) => handleResubmit(event, latestDocument)}
                  >
                    <div className="flex items-center justify-between gap-3">
                      <h3 className="text-base font-semibold text-foreground">
                        Edit resubmission
                      </h3>
                      <button
                        type="button"
                        onClick={() => {
                          setResubmitForm(null);
                          setResubmitNotes("");
                          setResubmitError(null);
                        }}
                        className="rounded-md border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-foreground"
                      >
                        Cancel
                      </button>
                    </div>

                    <div className="grid gap-4 md:grid-cols-2">
                      <label className="space-y-2">
                        <span className={labelClass}>Document type</span>
                        <select
                          className={fieldClass}
                          required
                          value={resubmitForm.documentTypeId}
                          onChange={(event) =>
                            updateResubmitField(
                              "documentTypeId",
                              event.target.value,
                            )
                          }
                        >
                          <option value="">Select type</option>
                          {documentTypeOptions.map((type) => (
                            <option key={type.id} value={type.id}>
                              {type.categoryName} · {type.name}
                            </option>
                          ))}
                        </select>
                      </label>

                      <label className="space-y-2">
                        <span className={labelClass}>Priority</span>
                        <select
                          className={fieldClass}
                          value={resubmitForm.priority}
                          onChange={(event) =>
                            updateResubmitField("priority", event.target.value)
                          }
                        >
                          <option>Low</option>
                          <option>Normal</option>
                          <option>High</option>
                          <option>Urgent</option>
                        </select>
                      </label>
                    </div>

                    <label className="space-y-2">
                      <span className={labelClass}>Title</span>
                      <input
                        className={fieldClass}
                        required
                        value={resubmitForm.title}
                        onChange={(event) =>
                          updateResubmitField("title", event.target.value)
                        }
                      />
                    </label>

                    <label className="space-y-2">
                      <span className={labelClass}>Details</span>
                      <textarea
                        className={`${fieldClass} min-h-24 resize-y`}
                        value={resubmitForm.description}
                        onChange={(event) =>
                          updateResubmitField("description", event.target.value)
                        }
                      />
                    </label>

                    {renderCategoryFields(
                      resubmitForm,
                      updateResubmitField,
                      resubmitType,
                    )}

                    <label className="space-y-2">
                      <span className={labelClass}>Change notes</span>
                      <textarea
                        className={`${fieldClass} min-h-20 resize-y`}
                        value={resubmitNotes}
                        onChange={(event) =>
                          setResubmitNotes(event.target.value)
                        }
                      />
                    </label>

                    {(resubmitError || resubmitMessage) && (
                      <p
                        className={`rounded-md px-3 py-2 text-sm ${
                          resubmitError
                            ? "bg-destructive/10 text-destructive"
                            : "bg-emerald-100 text-emerald-700"
                        }`}
                      >
                        {resubmitError ?? resubmitMessage}
                      </p>
                    )}

                    <button
                      type="submit"
                      disabled={updateDocument.isPending}
                      className="w-full rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      {updateDocument.isPending
                        ? "Resubmitting..."
                        : "Submit changes"}
                    </button>
                  </form>
                ) : (
                  <div className="mt-4 space-y-3">
                    {(resubmitError || resubmitMessage) && (
                      <p
                        className={`rounded-md px-3 py-2 text-sm ${
                          resubmitError
                            ? "bg-destructive/10 text-destructive"
                            : "bg-emerald-100 text-emerald-700"
                        }`}
                      >
                        {resubmitError ?? resubmitMessage}
                      </p>
                    )}
                    <button
                      type="button"
                      onClick={() => handleStartResubmit(latestDocument)}
                      disabled={updateDocument.isPending}
                      className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      Edit and resubmit
                    </button>
                  </div>
                ))}
            </section>

            <form
              className="rounded-lg border border-border/60 bg-card p-5 shadow-2xs"
              onSubmit={handleSubmit}
            >
              <div className="flex flex-col gap-1">
                <p className="text-sm font-medium text-muted-foreground">
                  Submission
                </p>
                <h2 className="text-xl font-semibold text-foreground">
                  New document
                </h2>
              </div>

              <div className="mt-5 grid gap-4 md:grid-cols-2">
                <label className="space-y-2">
                  <span className={labelClass}>Document type</span>
                  <select
                    className={fieldClass}
                    required
                    value={form.documentTypeId}
                    onChange={(event) =>
                      updateField("documentTypeId", event.target.value)
                    }
                  >
                    <option value="">Select type</option>
                    {documentTypeOptions.map((type) => (
                      <option key={type.id} value={type.id}>
                        {type.categoryName} · {type.name}
                      </option>
                    ))}
                  </select>
                  {documentTypesQuery.isError && (
                    <span className="block text-xs font-medium text-destructive">
                      Showing default document types while catalog reloads.
                    </span>
                  )}
                </label>
                <label className="space-y-2">
                  <span className={labelClass}>Priority</span>
                  <select
                    className={fieldClass}
                    value={form.priority}
                    onChange={(event) =>
                      updateField("priority", event.target.value)
                    }
                  >
                    <option>Low</option>
                    <option>Normal</option>
                    <option>High</option>
                    <option>Urgent</option>
                  </select>
                </label>
              </div>

              <div className="mt-4 grid gap-4">
                <label className="space-y-2">
                  <span className={labelClass}>Title</span>
                  <input
                    className={fieldClass}
                    required
                    value={form.title}
                    onChange={(event) =>
                      updateField("title", event.target.value)
                    }
                  />
                </label>
                <label className="space-y-2">
                  <span className={labelClass}>Details</span>
                  <textarea
                    className={`${fieldClass} min-h-28 resize-y`}
                    value={form.description}
                    onChange={(event) =>
                      updateField("description", event.target.value)
                    }
                  />
                </label>
                {renderCategoryFields(form, updateField, selectedType)}
              </div>

              {(formError || formMessage) && (
                <p
                  className={`mt-4 rounded-md px-3 py-2 text-sm ${
                    formError
                      ? "bg-destructive/10 text-destructive"
                      : "bg-emerald-100 text-emerald-700"
                  }`}
                >
                  {formError ?? formMessage}
                </p>
              )}

              <div className="mt-5 flex justify-end">
                <button
                  type="submit"
                  disabled={createDocument.isPending}
                  className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {createDocument.isPending ? "Submitting..." : "Submit"}
                </button>
              </div>
            </form>
          </div>

          <aside className="rounded-lg border border-border/60 bg-card p-5 shadow-2xs">
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  My documents
                </p>
                <h2 className="mt-1 text-xl font-semibold text-foreground">
                  Submission history
                </h2>
              </div>
              {myDocuments.length > 0 && (
                <span className="rounded-md border border-border/60 px-2 py-1 text-xs font-semibold text-muted-foreground">
                  {myDocuments.length}
                </span>
              )}
            </div>

            <div className="mt-5 space-y-3">
              {documentsQuery.isLoading && (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Loading documents...
                </p>
              )}
              {!documentsQuery.isLoading && sortedDocuments.length === 0 && (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  No submissions yet.
                </p>
              )}
              {sortedDocuments.map((document) => {
                if (!approvalHistory[document.id]) {
                  void loadDocumentApprovalHistory(document.id);
                }

                return (
                  <article
                    key={document.id}
                    className="rounded-lg border border-border/60 bg-background/40 p-4"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className="min-w-0">
                        <h3 className="truncate text-sm font-semibold text-foreground">
                          {document.title}
                        </h3>
                        <p className="mt-1 text-xs text-muted-foreground">
                          {documentTypeLabels.get(document.documentTypeId) ||
                            "Document type not set"}
                        </p>
                      </div>
                      <Pill
                        className={
                          statusClasses[document.status] ??
                          "bg-muted text-muted-foreground"
                        }
                      >
                        {formatDocumentStatus(document.status)}
                      </Pill>
                    </div>
                    <div className="mt-3 flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
                      <span>{document.priority} priority</span>
                      <span>Submitted {formatDate(document.createdAt)}</span>
                      <span>
                        Updated{" "}
                        {formatDate(document.updatedAt ?? document.createdAt)}
                      </span>
                      {document.latestVersionNumber != null && (
                        <span>
                          {document.latestVersionLabel ??
                            `v${document.latestVersionNumber}`}
                        </span>
                      )}
                    </div>
                    {document.latestVersionChangeNotes && (
                      <p className="mt-3 line-clamp-2 text-sm text-muted-foreground">
                        {document.latestVersionChangeNotes}
                      </p>
                    )}

                    {approvalHistory[document.id]?.length ? (
                      <div className="mt-4 space-y-2 border-t border-border/50 pt-3">
                        <p className="text-xs font-semibold uppercase text-muted-foreground">
                          Approval history
                        </p>

                        {approvalHistory[document.id].map((item) => (
                          <div
                            key={item.id}
                            className="rounded-md bg-muted/40 px-3 py-2 text-xs"
                          >
                            <div className="flex items-center justify-between gap-2">
                              <span className="font-semibold text-foreground">
                                {item.approverFullName}
                              </span>

                              <span className="text-muted-foreground">
                                {new Date(item.createdAt).toLocaleString()}
                              </span>
                            </div>

                            <p className="mt-1 font-medium text-primary">
                              {formatDocumentStatus(item.status)}
                            </p>

                            {item.comments && (
                              <p className="mt-1 text-muted-foreground">
                                {item.comments}
                              </p>
                            )}
                          </div>
                        ))}
                      </div>
                    ) : null}
                  </article>
                );
              })}
            </div>
          </aside>
        </section>
      </div>
    </div>
  );
};

const MockDashboard = ({ authUser, onLogout }: DashboardPageProps) => {
  const pendingApprovalsQuery = usePendingApprovals();
  const [showUsersModal, setShowUsersModal] = useState(false);
  const [adminMessage, setAdminMessage] = useState<string | null>(null);
  const [adminError, setAdminError] = useState<string | null>(null);
  const isAdmin = authUser.role.toLowerCase() === "admin";
  const adminUsersQuery = useAdminUsers();
  const updateRole = useUpdateAdminUserRole();
  const updateStatus = useUpdateAdminUserStatus();

  const filteredQueue = useMemo(() => {
    return pendingApprovalsQuery.data ?? [];
  }, [pendingApprovalsQuery.data]);

  const handleRoleChange = async (id: string, role: string) => {
    setAdminMessage(null);
    setAdminError(null);

    try {
      await updateRole.mutateAsync({ id, role });
      setAdminMessage("Role updated.");
    } catch (error) {
      setAdminError(getErrorMessage(error));
    }
  };

  const handleStatusChange = async (id: string, isActive: boolean) => {
    setAdminMessage(null);
    setAdminError(null);

    try {
      await updateStatus.mutateAsync({ id, isActive });
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
              {isAdmin ? (
                <button
                  type="button"
                  onClick={() => setShowUsersModal(true)}
                  className="rounded-md border border-border/60 bg-card px-5 py-2 text-sm font-semibold text-foreground/70 transition hover:border-primary/40 hover:text-foreground"
                >
                  Manage users
                </button>
              ) : null}
              <button className="rounded-md bg-primary px-5 py-2 text-sm font-semibold text-primary-foreground shadow-md shadow-primary/20 transition hover:bg-primary/90">
                New approval flow
              </button>
              <button className="rounded-md border border-border/60 bg-card px-5 py-2 text-sm font-semibold text-foreground/70 transition hover:border-primary/40 hover:text-foreground">
                Share weekly digest
              </button>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          {stats.map((stat) => (
            <article
              key={stat.label}
              className="rounded-lg border border-border/60 bg-card px-6 py-5 shadow-2xs"
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
          <div className="rounded-lg border border-border/60 bg-card p-6 shadow-2xs lg:col-span-2">
            <div className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Review queue
                </p>
                <h2 className="text-xl font-semibold text-foreground">
                  Pending approval requests
                </h2>
              </div>
              <span className="rounded-md border border-border/60 px-2 py-1 text-xs font-semibold text-muted-foreground">
                {filteredQueue.length}
              </span>
            </div>

            <div className="mt-6 space-y-3">
              {pendingApprovalsQuery.isLoading && (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Loading approvals...
                </p>
              )}

              {!pendingApprovalsQuery.isLoading &&
                filteredQueue.length === 0 && (
                  <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                    No pending approval requests.
                  </p>
                )}

              {filteredQueue.map((item) => (
                <article
                  key={item.documentId}
                  className="flex flex-col gap-4 rounded-lg border border-border/60 bg-background/40 px-4 py-4 transition hover:border-primary/30 hover:bg-card/80 sm:flex-row sm:items-center"
                >
                  <div className="flex-1">
                    <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                      {item.documentTypeName}
                    </p>
                    <h3 className="mt-1 text-lg font-semibold text-foreground">
                      {item.title}
                    </h3>
                    <p className="text-sm text-muted-foreground">
                      Submitted by {item.creatorFullName}
                    </p>
                  </div>
                  <div className="flex flex-wrap items-center gap-3">
                    <Pill className="bg-sky-100 text-sky-700">
                      Pending Approval
                    </Pill>
                    <div className="text-right text-sm text-muted-foreground">
                      <p>{formatDate(item.createdAt)}</p>
                    </div>
                    <button className="rounded-md border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-primary">
                      Open
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </div>

          <div className="space-y-6">
            <section className="rounded-lg border border-border/60 bg-card p-6 shadow-2xs">
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

            <section className="rounded-lg border border-border/60 bg-card p-6 shadow-2xs">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Automations
                  </p>
                  <h2 className="text-xl font-semibold text-foreground">
                    Stay proactive
                  </h2>
                </div>
                <button className="rounded-md border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-primary">
                  Configure
                </button>
              </div>
              <div className="mt-6 space-y-4">
                {automations.map((flow) => (
                  <article
                    key={flow.title}
                    className="rounded-lg border border-border/60 bg-background/40 p-4"
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

      {showUsersModal ? (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 px-4 py-6">
          <div className="w-full max-w-4xl rounded-2xl border border-border/60 bg-background shadow-2xl">
            <div className="flex items-center justify-between border-b border-border/60 px-6 py-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.35em] text-muted-foreground">
                  Admin
                </p>
                <h2 className="mt-1 text-xl font-semibold text-foreground">
                  User management
                </h2>
              </div>
              <button
                type="button"
                onClick={() => setShowUsersModal(false)}
                className="rounded-full border border-border/60 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-foreground"
              >
                Close
              </button>
            </div>

            <div className="max-h-[70vh] overflow-auto px-6 py-5">
              {adminUsersQuery.isLoading ? (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Loading users...
                </p>
              ) : null}

              {adminUsersQuery.isError ? (
                <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {getErrorMessage(adminUsersQuery.error)}
                </p>
              ) : null}

              {!adminUsersQuery.isLoading &&
              (adminUsersQuery.data?.length ?? 0) === 0 ? (
                <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  No users found.
                </p>
              ) : null}

              {adminUsersQuery.data && adminUsersQuery.data.length > 0 ? (
                <div className="space-y-3">
                  {adminUsersQuery.data.map((user) => (
                    <div
                      key={user.id}
                      className="flex flex-col gap-3 rounded-lg border border-border/60 bg-card/60 p-4 sm:flex-row sm:items-center sm:justify-between"
                    >
                      <div>
                        <p className="text-sm font-semibold text-foreground">
                          {user.fullName}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {user.email}
                        </p>
                      </div>
                      <div className="flex flex-wrap items-center gap-3">
                        <select
                          className="rounded-md border border-border/60 bg-background px-3 py-2 text-xs font-semibold text-foreground/80"
                          value={user.role}
                          onChange={(event) =>
                            handleRoleChange(user.id, event.target.value)
                          }
                        >
                          <option value="Employee">Employee</option>
                          <option value="Approver">Approver</option>
                          <option value="Admin">Admin</option>
                        </select>
                        <button
                          type="button"
                          onClick={() =>
                            handleStatusChange(user.id, !user.isActive)
                          }
                          className={`rounded-md border px-3 py-2 text-xs font-semibold transition ${
                            user.isActive
                              ? "border-emerald-200 bg-emerald-50 text-emerald-700 hover:border-emerald-300"
                              : "border-rose-200 bg-rose-50 text-rose-700 hover:border-rose-300"
                          }`}
                        >
                          {user.isActive ? "Active" : "Inactive"}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              ) : null}

              {adminMessage ? (
                <p className="mt-4 rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">
                  {adminMessage}
                </p>
              ) : null}
              {adminError ? (
                <p className="mt-4 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {adminError}
                </p>
              ) : null}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};

const DashboardPage = (props: DashboardPageProps) => {
  return props.authUser.role.toLowerCase() === "employee" ? (
    <EmployeeDashboard {...props} />
  ) : (
    <MockDashboard {...props} />
  );
};

export default DashboardPage;
