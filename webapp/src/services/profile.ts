import client from "./client"

export interface ProfileResponse {
  id: string
  displayName: string
  givenName: string
  familyName: string
  emailAddress: string
  picture: string | null
}

export function getProfile(): Promise<ProfileResponse | null> {
  return client.get("/api/profile")
}

export function login() {
  const params = new URLSearchParams({ redirect: window.location.pathname }).toString()
  window.location.href = client.baseUrl + "/api/challenge?" + params
}

export function logout() {
  return client.getResponse("/api/logout")
}
