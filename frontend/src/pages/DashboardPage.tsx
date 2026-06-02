import { useEffect, useMemo, useRef, useState, type FormEvent } from "react";
import axios from "axios";
import type { AuthUser } from "../auth/authStorage";
import Pill from "../components/ui/Pill";
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
import {
  useApprovalDecision,
  usePendingApprovals,
} from "../hooks/useApprovals";
import type {
  ApprovalDecisionAction,
  PendingApprovalItem,
} from "../api/approvals";
import type {
  ApprovalHistoryItem,
  CreateDocumentRequest,
  Document,
  UpdateDocumentRequest,
} from "../api/documents";
import { getApprovalHistory } from "../api/documents";
import {
  useMarkNotificationRead,
  useNotifications,
} from "../hooks/useNotifications";

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
  PendingApproval: "bg-amber-100 text-amber-800",
  UnderReview: "bg-sky-100 text-sky-700",
  ChangesRequested: "bg-amber-100 text-amber-800",
  Approved: "bg-emerald-100 text-emerald-700",
  Rejected: "bg-rose-100 text-rose-700",
};

const fieldClass =
  "w-full rounded-xl border border-input bg-background/80 px-3.5 py-2.5 text-sm text-foreground shadow-2xs outline-none transition placeholder:text-muted-foreground focus:border-primary/60 focus:bg-background focus:ring-2 focus:ring-primary/15";

const labelClass =
  "text-xs font-semibold uppercase tracking-wide text-muted-foreground";
const shellClass = "app-shell min-h-screen pb-10";

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

const getStatusTone = (status: string) =>
  statusClasses[status] ?? "bg-muted text-muted-foreground";

