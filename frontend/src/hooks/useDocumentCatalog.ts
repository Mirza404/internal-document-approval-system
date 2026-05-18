import { useQuery } from "@tanstack/react-query";
import {
  getDocumentCategories,
  getDocumentTypes,
  seededDocumentTypes,
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
