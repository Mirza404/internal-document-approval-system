import type { Stage } from "../../types/Stage";

export const stageStyles: Record<Stage, string> = {
  Legal: "bg-primary/12 text-primary",
  Finance: "bg-secondary/20 text-secondary",
  Risk: "bg-accent/40 text-accent-foreground",
  Security: "bg-muted text-muted-foreground",
  Executive: "bg-destructive/10 text-destructive",
};
