<template>
  <ul class="navbar-nav">
    <li v-if="store.profile !== null" class="nav-item dropdown">
      <button
        class="btn btn-link nav-link dropdown-toggle"
        type="button"
        data-bs-toggle="dropdown"
      >
        {{ store.profile.givenName }}
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

<script setup lang="ts">
import { useProfileStore } from "@/stores/profile"
import korga from "@/services/profile"

const store = useProfileStore()

async function login() {
  try {
    await korga.challengeLogin()
  } catch (error) {
    console.log(error)
  }
}

async function logout() {
  try {
    await korga.logout()
  } catch (error) {
    console.log(error)
  }
}
</script>
