import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getDocumentCategories,
  getDocumentTypes,
  seededDocumentTypes,
  createDocumentCategory,
  updateDocumentCategory,
  deleteDocumentCategory,
  createDocumentType,
  updateDocumentType,
  deleteDocumentType,
} from "../api/documentCatalog";

export const useDocumentCategories = () => {
  return useQuery({
    queryKey: ["document-categories"],
    queryFn: getDocumentCategories,
  });
};

export const useDocumentTypes = () => {
  return useQuery({
    queryKey: ["document-types"],
    queryFn: getDocumentTypes,
    initialData: seededDocumentTypes,
  });
};

export const useCreateDocumentCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createDocumentCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["document-categories"] });
    },
  });
};

export const useUpdateDocumentCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: Parameters<typeof updateDocumentCategory>[1];
    }) => updateDocumentCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["document-categories"] });
    },
  });
};

export const useDeleteDocumentCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteDocumentCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["document-categories"] });
    },
  });
};

export const useCreateDocumentType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createDocumentType,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["document-types"] });
    },
  });
};

export const useUpdateDocumentType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: Parameters<typeof updateDocumentType>[1];
    }) => updateDocumentType(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["document-types"] });
    },
  });
};

export const useDeleteDocumentType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteDocumentType,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["document-types"] });
    },
  });
};
