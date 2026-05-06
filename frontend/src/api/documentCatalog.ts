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

export const getDocumentCategories = (): Promise<DocumentCategory[]> =>
  apiClient.get("/document-categories");

export const getDocumentTypes = (): Promise<DocumentType[]> =>
  apiClient.get("/document-types");
