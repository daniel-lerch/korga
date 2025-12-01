<template>
  <div>
    <h2>Mailist Backend</h2>
    <form @submit.prevent="save">
      <input type="text" v-model="backendUrl" placeholder="Backend URL" />
      <button type="submit">Speichern</button>
    </form>
  </div>
  <!-- TODO: Add configuration for ChurchTools user, IMAP, and SMTP -->
</template>

<script setup lang="ts">
import { useExtensionStore } from '@/stores/extension';
import { ref } from 'vue';

const extension = useExtensionStore()

const backendUrl = ref(extension.backendUrl);

async function save() {
  try {
    extension.login(backendUrl.value)
  } catch (error) {
    alert("Fehler bei der Verbindung: " + error)
  }
}

</script>