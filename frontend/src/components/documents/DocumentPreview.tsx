import { useRef } from "react";
import {
  buildDocumentPreviewSections,
  previewPlaceholder,
  type DocumentPreviewSource,
} from "./documentPreview";

interface DocumentPreviewProps {
  source: DocumentPreviewSource;
  title?: string;
}

const isDateLabel = (label: string) =>
  label === "Start date" || label === "End date";

const formatPreviewValue = (label: string, value: string) => {
  if (value === previewPlaceholder || !isDateLabel(label)) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(
    new Date(value),
  );
};

const DocumentPreview = ({
  source,
  title = "Document preview",
}: DocumentPreviewProps) => {
  const previewRef = useRef<HTMLElement | null>(null);
  const sections = buildDocumentPreviewSections(source);
  const handlePrint = () => {
    const preview = previewRef.current;
    if (!preview) {
      return;
    }

    const clearPrintTarget = () => preview.classList.remove("print-target");
    preview.classList.add("print-target");
    window.addEventListener("afterprint", clearPrintTarget, { once: true });
    window.print();
  };

  return (
    <section
      ref={previewRef}
      className="document-preview rounded-xl border border-border/70 bg-background/70 p-4 shadow-sm"
    >
      <div className="flex items-start justify-between gap-3 border-b border-border/60 pb-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-primary">
            Read-only preview
          </p>
          <h3 className="mt-1 text-base font-semibold text-foreground">
            {title}
          </h3>
        </div>
        <button
          type="button"
          onClick={handlePrint}
          className="no-print rounded-md border border-border/70 bg-background px-3 py-1.5 text-xs font-semibold text-muted-foreground transition hover:border-primary/40 hover:text-primary"
        >
          Print
        </button>
      </div>

      <div className="mt-4 space-y-4">
        {sections.map((section) => (
          <div key={section.title}>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              {section.title}
            </p>
            <dl className="mt-2 grid gap-2 sm:grid-cols-2">
              {section.fields.map(({ label, value }) => (
                <div
                  key={label}
                  className="rounded-md border border-border/50 bg-card/70 px-3 py-2"
                >
                  <dt className="text-[11px] font-semibold uppercase text-muted-foreground">
                    {label}
                  </dt>
                  <dd
                    className={`mt-1 text-sm ${
                      value === previewPlaceholder
                        ? "italic text-muted-foreground"
                        : "font-medium text-foreground"
                    }`}
                  >
                    {formatPreviewValue(label, value)}
                  </dd>
                </div>
              ))}
            </dl>
          </div>
        ))}
      </div>
    </section>
  );
};

export default DocumentPreview;
