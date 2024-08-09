<template>
  <div class="container">
    <h1>Korga</h1>

    <div
      class="card"
      v-if="
        store.profile?.permissions.DistributionLists_View ||
        store.profile?.permissions.DistributionLists_Admin
      "
    >
      <div class="card-body">
        <h5 class="card-title">Verteilerlisten</h5>
        <p class="card-text">
          Mit Verteilerlisten kannst du Einzelpersonen, Gruppen oder alle
          Personen eines Status per E-Mail erreichen. Praktisch für Rundmails,
          gemeinsam genutzte Accounts und mehr.
        </p>
        <router-link
          :to="{ name: 'DistributionLists' }"
          class="btn btn-primary"
        >
          Verteilerlisten ansehen
        </router-link>
      </div>
    </div>

    <div class="card" v-if="store.profile?.permissions.ServiceHistory_View">
      <div class="card-body">
        <h5 class="card-title">Diensthistorie</h5>
        <p class="card-text">
          Die Diensthistorie zeigt dir, wer im letzten Jahr wie oft für welchen
          Dienst eingeteilt war. Das hilft dir bei der Dienstplanung, um Dienste
          gleichmäßig zu vergeben.
        </p>
        <router-link :to="{ name: 'ServiceHistory' }" class="btn btn-primary">
          Diensthistorie ansehen
        </router-link>
      </div>
    </div>

    <div
      class="card"
      v-if="
        store.profile?.permissions.Permissions_View ||
        store.profile?.permissions.Permissions_Admin
      "
    >
      <div class="card-body">
        <h5 class="card-title">Berechtigungen</h5>
        <p class="card-text">
          Mit Berechtigungen kannst du festlegen, wer welche Daten in Korga
          sehen und verändern darf. Das ist wichtig um sensible Daten zu
          schützen.
        </p>
        <router-link :to="{ name: 'Permissions' }" class="btn btn-primary">
          Berechtigungen ansehen
        </router-link>
      </div>
    </div>

    <button
      v-if="store.profile === null"
      class="btn btn-primary"
      @click.prevent="login"
    >
      Mit ChurchTools Anmelden
    </button>
    <div
      v-else-if="
        !Object.values(store.profile.permissions).some((obj) => obj === true)
      "
    >
      Ganz schön leer hier, oder? Das liegt daran, dass du noch keine
      Berechtigungen hast. Bitte wende dich an den Administrator, um Zugriff zu
      erhalten.
    </div>
  </div>
</template>

<script setup lang="ts">
import { useProfileStore } from "@/stores/profile"
import korga from "@/services/profile"

async function login() {
  try {
    await korga.challengeLogin()
  } catch (error) {
    console.log(error)
  }
}

const store = useProfileStore()
</script>
