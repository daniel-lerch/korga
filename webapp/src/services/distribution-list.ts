import { get } from "./client";

export interface DistributionList {
  id: number;
  alias: string;
  filters: PersonFilter[];
}

export interface PersonFilter {
  id: number;
  discriminator: string;
  statusName: string | null;
  groupName: string | null;
  groupRoleName: string | null;
  personFullName: string | null;
}

export function getDistributionLists() {
  return get<DistributionList[]>("/api/distribution-lists");
}
