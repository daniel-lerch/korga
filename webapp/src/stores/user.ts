import { getProfile, login, logout, type ProfileResponse } from "@/services/profile";
import { defineStore } from "pinia";

export const useUserStore = defineStore("user", {
  state: () => ({
    profile: undefined as ProfileResponse | null | undefined,
    loading: false,
  }),
  actions: {
    load() {
      if (this.profile === undefined) {
        this.reload()
      }
    },
    reload() {
      if (this.loading === false) {
        this.loading = true
        getProfile().then((profile) => {
          this.profile = profile as ProfileResponse | null
          this.loading = false
        }).catch(() => {
          this.profile = undefined
          this.loading = false
        })
      }
    },
    login() {
      login()
    },
    logout() {
      logout().then(() => {
        this.reload()
      }).catch(() => {
        this.reload()
      })
    },
  }
})
