import { get } from "./client";

export interface GroupResponse {
  id: number;
  name: string;
  description: string;
  roles: {
    id: number;
    name: string;
  }[];
  memberCount: number;
}

export function getGroups(): Promise<GroupResponse[]> {
  return get("/api/groups");
}
