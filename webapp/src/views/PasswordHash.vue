<template>
  <div v-if="loading">
    <h1>Validiere Token</h1>
  </div>
  <div v-else>
    <div class="container" v-if="userData">
      <h1>Hallo {{ userData.givenName }}</h1>
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
                :disabled="savedSuccessfully"
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
                :disabled="savedSuccessfully"
              />
            </div>
            <div v-if="!passwordsMatch" class="alert alert-danger">
              Passwörter stimmen nicht überein.
            </div>
            <div v-if="passwordAcceptable && passwordsMatch" class="mb-3">
              <button
                type="button"
                class="btn btn-primary"
                @click="sendHash"
                :disabled="savedSuccessfully"
              >
                Save
              </button>
            </div>
            <div v-if="savedSuccessfully" class="alert alert-success">
              Das Passwort wurde erfolgreich geändert.
            </div>
          </form>
        </div>
      </div>
    </div>
    <div v-else class="container">
      <h1>Token ungültig</h1>
      <div class="card shadow">
        <div class="card-body">
          <p>
            Der Token ist ungültig oder abgelaufen. Bitte wende dich an den
            Administrator.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import { computed } from "@vue/reactivity";
import { computedAsync } from "@vueuse/core";
import entropy from "ideal-password";
import { ssha, postHash, checkToken, TokenData } from "../services/hash";
import { useRoute } from "vue-router";

export default defineComponent({
  setup() {
    const newPassword = ref("");
    const confirmPassword = ref("");
    const copyToClipboardText = ref("📋");
    const token = ref("");
    const userData = ref<TokenData | null>(null);
    const loading = ref(true);
    const savedSuccessfully = ref(false);

    const passwordAcceptable = computed(
      () => entropy(newPassword.value).acceptable
    );

    const passwordsMatch = computed(
      () => newPassword.value === confirmPassword.value
    );

    const passwordHash = computedAsync(async () => {
      const hash = await ssha(newPassword.value);
      copyToClipboardText.value = "📋";
      return hash;
    });

    async function copyToClipboard() {
      if (navigator.clipboard) {
        await navigator.clipboard.writeText(passwordHash.value);
        copyToClipboardText.value = "✔";
      } else
        alert(
          "Dein Browser unterstützt die Zwischenablage nicht. Du musst den Text manuell kopieren."
        );
    }

    async function sendHash() {
      try {
        const res = await postHash({
          token: token.value,
          passwordHash: passwordHash.value,
        });
        if (res) {
          savedSuccessfully.value = true;
        }
      } catch (e) {
        console.log(e);
      }
    }

    async function validateToken() {
      try {
        const res = await checkToken(token.value);
        userData.value = res;
        loading.value = false;
      } catch (e) {
        console.log("token invalid");
        userData.value = null;
        loading.value = false;
      }
    }

    onMounted(async () => {
      const route = useRoute();
      token.value = route.query.token as string;
      validateToken();
    });

    return {
      newPassword,
      confirmPassword,
      copyToClipboardText,
      passwordAcceptable,
      passwordsMatch,
      passwordHash,
      token,
      userData,
      loading,
      savedSuccessfully,
      copyToClipboard,
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
