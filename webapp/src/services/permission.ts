import type { PersonFilter } from "./filter";
import client from "./client";

export interface Permission {
  key: "Permissions_View" | "Permissions_Admin" | "DistributionLists_View" | "DistributionLists_Admin" | "ServiceHistory_View"
  personFilters: PersonFilter[]
}

export function getPermissions(): Promise<Permission[]> {
  return client.get("/api/permissions")
}
