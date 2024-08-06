import client from "./client"

export interface ProfileResponse {
  id: string
  givenName: string
  familyName: string
  emailAddress: string
  permissions: {
    Permissions_View: boolean
    Permissions_Admin: boolean
    DistributionLists_View: boolean
    DistributionLists_Admin: boolean
    ServiceHistory_View: boolean
  }
}

export default {
  async getProfile(): Promise<ProfileResponse | null> {
    return await client.get("/api/profile")
  },

  async challengeLogin() {
    await client.getResponse("/api/challenge")
  },

  async logout() {
    await client.getResponse("/api/logout")
  },
}
