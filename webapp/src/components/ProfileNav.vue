<template>
  <ul class="navbar-nav">
    <li
      v-if="user !== null && user.expired === false"
      class="nav-item dropdown"
    >
      <button
        class="btn btn-link nav-link dropdown-toggle"
        type="button"
        data-bs-toggle="dropdown"
      >
        {{ user.profile.given_name }}
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
import type { User } from "oidc-client-ts"
//import type { ProfileResponse } from "@/services/profile"
import { defineComponent, onMounted, ref } from "vue"
//import korga from "@/services/profile"
import { useUserManagerStore } from "@/services/usermanager"

export default defineComponent({
  setup() {
    //const profile = ref<ProfileResponse | null>(null)
    const user = ref<User | null>(null)
    const store = useUserManagerStore()

    onMounted(async () => {
      user.value = await store.userManager.getUser()
      //try {
      //  profile.value = await korga.getProfile()
      //} catch (error) {
      //  profile.value = null
      //}
    })

    async function login() {
      await store.userManager.signinRedirect()
      //try {
      //  await korga.challengeLogin()
      //} catch (error) {
      //  profile.value = null
      //}
    }

    async function logout() {
      await store.userManager.signoutRedirect()
      //try {
      //  await korga.logout()
      //} catch (error) {
      //  profile.value = null
      //}
    }

    return { login, logout, user }
  },
})
</script>
