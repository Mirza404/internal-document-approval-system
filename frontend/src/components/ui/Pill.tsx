import type { PillProps } from "../../types/Pill";

const Pill = ({ children, className }: PillProps) => {
  return (
    <span
      className={`inline-flex items-center rounded-full border border-border/60 bg-card/70 px-3 py-1 text-xs font-medium text-card-foreground shadow-2xs ${className ?? ""}`}
    >
      {children}
    </span>
  );
};

export default Pill;
