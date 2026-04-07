import type { PillProps } from "../types/Pill";

const Pill = ({ children, className }: PillProps) => {
  return (
    <span
      className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-medium ${className ?? ""}`}
    >
      {children}
    </span>
  );
};

export default Pill;
