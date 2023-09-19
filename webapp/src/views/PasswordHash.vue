<template>
  <LoadingSpinner v-if="state === 'LOADING'" :state="{ error }" />
  <div v-else class="container page-loaded-container">
    <h1>Hallo {{ userData?.givenName }}</h1>
    <div class="card shadow">
      <div class="card-body">
        <form v-if="state !== 'SENT'" @submit.prevent="sendHash">
          <div class="mb-3">
            <label for="newPassword" class="form-label">Neues Passwort</label>
            <input
              type="password"
              id="newPassword"
              v-model="newPassword"
              class="form-control"
              :disabled="state === 'SENDING'"
            />
          </div>
          <div v-if="!newPassword" class="alert alert-secondary">
            Dein Passwort muss mindestens 8 Zeichen von mindestens zwei der
            folgenden Kategorien enthalten Großbuchstaben, Kleinbuchstaben,
            Zahlen und Sonderzeichen.
          </div>
          <div v-else-if="!passwordAcceptable" class="alert alert-warning">
            Dein Passwort ist zu unsicher. Verwende ein längeres Passwort im
            Idealfall mit Groß- und Kleinbuchstaben, Zahlen und Sonderzeichen.
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
              :disabled="state === 'SENDING'"
            />
          </div>
          <div
            v-if="confirmPassword.length > 0 && !passwordsMatch"
            class="alert alert-danger"
          >
            Passwörter stimmen nicht überein.
          </div>
          <button
            v-if="passwordAcceptable && passwordsMatch"
            type="submit"
            class="btn btn-primary mb-3"
            :disabled="state === 'SENDING'"
          >
            Passwort setzen
          </button>
        </form>
        <div v-if="state === 'SENT'" class="alert alert-success">
          Das Passwort wurde erfolgreich geändert.
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from "vue";
import entropy from "ideal-password";
import { ssha, postHash, checkToken, TokenData } from "../services/hash";
import { useRoute } from "vue-router";
import LoadingSpinner from "@/components/LoadingSpinner.vue";

export default defineComponent({
  components: { LoadingSpinner },
  setup() {
    const state = ref<"LOADING" | "LOADED" | "SENDING" | "SENT">("LOADING");
    const error = ref<string | null>(null);

    const token = ref("");
    const userData = ref<TokenData | null>(null);

    const newPassword = ref("");
    const confirmPassword = ref("");
    const passwordAcceptable = computed(
      () => entropy(newPassword.value).acceptable
    );
    const passwordsMatch = computed(
      () => newPassword.value === confirmPassword.value
    );

    onMounted(() => {
      const route = useRoute();
      token.value = route.query.token as string;
      validateToken();
    });

    async function validateToken() {
      try {
        const res = await checkToken(token.value);
        userData.value = res;
        state.value = "LOADED";
      } catch (e) {
        error.value =
          "Dieser Link ist ungültig oder abgelaufen. Bitte wende dich an den Administrator.";
      }
    }

    async function sendHash() {
      state.value = "SENDING";
      try {
        const hash = await ssha(newPassword.value);
        const res = await postHash({
          token: token.value,
          passwordHash: hash,
        });
        if (res) {
          state.value = "SENT";
        }
      } catch (e) {
        // TODO: Show a toast notification with error information
        state.value = "LOADED";
      }
    }

    return {
      state,
      error,
      newPassword,
      confirmPassword,
      passwordAcceptable,
      passwordsMatch,
      token,
      userData,
      sendHash,
      validateToken,
    };
  },
});
</script>

<style scoped>
.container {
  max-width: 800px;
}
</style>
