import type { Priority } from "../../types/Priority";

export const priorityStyles: Record<Priority, string> = {
  High: "bg-destructive/10 text-destructive",
  Medium: "bg-secondary/15 text-secondary",
  Low: "bg-accent/35 text-accent-foreground",
};
