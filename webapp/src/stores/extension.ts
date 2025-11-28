import type { Person } from "@/utils/ct-types"
import {
  createCustomDataCategory,
  getCustomDataCategories,
  getModule,
  updateCustomDataCategory,
} from "@/utils/kv-store"
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { defineStore } from "pinia"

export const useExtensionStore = defineStore("extension", {
  state: () => ({
    moduleId: 0,
    backendUrl: "",
    accessToken: "",
  }),
  actions: {
    async load() {
      const username = import.meta.env.VITE_USERNAME
      const password = import.meta.env.VITE_PASSWORD
      if (import.meta.env.DEV && username && password) {
        await churchtoolsClient.post("/login", {
          username,
          password,
        })
      }

      const module = await getModule()
      this.moduleId = module.id

      const categories = await getCustomDataCategories<{ backendUrl?: string }>(module.id)
      const configCategory = categories.find((c) => c.shorty === "config")
      this.backendUrl = configCategory?.backendUrl ?? ""
      // Try to restore access token from session storage so users don't have to
      // re-login on page reload during the same browser session.
      const stored = sessionStorage.getItem("korga.accessToken")
      if (stored) {
        this.accessToken = stored
      }
    },
    async authenticate(backendUrl: string): Promise<string> {
      const user = await churchtoolsClient.get<Person>("/whoami")
      const loginToken = await churchtoolsClient.get<string>(`/persons/${user.id}/logintoken`)
      const response = await fetch(`${backendUrl}/api/token`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          churchToolsUrl: window.settings?.base_url ?? import.meta.env.VITE_CHURCHTOOLS_URL,
          loginToken,
        }),
      })
      if (!response.ok) {
        throw new Error(
          `Login failed. ${backendUrl} replied with error ${response.status} ${response.statusText}`
        )
      }
      return (await response.json()).accessToken
    },
    async login(backendUrl?: string) {
      backendUrl = backendUrl ?? this.backendUrl
      if (!backendUrl) {
        throw new Error("Invalid operation: Cannot login when backendUrl is not set")
      }
      this.accessToken = await this.authenticate(backendUrl)
      sessionStorage.setItem("korga.accessToken", this.accessToken)
    },
    async setBackendUrl(backendUrl: string) {
      if (backendUrl !== this.backendUrl) {
        // Successful login, save backendUrl in ChurchTools value store
        const categories = await getCustomDataCategories(this.moduleId)
        const configCategory = categories.find((c) => c.shorty === "config")
        if (configCategory === undefined) {
          await createCustomDataCategory(
            {
              customModuleId: this.moduleId,
              name: "Configuration",
              shorty: "config",
              description: "Configuration for the Korga extension",
              data: JSON.stringify({ backendUrl }),
            },
            this.moduleId
          )
        } else {
          await updateCustomDataCategory(
            configCategory.id,
            {
              id: configCategory.id,
              customModuleId: this.moduleId,
              name: configCategory.name,
              shorty: configCategory.shorty,
              description: configCategory.description,
              data: JSON.stringify({
                backendUrl,
              }),
            },
            this.moduleId
          )
        }
        this.backendUrl = backendUrl
      }
    },
  },
})
