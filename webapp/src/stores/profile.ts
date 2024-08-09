import type { ProfileResponse } from "@/services/profile"
import { defineStore } from "pinia"
import { ref } from "vue"

export const useProfileStore = defineStore("profile", () => {
  const profile = ref<ProfileResponse | null>(null)
  return { profile }
})
