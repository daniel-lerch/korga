<template>
  <ul class="navbar-nav">
    <li v-if="profile !== null" class="nav-item dropdown">
      <button
        class="btn btn-link nav-link dropdown-toggle"
        type="button"
        data-bs-toggle="dropdown"
      >
        {{ profile.givenName }}
      </button>
      <ul class="dropdown-menu dropdown-menu-end">
        <li>
          <button class="dropdown-item" @click.prevent="logout">
            Abmelden
          </button>
        </li>
      </ul>
    </li>
    <button v-else class="btn btn-outline-light" @click.prevent="login">
      Login
    </button>
  </ul>
</template>

<script lang="ts">
import type { ProfileResponse } from "@/services/profile"
import { defineComponent, onMounted, ref } from "vue"
import korga from "@/services/profile"

export default defineComponent({
  setup() {
    const profile = ref<ProfileResponse | null>(null)

    onMounted(async () => {
      try {
        profile.value = await korga.getProfile()
      } catch (error) {
        profile.value = null
      }
    })

    async function login() {
      try {
        await korga.challengeLogin()
      } catch (error) {
        profile.value = null
      }
    }

    async function logout() {
      try {
        await korga.logout()
      } catch (error) {
        profile.value = null
      }
    }

    return { login, logout, profile }
  },
})
</script>
