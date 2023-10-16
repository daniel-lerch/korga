import client from "./client";

export interface ProfileResponse {
  id: string;
  givenName: string;
  familyName: string;
  emailAddress: string;
}

export default {
  getProfile(): Promise<ProfileResponse | null> {
    return client.get("/api/profile");
  },

  async challengeLogin() {
    await client.getResponse("/api/challenge");
  },

  async logout() {
    await client.getResponse("/api/logout");
  },
};
