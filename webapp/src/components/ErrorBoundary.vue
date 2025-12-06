<template>
  <div v-if="error">
    <Panel header="Fehler" class="mx-2 my-8">
      <p class="mb-2">{{ error.message }}</p>
      <Button label="Reload" icon="pi pi-refresh" @click="reload" />
    </Panel>
  </div>
  <slot v-else></slot>
</template>

<script setup lang="ts">
import Panel from "primevue/panel"
import Button from "primevue/button"
import { onErrorCaptured, ref, watch } from 'vue';
import { useRoute } from "vue-router";

const route = useRoute()

const error = ref<Error | null>(null);

onErrorCaptured((err) => {
  error.value = err;
  // Return false to stop the error from propagating further
  return false;
})

function reload() {
  error.value = null;
}

// I would like to reset the error boundary whenever a user clicks a router-link.
// However, Vue Router only provides callbacks for navigations with path changes.
watch(() => route.path, () => reload())
</script>