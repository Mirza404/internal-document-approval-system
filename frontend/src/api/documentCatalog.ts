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

export const seededDocumentTypes: DocumentType[] = [
  {
    id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    name: "Transcript",
    description: "Official academic transcript request.",
    categoryId: "11111111-1111-1111-1111-111111111111",
    categoryName: "Academic Records",
    requiresApproval: true,
    createdAt: "2026-05-06T00:00:00Z",
  },
  {
    id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    name: "Certificate",
    description: "Enrollment, status, and other student certificate requests.",
    categoryId: "22222222-2222-2222-2222-222222222222",
    categoryName: "Student Services",
    requiresApproval: true,
    createdAt: "2026-05-06T00:00:00Z",
  },
  {
    id: "cccccccc-cccc-cccc-cccc-cccccccccccc",
    name: "Internship Submission",
    description: "Internship approval and supporting document submission.",
    categoryId: "33333333-3333-3333-3333-333333333333",
    categoryName: "Internships",
    requiresApproval: true,
    createdAt: "2026-05-06T00:00:00Z",
  },
  {
    id: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
    name: "Payment Procedure",
    description:
      "Payment procedure request requiring amount and student finance reference.",
    categoryId: "44444444-4444-4444-4444-444444444444",
    categoryName: "Payments",
    requiresApproval: true,
    createdAt: "2026-05-06T00:00:00Z",
  },
];

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
): Promise<DocumentCategory> => apiClient.post("/document-categories", data);

export const updateDocumentCategory = (
  id: string,
  data: UpdateDocumentCategoryRequest,
): Promise<DocumentCategory> =>
  apiClient.put(`/document-categories/${id}`, data);

export const deleteDocumentCategory = (id: string): Promise<void> =>
  apiClient.delete(`/document-categories/${id}`);

export const createDocumentType = (
  data: CreateDocumentTypeRequest,
): Promise<DocumentType> => apiClient.post("/document-types", data);

export const updateDocumentType = (
  id: string,
  data: UpdateDocumentTypeRequest,
): Promise<DocumentType> => apiClient.put(`/document-types/${id}`, data);

export const deleteDocumentType = (id: string): Promise<void> =>
  apiClient.delete(`/document-types/${id}`);
