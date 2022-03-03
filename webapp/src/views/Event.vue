<template>
  <h1>{{ event?.name }}</h1>
  <input
    type="text"
    v-model="givenName"
    id="givenName"
    class="form-control"
    placeholder="Vorname"
    required
  />
  <input
    type="text"
    v-model="familyName"
    id="familyName"
    class="form-control"
    placeholder="Nachname"
    required
  />
</template>

<script lang="ts">
import { EventResponse2, getEvent } from "@/services/event";
import { defineComponent, onMounted, ref } from "vue";

export default defineComponent({
  props: {
    id: {
      type: String,
      required: true,
    },
  },
  setup(props) {
    const event = ref<EventResponse2 | null>(null);
    const givenName = ref("");
    const familyName = ref("");
    onMounted(async () => {
      event.value = await getEvent(props.id);
    });

    return { event, givenName, familyName };
  },
});
</script>

<style></style>