const getDocumentMetadataRows = (document: Document) =>
  [
    document.leaveType ? ["Leave type", document.leaveType] : null,
    document.leaveStartDate
      ? ["Leave start", formatDate(document.leaveStartDate)]
      : null,
    document.leaveEndDate
      ? ["Leave end", formatDate(document.leaveEndDate)]
      : null,
    document.amount != null ? ["Amount", String(document.amount)] : null,
    document.budgetCode ? ["Payment reference", document.budgetCode] : null,
    document.counterparty ? ["Organization", document.counterparty] : null,
    document.attachmentNote
      ? ["Supporting note", document.attachmentNote]
      : null,
  ].filter(Boolean) as [string, string][];

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

  if (
    typeName === "leave request" ||
    categoryName === "hr" ||
    categoryName === "human resources"
  ) {
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

const NotificationsMenu = () => {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement | null>(null);
  const notificationsQuery = useNotifications();
  const markRead = useMarkNotificationRead();
  const notifications = notificationsQuery.data ?? [];
  const unreadCount = notifications.filter(
    (notification) => !notification.isRead,
  ).length;

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <div className="relative" ref={menuRef}>
      <button
        type="button"
        onClick={() => setIsOpen((current) => !current)}
        className="relative rounded-md border border-border/60 bg-background p-2 transition hover:bg-muted"
        aria-label="Notifications"
        aria-expanded={isOpen}
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={2}
          stroke="currentColor"
          className="h-5 w-5 text-muted-foreground"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M14.857 17.082a23.848 23.848 0 005.454-1.31A8.967 8.967 0 0018 9.75V9a6 6 0 10-12 0v.75a8.967 8.967 0 00-2.311 6.022c1.733.64 3.56 1.08 5.454 1.31m5.714 0a24.255 24.255 0 01-5.714 0m5.714 0a3 3 0 11-5.714 0"
          />
        </svg>

        {unreadCount > 0 && (
          <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="app-panel absolute right-0 z-30 mt-2 w-80 rounded-2xl shadow-lg">
          <div className="flex items-center justify-between border-b border-border/60 px-4 py-3">
            <div>
              <p className="font-semibold text-foreground">Notifications</p>
              <p className="text-xs text-muted-foreground">
                {unreadCount > 0 ? `${unreadCount} unread` : "All caught up"}
              </p>
            </div>

            <button
              type="button"
              onClick={() => setIsOpen(false)}
              className="text-xs text-muted-foreground hover:text-foreground"
            >
              Close
            </button>
          </div>

          <div className="max-h-80 space-y-2 overflow-y-auto p-3">
            {notificationsQuery.isLoading && (
              <p className="text-xs text-muted-foreground">
                Loading notifications...
              </p>
            )}

            {!notificationsQuery.isLoading && notifications.length === 0 && (
              <p className="text-xs text-muted-foreground">
                No notifications yet.
              </p>
            )}

            {notifications.slice(0, 6).map((notification) => (
              <div
                key={notification.id}
                className={`rounded-md border px-3 py-3 text-xs ${
                  notification.isRead
                    ? "border-border/60 bg-muted/30 text-muted-foreground"
                    : "border-primary/20 bg-primary/5 text-foreground"
                }`}
              >
                <p className="font-semibold">{notification.title}</p>
                <p className="mt-1">{notification.message}</p>

                <div className="mt-2 flex items-center justify-between">
                  <span className="text-[11px] text-muted-foreground">
                    {new Date(notification.createdAt).toLocaleString()}
                  </span>

                  {!notification.isRead && (
                    <button
                      type="button"
                      onClick={() => markRead.mutate(notification.id)}
                      className="text-[11px] font-semibold text-primary hover:underline"
                    >
                      Mark read
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

const EmployeeDashboard = ({ authUser, onLogout }: DashboardPageProps) => {
  const [form, setForm] = useState<SubmissionFormState>(initialFormState);
  const [resubmitForm, setResubmitForm] = useState<SubmissionFormState | null>(
    null,
  );
  const [resubmitDocumentId, setResubmitDocumentId] = useState<string | null>(
    null,
  );
  const [resubmitNotes, setResubmitNotes] = useState("");
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(
    null,
  );
  const [isDetailExpanded, setIsDetailExpanded] = useState(true);
  const detailPanelRef = useRef<HTMLElement | null>(null);
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
  const selectedDocument = useMemo(
    () =>
      sortedDocuments.find((document) => document.id === selectedDocumentId) ??
      latestDocument,
    [latestDocument, selectedDocumentId, sortedDocuments],
  );
  const selectedApprovalHistory = selectedDocument
    ? (approvalHistory[selectedDocument.id] ?? [])
    : [];

  const pendingCount = myDocuments.filter(
    (document) => document.status === "PendingApproval",
  ).length;
  const changesCount = myDocuments.filter(
    (document) => document.status === "ChangesRequested",
  ).length;
  const approvedCount = myDocuments.filter(
    (document) => document.status === "Approved",
  ).length;
  const rejectedCount = myDocuments.filter(
    (document) => document.status === "Rejected",
  ).length;
  const totalDocuments = myDocuments.length;
  const openCount = pendingCount + changesCount;
  const approvalRate = totalDocuments
    ? Math.round((approvedCount / totalDocuments) * 100)
    : 0;
  const dashboardStats = [
    {
      label: "Open",
      value: openCount,
      helper: "Pending or returned",
      className: "border-sky-200 bg-sky-50 text-sky-800",
    },
    {
      label: "Needs changes",
      value: changesCount,
      helper: "Waiting on you",
      className: "border-amber-200 bg-amber-50 text-amber-800",
    },
    {
      label: "Approved",
      value: approvedCount,
      helper: `${approvalRate}% approval rate`,
      className: "border-emerald-200 bg-emerald-50 text-emerald-800",
    },
    {
      label: "Rejected",
      value: rejectedCount,
      helper: "Closed without approval",
      className: "border-rose-200 bg-rose-50 text-rose-800",
    },
  ];

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
      const createdDocument = await createDocument.mutateAsync(
        buildCreatePayload(form),
      );
      setSelectedDocumentId(createdDocument.id);
      setForm(initialFormState);
      setFormMessage("Submitted for approval.");
    } catch (error) {
      setFormError(getErrorMessage(error));
    }
  };

  const handleStartResubmit = (document: Document) => {
    handleSelectDocument(document.id);
    setResubmitDocumentId(document.id);
    setResubmitForm(buildFormFromDocument(document));
    setResubmitNotes("");
    setResubmitMessage(null);
    setResubmitError(null);
  };

  const handleSelectDocument = (documentId: string) => {
    setSelectedDocumentId(documentId);
    setIsDetailExpanded(true);

    if (window.matchMedia("(max-width: 1279px)").matches) {
      window.requestAnimationFrame(() => {
        detailPanelRef.current?.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });
      });
    }
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
          changeNotes: resubmitNotes || null,
        },
      });
      setResubmitForm(null);
      setResubmitDocumentId(null);
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
        <div className="grid gap-4">
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
        <div className="grid gap-4">
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
        <div className="grid gap-4">
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
    <div className={shellClass}>
      <div className="mx-auto flex max-w-7xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
        <header className="app-panel overflow-hidden rounded-2xl">
          <div className="grid gap-0 lg:grid-cols-[minmax(0,1fr)_340px]">
            <div className="border-l-4 border-primary px-6 py-7 sm:px-8">
              <p className="text-xs font-semibold uppercase tracking-wide text-primary">
                Employee workspace
              </p>
              <h1 className="mt-2 text-3xl font-semibold text-foreground">
                Submission overview
              </h1>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground">
                Track active requests, returned documents, and approvals from
                one workspace.
              </p>
            </div>
            <div className="border-t border-border/60 bg-accent/45 px-6 py-5 lg:border-l lg:border-t-0">
              <div className="flex items-center gap-2">
                <a
                  href="#new-document"
                  className="inline-flex rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90"
                >
                  New Document
                </a>
                <NotificationsMenu />
              </div>
              <p className="mt-4 text-sm font-semibold text-foreground">
                {authUser.fullName}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {authUser.role} account
              </p>
              <button
                type="button"
                onClick={onLogout}
                className="mt-4 rounded-md border border-border/70 bg-background px-4 py-2 text-sm font-semibold text-foreground/80 transition hover:border-primary/40 hover:text-foreground"
              >
                Sign out
              </button>
            </div>
          </div>
        </header>

        <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          {dashboardStats.map((stat) => (
            <article
              key={stat.label}
              className={`rounded-2xl border px-5 py-4 shadow-sm transition duration-200 hover:-translate-y-0.5 hover:shadow-md ${stat.className}`}
            >
              <p className="text-sm font-medium opacity-80">{stat.label}</p>
              <p className="mt-2 text-3xl font-semibold">{stat.value}</p>
              <p className="mt-1 text-xs font-semibold uppercase opacity-70">
                {stat.helper}
              </p>
            </article>
          ))}
        </section>

        <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_430px]">
          <div className="space-y-6">
            <section
              ref={detailPanelRef}
              className="app-panel rounded-2xl p-5 xl:sticky xl:top-6 xl:z-20 xl:max-h-[calc(100vh-3rem)] xl:overflow-y-auto"
            >
              <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-muted-foreground">
                    Document detail
                  </p>
                  <h2 className="mt-1 truncate text-lg font-semibold text-foreground">
                    {selectedDocument?.title ?? "No submissions yet"}
                  </h2>
                  {selectedDocument && (
                    <p className="mt-1 text-sm text-muted-foreground">
                      {documentTypeLabels.get(
                        selectedDocument.documentTypeId,
                      ) || "Document type not set"}
                    </p>
                  )}
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  {selectedDocument && (
                    <Pill className={getStatusTone(selectedDocument.status)}>
                      {formatDocumentStatus(selectedDocument.status)}
                    </Pill>
                  )}
                  <button
                    type="button"
                    onClick={() => setIsDetailExpanded((current) => !current)}
                    aria-expanded={isDetailExpanded}
                    className="rounded-md border border-border/60 bg-background/80 px-3 py-1 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:bg-primary/5 hover:text-primary"
                  >
                    {isDetailExpanded ? "Collapse detail" : "Expand detail"}
                  </button>
                </div>
              </div>

              {isDetailExpanded && documentsQuery.isLoading && (
                <p className="mt-4 rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Loading document detail...
                </p>
              )}

              {isDetailExpanded && !documentsQuery.isLoading && selectedDocument && (
                <div className="mt-4 grid gap-3 text-sm sm:grid-cols-3">
                  <div>
                    <p className="text-xs font-semibold uppercase text-muted-foreground">
                      Priority
                    </p>
                    <p className="mt-1 font-medium text-foreground">
                      {selectedDocument.priority}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs font-semibold uppercase text-muted-foreground">
                      Submitted
                    </p>
                    <p className="mt-1 font-medium text-foreground">
                      {formatDate(selectedDocument.createdAt)}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs font-semibold uppercase text-muted-foreground">
                      Updated
                    </p>
                    <p className="mt-1 font-medium text-foreground">
                      {formatDate(
                        selectedDocument.updatedAt ??
                          selectedDocument.createdAt,
                      )}
                    </p>
                  </div>
                </div>
              )}

              {isDetailExpanded && !documentsQuery.isLoading && !selectedDocument && (
                <p className="mt-4 text-sm text-muted-foreground">
                  Submit a document to start building your history.
                </p>
              )}

              {isDetailExpanded && selectedDocument?.description && (
                <div className="mt-4 rounded-md bg-muted/60 px-3 py-2 text-sm text-muted-foreground">
                  {selectedDocument.description}
                </div>
              )}

              {isDetailExpanded &&
                selectedDocument &&
                getDocumentMetadataRows(selectedDocument).length > 0 && (
                  <div className="mt-4 grid gap-3 rounded-md border border-border/60 bg-background/40 p-3 text-sm sm:grid-cols-2">
                    {getDocumentMetadataRows(selectedDocument).map(
                      ([label, value]) => (
                        <div key={label}>
                          <p className="text-xs font-semibold uppercase text-muted-foreground">
                            {label}
                          </p>
                          <p className="mt-1 font-medium text-foreground">
                            {value}
                          </p>
                        </div>
                      ),
                    )}
                  </div>
                )}

              {isDetailExpanded &&
                selectedDocument?.latestVersionNumber != null && (
                <p className="mt-4 rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Latest version{" "}
                  {selectedDocument.latestVersionLabel ??
                    `v${selectedDocument.latestVersionNumber}`}
                  {selectedDocument.latestVersionCreatedAt
                    ? ` · ${formatDate(
                        selectedDocument.latestVersionCreatedAt,
                      )}`
                    : ""}
                  {selectedDocument.latestVersionChangeNotes
                    ? ` · ${selectedDocument.latestVersionChangeNotes}`
                    : ""}
                </p>
              )}

              {isDetailExpanded &&
                selectedDocument &&
                selectedApprovalHistory.length > 0 && (
                <div className="mt-5 space-y-3 border-t border-border/60 pt-4">
                  <p className="text-xs font-semibold uppercase text-muted-foreground">
                    Approval history
                  </p>
                  {selectedApprovalHistory.map((item) => (
                    <div key={item.id} className="flex gap-3 text-sm">
                      <span
                        className="mt-2 h-2 w-2 rounded-full bg-primary"
                        aria-hidden="true"
                      />
                      <div className="flex-1 rounded-md bg-muted/40 px-3 py-2">
                        <div className="flex flex-wrap items-center justify-between gap-2">
                          <span className="font-semibold text-foreground">
                            {item.approverFullName}
                          </span>
                          <span className="text-xs text-muted-foreground">
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
                    </div>
                  ))}
                </div>
              )}

              {isDetailExpanded &&
                selectedDocument?.status === "ChangesRequested" &&
                (resubmitForm && resubmitDocumentId === selectedDocument.id ? (
                  <form
                    className="mt-5 space-y-4 rounded-lg border border-border/60 bg-background/50 p-4"
                    onSubmit={(event) =>
                      handleResubmit(event, selectedDocument)
                    }
                  >
                    <div className="flex items-center justify-between gap-3">
                      <h3 className="text-base font-semibold text-foreground">
                        Edit resubmission
                      </h3>
                      <button
                        type="button"
                        onClick={() => {
                          setResubmitForm(null);
                          setResubmitDocumentId(null);
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
                      onClick={() => handleStartResubmit(selectedDocument)}
                      disabled={updateDocument.isPending}
                      className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      Edit and resubmit
                    </button>
                  </div>
                ))}
            </section>

            <section className="app-panel rounded-2xl p-5">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    My documents
                  </p>
                  <h2 className="mt-1 text-xl font-semibold text-foreground">
                    Submission timeline
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
                      role="button"
                      tabIndex={0}
                      onClick={() => handleSelectDocument(document.id)}
                      onKeyDown={(event) => {
                        if (event.key === "Enter" || event.key === " ") {
                          event.preventDefault();
                          handleSelectDocument(document.id);
                        }
                      }}
                      aria-label={`Show details for ${document.title}`}
                      aria-pressed={selectedDocument?.id === document.id}
                      className={`app-card-interactive grid cursor-pointer gap-4 rounded-xl border p-4 text-left transition hover:border-primary/30 hover:shadow-sm md:grid-cols-[minmax(0,1fr)_180px] ${
                        selectedDocument?.id === document.id
                          ? "border-primary/60 bg-primary/10 shadow-sm ring-1 ring-primary/20"
                          : "border-border/60 bg-background hover:bg-primary/[0.03]"
                      }`}
                    >
                      <div className="min-w-0">
                        <div className="flex flex-wrap items-center gap-2">
                          <Pill className={getStatusTone(document.status)}>
                            {formatDocumentStatus(document.status)}
                          </Pill>
                          {document.latestVersionNumber != null && (
                            <span className="rounded-full bg-muted px-3 py-1 text-xs font-semibold text-muted-foreground">
                              {document.latestVersionLabel ??
                                `v${document.latestVersionNumber}`}
                            </span>
                          )}
                        </div>
                        <h3 className="mt-3 truncate text-base font-semibold text-foreground">
                          {document.title}
                        </h3>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {documentTypeLabels.get(document.documentTypeId) ||
                            "Document type not set"}
                        </p>
                        <p className="mt-3 text-xs font-semibold uppercase text-primary">
                          {selectedDocument?.id === document.id
                            ? "Showing in detail panel"
                            : "Select to view details"}
                        </p>
                        {document.latestVersionChangeNotes && (
                          <p className="mt-3 line-clamp-2 text-sm text-muted-foreground">
                            {document.latestVersionChangeNotes}
                          </p>
                        )}

                        {approvalHistory[document.id]?.length ? (
                          <div className="mt-4 space-y-2 border-t border-border/50 pt-3">
                            <p className="text-xs font-semibold uppercase text-muted-foreground">
                              Approval notes
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
                      </div>

                      <div className="grid content-start gap-3 rounded-md bg-muted/40 p-3 text-xs text-muted-foreground">
                        <div>
                          <p className="font-semibold uppercase">Priority</p>
                          <p className="mt-1 text-sm font-medium text-foreground">
                            {document.priority}
                          </p>
                        </div>
                        <div>
                          <p className="font-semibold uppercase">Submitted</p>
                          <p className="mt-1 text-sm font-medium text-foreground">
                            {formatDate(document.createdAt)}
                          </p>
                        </div>
                        <div>
                          <p className="font-semibold uppercase">Updated</p>
                          <p className="mt-1 text-sm font-medium text-foreground">
                            {formatDate(
                              document.updatedAt ?? document.createdAt,
                            )}
                          </p>
                        </div>
                      </div>
                    </article>
                  );
                })}
              </div>
            </section>
          </div>

          <aside className="xl:sticky xl:top-6 xl:self-start">
            <form
              id="new-document"
              className="app-panel rounded-2xl p-5"
              onSubmit={handleSubmit}
            >
              <div className="flex flex-col gap-1">
                <p className="text-sm font-medium text-primary">
                  Start submission
                </p>
                <h2 className="text-xl font-semibold text-foreground">
                  New document
                </h2>
              </div>

              <div className="mt-5 grid gap-4">
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

              <button
                type="submit"
                disabled={createDocument.isPending}
                className="mt-5 w-full rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow-2xs transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {createDocument.isPending ? "Submitting..." : "Submit"}
              </button>
            </form>
          </aside>
        </section>
      </div>
    </div>
  );
};

const ApprovalDetail = ({
  comments,
  error,
  isPending,
  item,
  message,
  onCommentsChange,
  onDecision,
}: {
  comments: string;
  error: string | null;
  isPending: boolean;
  item: PendingApprovalItem;
  message: string | null;
  onCommentsChange: (value: string) => void;
  onDecision: (action: ApprovalDecisionAction) => void;
}) => {
  const rows = [
    ["Type", item.documentTypeName],
    ["Submitted by", item.creatorFullName],
    ["Submitted", formatDate(item.createdAt)],
    ["Status", formatDocumentStatus(item.status)],
  ];

  return (
    <div className="mt-5 space-y-4">
      <Pill className="bg-sky-100 text-sky-700">
        {formatDocumentStatus(item.status)}
      </Pill>

      <div className="grid gap-3 rounded-md border border-border/60 bg-background/50 p-4 text-sm">
        {rows.map(([label, value]) => (
          <div key={label}>
            <p className="text-xs font-semibold uppercase text-muted-foreground">
              {label}
            </p>
            <p className="mt-1 font-medium text-foreground">{value}</p>
          </div>
        ))}
      </div>

      <label className="space-y-2">
        <span className={labelClass}>Reviewer notes</span>
        <textarea
          className={`${fieldClass} min-h-24 resize-y`}
          value={comments}
          onChange={(event) => onCommentsChange(event.target.value)}
        />
      </label>

      {(error || message) && (
        <p
          className={`rounded-md px-3 py-2 text-sm ${
            error
              ? "bg-destructive/10 text-destructive"
              : "bg-emerald-100 text-emerald-700"
          }`}
        >
          {error ?? message}
        </p>
      )}

      <div className="grid gap-2 sm:grid-cols-3">
        <button
          type="button"
          disabled={isPending}
          onClick={() => onDecision("approve")}
          className="rounded-md bg-emerald-600 px-3 py-2 text-sm font-semibold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
        >
          Approve
        </button>
        <button
          type="button"
          disabled={isPending}
          onClick={() => onDecision("request-changes")}
          className="rounded-md bg-amber-500 px-3 py-2 text-sm font-semibold text-white transition hover:bg-amber-600 disabled:cursor-not-allowed disabled:opacity-60"
        >
          Request changes
        </button>
        <button
          type="button"
          disabled={isPending}
          onClick={() => onDecision("reject")}
          className="rounded-md bg-rose-600 px-3 py-2 text-sm font-semibold text-white transition hover:bg-rose-700 disabled:cursor-not-allowed disabled:opacity-60"
        >
          Reject
        </button>
      </div>
    </div>
  );
};

const ApprovalDashboard = ({ authUser, onLogout }: DashboardPageProps) => {
  const pendingApprovalsQuery = usePendingApprovals();
  const [showUsersModal, setShowUsersModal] = useState(false);
  const [selectedApprovalId, setSelectedApprovalId] = useState<string | null>(
    null,
  );
  const [decisionComments, setDecisionComments] = useState("");
  const [decisionMessage, setDecisionMessage] = useState<string | null>(null);
  const [decisionError, setDecisionError] = useState<string | null>(null);
  const [adminMessage, setAdminMessage] = useState<string | null>(null);
  const [adminError, setAdminError] = useState<string | null>(null);
  const isAdmin = authUser.role.toLowerCase() === "admin";
  const adminUsersQuery = useAdminUsers();
  const updateRole = useUpdateAdminUserRole();
  const updateStatus = useUpdateAdminUserStatus();
  const approvalDecision = useApprovalDecision();

  const filteredQueue = useMemo(() => {
    return pendingApprovalsQuery.data ?? [];
  }, [pendingApprovalsQuery.data]);
  const selectedApproval =
    filteredQueue.find((item) => item.documentId === selectedApprovalId) ??
    filteredQueue[0];

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

  const handleSelectApproval = (item: PendingApprovalItem) => {
    setSelectedApprovalId(item.documentId);
    setDecisionComments("");
    setDecisionMessage(null);
    setDecisionError(null);
  };

  const handleApprovalDecision = async (action: ApprovalDecisionAction) => {
    if (!selectedApproval) {
      return;
    }

    setDecisionMessage(null);
    setDecisionError(null);

    try {
      await approvalDecision.mutateAsync({
        documentId: selectedApproval.documentId,
        action,
        comments: decisionComments || null,
      });
      setDecisionComments("");
      setSelectedApprovalId(null);
      setDecisionMessage("Decision saved.");
    } catch (error) {
      setDecisionError(getErrorMessage(error));
    }
  };

  return (
    <div className={shellClass}>
      <div className="mx-auto flex max-w-7xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
        <header className="app-panel overflow-hidden rounded-2xl">
          <div className="border-l-4 border-secondary px-6 py-7 sm:px-8">
            <p className="text-xs font-semibold uppercase tracking-wide text-secondary">
              Internal workflows
            </p>
            <div className="mt-4 flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
              <div className="space-y-3">
                <h1 className="text-3xl font-semibold text-foreground sm:text-4xl">
                  Approval operations
                </h1>
                <p className="max-w-2xl text-base text-muted-foreground">
                  Review pending requests, manage user access, and keep document
                  movement visible from one operational workspace.
                </p>
              </div>
              <div className="flex flex-wrap gap-3">
                <NotificationsMenu />
                <div className="rounded-md border border-border/60 bg-accent/45 px-4 py-2 text-sm font-medium text-foreground/80">
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
              </div>
            </div>
          </div>
        </header>

        <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_380px]">
          <div className="app-panel rounded-2xl p-6 lg:col-span-2">
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
                  className={`app-card-interactive flex flex-col gap-4 rounded-xl border px-4 py-4 transition hover:border-primary/30 hover:bg-card/80 sm:flex-row sm:items-center ${
                    selectedApproval?.documentId === item.documentId
                      ? "border-primary/60 bg-primary/10 shadow-sm ring-1 ring-primary/20"
                      : "border-border/60 bg-background/40"
                  }`}
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
                    <button
                      type="button"
                      onClick={() => handleSelectApproval(item)}
                      aria-pressed={
                        selectedApproval?.documentId === item.documentId
                      }
                      className={`rounded-md border px-3 py-1 text-xs font-semibold transition ${
                        selectedApproval?.documentId === item.documentId
                          ? "border-primary/50 bg-primary/10 text-primary shadow-inner"
                          : "border-border/60 text-muted-foreground hover:border-primary/40 hover:bg-primary/5 hover:text-primary"
                      }`}
                    >
                      Review
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </div>

          <div className="space-y-6">
            <section className="app-panel rounded-2xl p-6">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Request detail
                </p>
                <h2 className="mt-1 text-xl font-semibold text-foreground">
                  {selectedApproval?.title ?? "No request selected"}
                </h2>
              </div>

              {selectedApproval ? (
                <ApprovalDetail
                  comments={decisionComments}
                  error={decisionError}
                  isPending={approvalDecision.isPending}
                  item={selectedApproval}
                  message={decisionMessage}
                  onCommentsChange={setDecisionComments}
                  onDecision={handleApprovalDecision}
                />
              ) : (
                <p className="mt-5 rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">
                  Pending requests will appear here when they are assigned.
                </p>
              )}
            </section>
          </div>
        </section>
      </div>

      {showUsersModal ? (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 px-4 py-6">
          <div className="app-panel w-full max-w-4xl rounded-3xl shadow-2xl">
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
                      className="app-card app-card-interactive flex flex-col gap-3 rounded-xl p-4 sm:flex-row sm:items-center sm:justify-between"
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
    <ApprovalDashboard {...props} />
  );
};

export default DashboardPage;
