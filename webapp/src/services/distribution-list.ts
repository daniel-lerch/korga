import client from "./client";

export interface DistributionList {
  id: number;
  alias: string;
  newsletter: boolean;
  permittedRecipients: PersonFilter;
}

export interface PersonFilter {
  id: number;
  discriminator: string;
  children: PersonFilter[];
  statusName: string | null;
  groupName: string | null;
  groupTypeName: string | null;
  groupRoleName: string | null;
  personFullName: string | null;
}

export function getDistributionLists(): Promise<DistributionList[]> {
  return client.get("/api/distribution-lists");
}
