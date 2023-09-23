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
        <li><a class="dropdown-item" href="#">Abmelden</a></li>
      </ul>
    </li>
    <form v-else @submit.prevent="login">
      <button class="btn btn-outline-light" type="submit">Login</button>
    </form>
  </ul>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import {
  challengeLogin,
  getProfile,
  ProfileResponse,
} from "@/services/profile";

export default defineComponent({
  setup() {
    const profile = ref<ProfileResponse | null>(null);

    onMounted(async () => {
      try {
        profile.value = await getProfile();
      } catch (error) {
        profile.value = null;
      }
    });

    async function login() {
      try {
        await challengeLogin();
      } catch (error) {
        profile.value = null;
      }
    }

    return { login, profile };
  },
});
</script>
