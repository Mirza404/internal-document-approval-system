import { useCallback, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminApi, type DocumentType, type CreateDocumentTypeRequest, type UpdateDocumentTypeRequest } from '../api/admin';

export const useDocumentTypes = () => {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const query = useQuery({
    queryKey: ['document-types'],
    queryFn: async () => {
      const response = await adminApi.getDocumentTypes();
      return response.data;
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateDocumentTypeRequest) => adminApi.createDocumentType(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['document-types'] });
      setError(null);
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to create document type');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDocumentTypeRequest }) =>
      adminApi.updateDocumentType(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['document-types'] });
      setError(null);
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to update document type');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => adminApi.deleteDocumentType(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['document-types'] });
      setError(null);
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to delete document type');
    },
  });

  const createDocumentType = useCallback((data: CreateDocumentTypeRequest) => {
    return createMutation.mutateAsync(data);
  }, [createMutation]);

  const updateDocumentType = useCallback((id: string, data: UpdateDocumentTypeRequest) => {
    return updateMutation.mutateAsync({ id, data });
  }, [updateMutation]);

  const deleteDocumentType = useCallback((id: string) => {
    return deleteMutation.mutateAsync(id);
  }, [deleteMutation]);

  return {
    documentTypes: query.data || [],
    isLoading: query.isLoading,
    error,
    createDocumentType,
    updateDocumentType,
    deleteDocumentType,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
};
