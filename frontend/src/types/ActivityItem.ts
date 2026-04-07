export type ActivityItem = {
  id: number;
  summary: string;
  time: string;
  owner: string;
  channel: "Slack" | "Email" | "Comment";
};
