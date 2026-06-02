import { apiClient } from "./client";

export interface DocumentCategory {
  id: string;
  name: string;
  description: string;
  createdAt: string;
}

export interface DocumentType {
  id: string;
  name: string;
  description: string;
  categoryId: string;
  categoryName: string;
  requiresApproval: boolean;
  createdAt: string;
}

export interface CreateDocumentCategoryRequest {
  name: string;
  description: string;
}

export interface UpdateDocumentCategoryRequest {
  name: string;
  description: string;
}

export interface CreateDocumentTypeRequest {
  name: string;
  description: string;
  categoryId: string;
  requiresApproval: boolean;
}

export interface UpdateDocumentTypeRequest {
  name: string;
  description: string;
  categoryId: string;
  requiresApproval: boolean;
}

export const getDocumentCategories = (): Promise<DocumentCategory[]> =>
  apiClient.get("/document-categories");

export const getDocumentTypes = (): Promise<DocumentType[]> =>
  apiClient.get("/document-types");

export const createDocumentCategory = (
  data: CreateDocumentCategoryRequest,
): Promise<DocumentCategory> =>
  apiClient.post("/document-categories", data);

export const updateDocumentCategory = (
  id: string,
  data: UpdateDocumentCategoryRequest,
): Promise<DocumentCategory> =>
  apiClient.put(`/document-categories/${id}`, data);

export const deleteDocumentCategory = (id: string): Promise<void> =>
  apiClient.delete(`/document-categories/${id}`);

export const createDocumentType = (
  data: CreateDocumentTypeRequest,
): Promise<DocumentType> =>
  apiClient.post("/document-types", data);

export const updateDocumentType = (
  id: string,
  data: UpdateDocumentTypeRequest,
): Promise<DocumentType> =>
  apiClient.put(`/document-types/${id}`, data);

export const deleteDocumentType = (id: string): Promise<void> =>
  apiClient.delete(`/document-types/${id}`);
