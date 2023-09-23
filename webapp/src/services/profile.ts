import client from "./client";

export interface ProfileResponse {
  id: string;
  givenName: string;
  familyName: string;
  emailAddress: string;
}

export function getProfile(): Promise<ProfileResponse | null> {
  return client.get("/api/profile");
}

export async function challengeLogin() {
  await client.getResponse("/api/challenge");
}
