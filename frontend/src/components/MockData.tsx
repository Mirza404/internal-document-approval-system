import type { ReviewItem } from "../types/ReviewItem";
import type { ActivityItem } from "../types/ActivityItem";
import type { Automation } from "../types/Automation";

export const stats = [
  { label: "Awaiting sign-off", value: "14", helper: "+3 vs last week" },
  { label: "Avg. turnaround", value: "6.2d", helper: "1.1d faster" },
  { label: "Escalations", value: "2", helper: "Needs attention" },
];

export const reviewQueue: ReviewItem[] = [
  {
    id: "DOC-2041",
    title: "Q2 Board Deck",
    owner: "Mara Liu",
    stage: "Executive",
    department: "Strategy",
    due: "today • 6:00 PM",
    priority: "High",
    updated: "15m ago",
  },
  {
    id: "DOC-1988",
    title: "Vendor MSA — Northwind",
    owner: "Gabriel Mendez",
    stage: "Legal",
    department: "Revenue",
    due: "tomorrow",
    priority: "Medium",
    updated: "1h ago",
  },
  {
    id: "DOC-1877",
    title: "PCI Self-Assessment",
    owner: "Samira Patel",
    stage: "Security",
    department: "GRC",
    due: "in 3 days",
    priority: "High",
    updated: "2h ago",
  },
  {
    id: "DOC-1765",
    title: "FY26 Budget Reforecast",
    owner: "Trevor K",
    stage: "Finance",
    department: "FP&A",
    due: "next week",
    priority: "Low",
    updated: "yesterday",
  },
  {
    id: "DOC-1702",
    title: "Data Processing Addendum",
    owner: "Nia Gomez",
    stage: "Risk",
    department: "Compliance",
    due: "due Friday",
    priority: "Medium",
    updated: "45m ago",
  },
];

export const activityFeed: ActivityItem[] = [
  {
    id: 1,
    summary: "Legal approved Supplier NDA",
    owner: "S. Patel",
    time: "09:24",
    channel: "Slack",
  },
  {
    id: 2,
    summary: "Finance requested edits on DOC-1765",
    owner: "T. Kim",
    time: "08:50",
    channel: "Email",
  },
  {
    id: 3,
    summary: "Security attached test evidence",
    owner: "GRC Bot",
    time: "08:12",
    channel: "Comment",
  },
  {
    id: 4,
    summary: "Executive signed off DOC-2041",
    owner: "Board Portal",
    time: "07:40",
    channel: "Slack",
  },
];

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