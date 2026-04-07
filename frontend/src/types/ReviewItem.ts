import type { Stage } from "./Stage";
import type { Priority } from "./Priority";
export type ReviewItem = {
  id: string;
  title: string;
  owner: string;
  stage: Stage;
  due: string;
  priority: Priority;
  department: string;
  updated: string;
};