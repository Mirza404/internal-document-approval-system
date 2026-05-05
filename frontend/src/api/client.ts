import axios from "./axios";

//THIS IS BOILERPLATE CODE, WE FORCE ANY BECAUSE WE WANT TO AVOID
// HAVING TO DEFINE TYPES IN ADVANCE.
export const apiClient = {
  //make linter ignore any
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  get: <T = any>(url: string, params?: any) =>
    axios.get<T>(url, { params }).then((res) => res.data),

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  post: <T = any>(url: string, data?: any) =>
    axios.post<T>(url, data).then((res) => res.data),

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  put: <T = any>(url: string, data?: any) =>
    axios.put<T>(url, data).then((res) => res.data),

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  delete: <T = any>(url: string) =>
    axios.delete<T>(url).then((res) => res.data),
};
