import client from "./client"

export interface Service {
  id: number
  name: string
  serviceGroup: string | null
}

export interface ServiceHistory {
  personId: number
  firstName: string
  lastName: string
  groups: {
    groupId: number
    groupName: string
    groupMemberStatus: "Active" | "Requested" | "To_Delete"
    comment: string
  }[]
  serviceDates: ServiceDate[]
}

export interface ServiceDate {
  serviceId: number
  date: string
}

export function getServices(): Promise<Service[]> {
  return client.get("/api/services")
}

export function getServiceHistory(ids: number[]): Promise<ServiceHistory[]> {
  return client.get(
    `/api/services/history/?${ids.map((id) => `serviceId=${id}`).join("&")}`
  )
}
