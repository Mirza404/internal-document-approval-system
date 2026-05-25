import api from './axios';

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Employee' | 'Approver';
  department?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: 'Admin' | 'Employee' | 'Approver';
  department?: string;
}

export interface UpdateUserRequest {
  fullName?: string;
  email?: string;
  role?: 'Admin' | 'Employee' | 'Approver';
  department?: string;
  isActive?: boolean;
}

export interface DocumentType {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
}

export interface CreateDocumentTypeRequest {
  name: string;
  description?: string;
}

export interface UpdateDocumentTypeRequest {
  name?: string;
  description?: string;
}

// User endpoints
export const adminApi = {
  // Users
  getUsers: () => api.get<User[]>('/api/users'),
  getUserById: (id: string) => api.get<User>(`/api/users/${id}`),
  createUser: (data: CreateUserRequest) => api.post<User>('/api/users', data),
  updateUser: (id: string, data: UpdateUserRequest) => api.put<User>(`/api/users/${id}`, data),
  deleteUser: (id: string) => api.delete(`/api/users/${id}`),

  // Document Types
  getDocumentTypes: () => api.get<DocumentType[]>('/api/document-types'),
  getDocumentTypeById: (id: string) => api.get<DocumentType>(`/api/document-types/${id}`),
  createDocumentType: (data: CreateDocumentTypeRequest) => api.post<DocumentType>('/api/document-types', data),
  updateDocumentType: (id: string, data: UpdateDocumentTypeRequest) => api.put<DocumentType>(`/api/document-types/${id}`, data),
  deleteDocumentType: (id: string) => api.delete(`/api/document-types/${id}`),
};
