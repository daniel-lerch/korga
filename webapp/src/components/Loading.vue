<template>
  <div class="page-load-container container">
    <div v-if="state.error === null" class="spinner-border" role="status"></div>
    <div v-else class="alert alert-danger" role="alert">
      {{ state.error.toString() }}
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";

export default defineComponent({
  props: {
    state: {
      type: Object as PropType<{ error: Error | string | null }>,
      required: true,
    },
  },
});
</script>
