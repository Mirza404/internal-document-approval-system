import { apiClient } from "./client";

// Types (placeholders - replace with actual models)
export interface Document {
  id: string;
  title: string;
  status: string;
  // Add more fields as needed
}

export interface CreateDocumentRequest {
  title: string;
  // Add more fields
}

// API functions
export const getDocuments = (): Promise<Document[]> =>
  apiClient.get("/documents");

export const getDocument = (id: string): Promise<Document> =>
  apiClient.get(`/documents/${id}`);

export const createDocument = (
  data: CreateDocumentRequest,
): Promise<Document> => apiClient.post("/documents", data);

export const updateDocument = (
  id: string,
  data: Partial<Document>,
): Promise<Document> => apiClient.put(`/documents/${id}`, data);

export const deleteDocument = (id: string): Promise<void> =>
  apiClient.delete(`/documents/${id}`);
