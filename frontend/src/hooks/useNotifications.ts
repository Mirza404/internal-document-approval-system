import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getNotifications, markNotificationRead } from "../api/notifications";

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
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ["notifications"],
      });
    },
  });
};
