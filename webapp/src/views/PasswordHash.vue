<template>
  <div class="container">
    <h1>Passwort erstellen</h1>
    <div class="card shadow">
      <div class="card-body">
        <form>
          <div class="mb-3">
            <label for="newPassword" class="form-label">Neues Passwort</label>
            <input
              type="password"
              id="newPassword"
              v-model="newPassword"
              class="form-control"
            />
          </div>
          <div class="mb-3">
            <label for="passwordConfirmation" class="form-label">
              Passwort bestätigen
            </label>
            <input
              type="password"
              id="confirmPassword"
              v-model="confirmPassword"
              class="form-control"
            />
          </div>
        </form>
        <div v-if="passwordsMatch">
          {{ passwordHash }}
        </div>
        <div v-else class="alert alert-danger">
          Passwörter stimmen nicht überein.
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from "vue";
import { encode } from "base64-arraybuffer";
import { computedAsync } from "@vueuse/core";
import { computed } from "@vue/reactivity";

export default defineComponent({
  setup() {
    const newPassword = ref("");
    const confirmPassword = ref("");

    const passwordsMatch = computed(
      () => newPassword.value === confirmPassword.value
    );

    const passwordHash = computedAsync(async () => {
      const password = new TextEncoder().encode(newPassword.value);
      const salt = crypto.getRandomValues(new Uint8Array(4));
      const combined = new Uint8Array(password.length + salt.length);
      combined.set(password);
      combined.set(salt, password.length);
      const hash = await crypto.subtle.digest("SHA-1", combined);
      const hashAndSalt = new Uint8Array(hash.byteLength + salt.length);
      hashAndSalt.set(new Uint8Array(hash));
      hashAndSalt.set(salt, hash.byteLength);
      const base64 = encode(hashAndSalt.buffer);
      return "{SSHA}" + base64;
    });

    return {
      newPassword,
      confirmPassword,
      passwordsMatch,
      passwordHash,
    };
  },
});
</script>
