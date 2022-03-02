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
    },
  },
  setup(props) {
    const event = ref<EventResponse2 | null>(null);
    const givenName = ref("");
    const familyName = ref("");
    console.log(props.id);
    onMounted(async () => {
      event.value = await getEvent("1"); // props.id
      // console.log(props.id)
    });

    return { event, givenName, familyName };
  },
});
</script>

<style></style>
