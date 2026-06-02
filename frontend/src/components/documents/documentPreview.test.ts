import { describe, expect, it } from "vitest";
import {
  buildDocumentPreviewSections,
  previewPlaceholder,
} from "./documentPreview";

const findField = (
  sections: ReturnType<typeof buildDocumentPreviewSections>,
  label: string,
) =>
  sections
    .flatMap((section) => section.fields)
    .find((field) => field.label === label)?.value;

describe("buildDocumentPreviewSections", () => {
  it("maps shared request and requester fields", () => {
    const sections = buildDocumentPreviewSections({
      title: "Annual leave",
      description: "Family trip",
      documentTypeName: "Leave Request",
      documentTypeDescription: "Employee leave request.",
      documentCategoryName: "Human Resources",
      requesterFullName: "Demo Employee",
      requesterEmail: "employee@example.com",
      priority: "Normal",
    });

    expect(findField(sections, "Document type")).toBe("Leave Request");
    expect(findField(sections, "Type description")).toBe(
      "Employee leave request.",
    );
    expect(findField(sections, "Full name")).toBe("Demo Employee");
    expect(findField(sections, "Email")).toBe("employee@example.com");
    expect(findField(sections, "Description")).toBe("Family trip");
  });

  it("shows placeholders for unavailable leave values", () => {
    const sections = buildDocumentPreviewSections({
      documentTypeName: "Leave Request",
      documentCategoryName: "Human Resources",
    });

    expect(findField(sections, "Leave type")).toBe(previewPlaceholder);
    expect(findField(sections, "Start date")).toBe(previewPlaceholder);
    expect(findField(sections, "End date")).toBe(previewPlaceholder);
  });

  it("maps payment and internship metadata for supported types", () => {
    const paymentSections = buildDocumentPreviewSections({
      documentTypeName: "Payment Procedure",
      amount: 125.5,
      budgetCode: "PAY-42",
    });
    const internshipSections = buildDocumentPreviewSections({
      documentTypeName: "Internship Submission",
      counterparty: "Example Ltd",
      attachmentNote: "",
    });

    expect(findField(paymentSections, "Amount")).toBe("125.5");
    expect(findField(paymentSections, "Payment reference")).toBe("PAY-42");
    expect(findField(internshipSections, "Organization")).toBe("Example Ltd");
    expect(findField(internshipSections, "Supporting note")).toBe(
      previewPlaceholder,
    );
  });
});
