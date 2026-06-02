import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getNotifications,
  markAllNotificationsRead,
  markNotificationRead,
  type Notification,
} from "../api/notifications";

export const useNotifications = () =>
  useQuery({
    queryKey: ["notifications"],
    queryFn: getNotifications,
    refetchInterval: 30000,
  });

export const useMarkNotificationRead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: markNotificationRead,
    onSuccess: (notification) => {
      queryClient.setQueryData<Notification[]>(["notifications"], (current) =>
        current?.map((item) =>
          item.id === notification.id ? notification : item,
        ),
      );
      void queryClient.invalidateQueries({
        queryKey: ["notifications"],
      });
    },
  });
};

export const useMarkAllNotificationsRead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: () => {
      queryClient.setQueryData<Notification[]>(["notifications"], (current) =>
        current?.map((notification) => ({ ...notification, isRead: true })),
      );
      void queryClient.invalidateQueries({
        queryKey: ["notifications"],
      });
    },
  });
};
