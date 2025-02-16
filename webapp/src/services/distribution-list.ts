import type { PersonFilter } from "./filter"
import client from "./client"

export interface DistributionList {
  id: number
  alias: string
  newsletter: boolean
  permittedRecipients: PersonFilter[]
}

export function getDistributionLists(): Promise<DistributionList[]> {
  return client.get("/api/distribution-lists")
}
