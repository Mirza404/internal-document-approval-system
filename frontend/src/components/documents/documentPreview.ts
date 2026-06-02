export const previewPlaceholder = "Not provided";

export interface DocumentPreviewSource {
  title?: string | null;
  description?: string | null;
  documentTypeName?: string | null;
  documentTypeDescription?: string | null;
  documentCategoryName?: string | null;
  requesterFullName?: string | null;
  requesterEmail?: string | null;
  priority?: string | null;
  leaveType?: string | null;
  leaveStartDate?: string | null;
  leaveEndDate?: string | null;
  amount?: number | string | null;
  budgetCode?: string | null;
  counterparty?: string | null;
  attachmentNote?: string | null;
}

export interface DocumentPreviewField {
  label: string;
  value: string;
}

export interface DocumentPreviewSection {
  title: string;
  fields: DocumentPreviewField[];
}

type MetadataKind = "leave" | "payment" | "internship" | "none";

const displayValue = (value?: number | string | null) => {
  if (value == null || String(value).trim() === "") {
    return previewPlaceholder;
  }

  return String(value);
};

const getMetadataKind = ({
  documentCategoryName,
  documentTypeName,
}: DocumentPreviewSource): MetadataKind => {
  const typeName = documentTypeName?.trim().toLowerCase();
  const categoryName = documentCategoryName?.trim().toLowerCase();

  if (
    typeName === "leave request" ||
    categoryName === "hr" ||
    categoryName === "human resources"
  ) {
    return "leave";
  }

  if (
    typeName === "payment procedure" ||
    categoryName === "payments" ||
    categoryName === "finance"
  ) {
    return "payment";
  }

  if (
    typeName === "internship submission" ||
    categoryName === "internships" ||
    categoryName === "contract"
  ) {
    return "internship";
  }

  return "none";
};

const getMetadataFields = (
  source: DocumentPreviewSource,
): DocumentPreviewField[] => {
  switch (getMetadataKind(source)) {
    case "leave":
      return [
        { label: "Leave type", value: displayValue(source.leaveType) },
        { label: "Start date", value: displayValue(source.leaveStartDate) },
        { label: "End date", value: displayValue(source.leaveEndDate) },
      ];
    case "payment":
      return [
        { label: "Amount", value: displayValue(source.amount) },
        {
          label: "Payment reference",
          value: displayValue(source.budgetCode),
        },
      ];
    case "internship":
      return [
        { label: "Organization", value: displayValue(source.counterparty) },
        {
          label: "Supporting note",
          value: displayValue(source.attachmentNote),
        },
      ];
    default:
      return [];
  }
};

export const buildDocumentPreviewSections = (
  source: DocumentPreviewSource,
): DocumentPreviewSection[] => {
  const sections: DocumentPreviewSection[] = [
    {
      title: "Request",
      fields: [
        { label: "Title", value: displayValue(source.title) },
        { label: "Priority", value: displayValue(source.priority) },
        {
          label: "Document type",
          value: displayValue(source.documentTypeName),
        },
        {
          label: "Category",
          value: displayValue(source.documentCategoryName),
        },
        {
          label: "Type description",
          value: displayValue(source.documentTypeDescription),
        },
      ],
    },
    {
      title: "Requester",
      fields: [
        {
          label: "Full name",
          value: displayValue(source.requesterFullName),
        },
        { label: "Email", value: displayValue(source.requesterEmail) },
      ],
    },
    {
      title: "Details",
      fields: [
        { label: "Description", value: displayValue(source.description) },
      ],
    },
  ];
  const metadataFields = getMetadataFields(source);

  if (metadataFields.length > 0) {
    sections.push({
      title: "Type-specific information",
      fields: metadataFields,
    });
  }

  return sections;
};
