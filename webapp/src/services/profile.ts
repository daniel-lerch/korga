import client from "./client";

export interface ProfileResponse {
  id: string;
  givenName: string;
  familyName: string;
  emailAddress: string;
}

let profile: ProfileResponse | null = null;
export default {
  async getProfile(): Promise<ProfileResponse | null> {
    if (profile != null) {
      return profile;
    }
    profile = await client.get("/api/profile");
    return profile;
  },

  async challengeLogin() {
    await client.getResponse("/api/challenge");
  },

  async logout() {
    await client.getResponse("/api/logout");
    profile = null;
  },
};
