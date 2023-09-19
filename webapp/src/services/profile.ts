import { get } from "./client";

export interface ProfileResponse {
  id: string;
  givenName: string;
  familyName: string;
  emailAddress: string;
}

export function getProfile(): Promise<ProfileResponse> {
  return get("/api/profile");
}
