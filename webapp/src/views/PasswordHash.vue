<template>
  <div class="container">
    <h1>Passwort erstellen</h1>
    <div class="card shadow">
      <div class="card-body">
        <form @submit.prevent>
          <div class="mb-3">
            <label for="newPassword" class="form-label">Neues Passwort</label>
            <input
              type="password"
              id="newPassword"
              v-model="newPassword"
              class="form-control"
            />
          </div>
          <div v-if="!passwordAcceptable" class="alert alert-warning">
            Dein Passwort ist zu unsicher. Verwende ein längeres Passwort im
            Idealfall oder mit Groß- und Kleinbuchstaben, Zahlen und
            Sonderzeichen.
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
          <div v-if="!passwordsMatch" class="alert alert-danger">
            Passwörter stimmen nicht überein.
          </div>
          <div v-if="passwordAcceptable && passwordsMatch">
            <label for="passwordHash" class="form-label">Passwort Hash</label>
            <div class="input-group">
              <input
                id="passwordHash"
                class="form-control"
                v-model="passwordHash"
                disabled
              />
              <button
                type="button"
                class="btn btn-outline-secondary"
                @click="copyToClipboard"
              >
                📋
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from "vue";
import { computed } from "@vue/reactivity";
import { computedAsync } from "@vueuse/core";
import entropy from "ideal-password";
import { ssha } from "../services/hash";

export default defineComponent({
  setup() {
    const newPassword = ref("");
    const confirmPassword = ref("");

    const passwordAcceptable = computed(
      () => entropy(newPassword.value).acceptable
    );

    const passwordsMatch = computed(
      () => newPassword.value === confirmPassword.value
    );

    const passwordHash = computedAsync(
      async () => await ssha(newPassword.value)
    );

    function copyToClipboard() {
      if (navigator.clipboard)
        navigator.clipboard.writeText(passwordHash.value);
      else
        alert(
          "Dein Browser unterstützt die Zwischenablage nicht. Du musst den Text manuell kopieren."
        );
    }

    return {
      newPassword,
      confirmPassword,
      passwordAcceptable,
      passwordsMatch,
      passwordHash,
      copyToClipboard,
    };
  },
});
</script>
