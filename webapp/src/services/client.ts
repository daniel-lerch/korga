import { UserManager } from "oidc-client-ts";

const baseUrl = import.meta.env.PROD
  ? window.basePath.slice(0, -1)
  : import.meta.env.VITE_API_BASE_PATH.slice(0, -1)

let userManager: UserManager | null = null

const getInfo: RequestInit = {
  credentials: "include",
}
const postInfo: RequestInit = {
  method: "POST",
  credentials: "include",
}
const deleteInfo: RequestInit = {
  method: "DELETE",
  credentials: "include",
  headers: {
    "Content-Type": "application/json",
  },
}

export default {
  async userManager() {
    if (userManager != null) {
      return userManager
    }
    const response = await fetch(baseUrl + "/api/settings")
    if (response.ok === false) {
      throw new Error("Unexpected status code " + response.status)
    }
    const settings = await response.json() as { oidcAuthority: string, oidcClientId: string, oidcRedirectUri: string }
    userManager = new UserManager({
      authority: settings.oidcAuthority,
      client_id: settings.oidcClientId,
      metadata: {
        authorization_endpoint: settings.oidcAuthority + "/oauth/authorize",
        token_endpoint: settings.oidcAuthority + "/oauth/access_token",
        userinfo_endpoint: settings.oidcAuthority + "/oauth/userinfo",
      },
      redirect_uri: settings.oidcRedirectUri,
    })
    return userManager
  },
  async get<T>(path: string) {
    const userManager = await this.userManager()
    const user = await userManager.getUser()
    const response = await fetch(baseUrl + path, {
      headers: { Authorization: `Bearer ${user?.access_token}` },
    })
    //const response = await fetch(baseUrl + path, getInfo)
    if (response.ok === false) {
      if (response.status === 401) {
        const responseData = await response.json()
        window.location.href = responseData.openIdConnectRedirectUrl
      }
      throw new Error("Unexpected status code " + response.status)
    }
    return (await response.json()) as T
  },
  async getResponse(path: string) {
    const userManager = await this.userManager()
    const user = await userManager.getUser()
    const response = await fetch(baseUrl + path, {
      headers: { Authorization: `Bearer ${user?.access_token}` },
    })
    //const response = await fetch(baseUrl + path, getInfo)
    if (response.ok === false) {
      if (response.status === 401) {
        const responseData = await response.json()
        window.location.href = responseData.openIdConnectRedirectUrl
      }
      throw new Error("Unexpected status code " + response.status)
    }
    return response
  },
  async post(path: string, data: object) {
    const response = await fetch(baseUrl + path, {
      ...postInfo,
      body: JSON.stringify(data),
    })
    if (response.ok === true) {
      return true
    } else if (response.status === 409) {
      return false
    } else {
      throw new Error("Unexpected status code " + response.status)
    }
  },
  async delete(path: string) {
    const response = await fetch(baseUrl + path, deleteInfo)
    if (response.ok === false) {
      throw new Error("Unexpected status code " + response.status)
    }
  },
}
