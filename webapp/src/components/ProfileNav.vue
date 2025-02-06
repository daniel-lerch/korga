<template>
  <ul class="navbar-nav">
    <li v-if="user.profile" class="nav-item dropdown">
      <button class="btn btn-link nav-link dropdown-toggle py-0" type="button" data-bs-toggle="dropdown">
        <img :src="user.profile.picture ?? undefined" alt="Profile Picture" class="profile-picture" />
      </button>
      <ul class="dropdown-menu dropdown-menu-end">
        <li class="dropdown-header">{{ user.profile.displayName }}</li>
        <li>
          <button class="dropdown-item" @click.prevent="user.logout">
            Abmelden
          </button>
        </li>
      </ul>
    </li>
    <button v-else-if="user.profile === null" class="btn btn-outline-light" @click.prevent="user.login">
      Login
    </button>
  </ul>
</template>

<script setup lang="ts">
import { onMounted } from "vue"
import { useUserStore } from "@/stores/user"

const user = useUserStore()

onMounted(() => {
  user.load()
})
</script>

<style scoped>
.profile-picture {
  width: 36px;
  height: 36px;
  border-radius: 50%;
}
</style>
