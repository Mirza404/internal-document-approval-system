import { apiClient } from "./client";

export interface Document {
  id: string;
  title: string;
  description: string;
  documentTypeId: string;
  documentTypeName: string;
  documentCategoryName: string;
  createdByUserId: string;
  status: string;
  priority: string;
  leaveType?: string | null;
  leaveStartDate?: string | null;
  leaveEndDate?: string | null;
  amount?: number | null;
  budgetCode?: string | null;
  counterparty?: string | null;
  attachmentNote?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  approvedAt?: string | null;
  latestVersionNumber?: number | null;
  latestVersionCreatedAt?: string | null;
  latestVersionChangeNotes?: string | null;
}

export interface CreateDocumentRequest {
  title: string;
  description?: string | null;
  documentTypeId?: string | null;
  priority?: string | null;
  leaveType?: string | null;
  leaveStartDate?: string | null;
  leaveEndDate?: string | null;
  amount?: number | null;
  budgetCode?: string | null;
  counterparty?: string | null;
  attachmentNote?: string | null;
}

export interface UpdateDocumentRequest {
  title?: string | null;
  description?: string | null;
  documentTypeId?: string | null;
  status?: string | null;
  priority?: string | null;
  approvedAt?: string | null;
  leaveType?: string | null;
  leaveStartDate?: string | null;
  leaveEndDate?: string | null;
  amount?: number | null;
  budgetCode?: string | null;
  counterparty?: string | null;
  attachmentNote?: string | null;
}

// API functions
export const getDocuments = (): Promise<Document[]> =>
  apiClient.get("/documents");

export const getMyDocuments = (): Promise<Document[]> =>
  apiClient.get("/documents/my");

export const getDocument = (id: string): Promise<Document> =>
  apiClient.get(`/documents/${id}`);

export const getMyDocument = (id: string): Promise<Document> =>
  apiClient.get(`/documents/my/${id}`);

export const createDocument = (
  data: CreateDocumentRequest,
): Promise<Document> => apiClient.post("/documents", data);

export const updateDocument = (
  id: string,
  data: UpdateDocumentRequest,
): Promise<Document> => apiClient.put(`/documents/${id}`, data);

export const deleteDocument = (id: string): Promise<void> =>
  apiClient.delete(`/documents/${id}`);
