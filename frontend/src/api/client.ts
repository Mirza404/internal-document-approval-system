import axios from "./axios";

// Generic API client functions
export const apiClient = {
  get: <T = any>(url: string, params?: any) =>
    axios.get<T>(url, { params }).then((res) => res.data),

  post: <T = any>(url: string, data?: any) =>
    axios.post<T>(url, data).then((res) => res.data),

  put: <T = any>(url: string, data?: any) =>
    axios.put<T>(url, data).then((res) => res.data),

  delete: <T = any>(url: string) =>
    axios.delete<T>(url).then((res) => res.data),
};
