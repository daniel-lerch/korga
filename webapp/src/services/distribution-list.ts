import { useExtensionStore } from "@/stores/extension"

export interface DistributionList {
  id: number
  alias: string
  newsletter: boolean
  permittedRecipients: PersonFilter[]
}

export interface PersonFilter {
  id: number
  discriminator: string
  statusName: string | null
  groupName: string | null
  groupRoleName: string | null
  groupTypeName: string | null
  personFullName: string | null
}

export async function getDistributionLists(): Promise<DistributionList[]> {
  const extension = useExtensionStore()
  let response = await fetch(`${extension.backendUrl}/api/distribution-lists`, {
    headers: {
      Authorization: `Bearer ${extension.accessToken}`,
    },
  })
  if (response.status === 401) {
    // Access token invalid, try to re-login
    await extension.login()
    response = await fetch(`${extension.backendUrl}/api/distribution-lists`, {
      headers: {
        Authorization: `Bearer ${extension.accessToken}`,
      },
    })
    if (response.status === 401)
      throw new Error("Unauthorized: Access token is invalid even after re-login")
  }
  return await response.json()
}
