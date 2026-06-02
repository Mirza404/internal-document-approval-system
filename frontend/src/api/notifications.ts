import { apiClient } from "./client";

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export const getNotifications = () =>
  apiClient.get<Notification[]>("/notifications");

export const markNotificationRead = (id: string) =>
  apiClient.post(`/notifications/${id}/read`);

export const markAllNotificationsRead = () =>
  apiClient.post("/notifications/read-all");
