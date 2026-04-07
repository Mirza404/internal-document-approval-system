import type { Automation } from "../types/Automation";

export const automations: Automation[] = [
  {
    title: "Auto-remind reviewers",
    description: "Slack reminder 24h before deadline for assigned reviewers.",
    status: "Active",
  },
  {
    title: "Archive stale drafts",
    description: "Move drafts with no edits in 30 days to the backlog column.",
    status: "Draft",
  },
];