import client from "./client";

export interface Services {
  id: number;
  name: string;
  serviceGroup: string | null;
}

export interface ServiceHistory {
  personId: number;
  firstName: string;
  lastName: string;
  groupMemberStatus: "Active" | "Requested" | "ToDelete";
  serviceDates: string[];
}

export function getServices(): Promise<Services[]> {
  return client.get("/api/services");
}

export function getServiceHistory(id: number): Promise<ServiceHistory[]> {
  return client.get(`/api/services/${id}/history`);
}
