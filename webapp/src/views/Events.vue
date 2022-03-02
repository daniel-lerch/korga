<template>
  <h1>Events</h1>
  <div v-for="event in events" :key="event.id">
    <h2>{{ event.name }}</h2>
    <div v-for="program in event.programs" :key="program.id">
      <h5>{{ program.name }}</h5>
    </div>
    <router-link
      :to="{ name: 'Event', params: { id: event.id } }"
      class="btn btn-primary"
      >Anmelden</router-link
    >
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import { EventResponse, getEvents } from "@/services/event";

export default defineComponent({
  setup() {
    const events = ref<EventResponse[]>([]);

    onMounted(async () => {
      events.value.push(...(await getEvents()));
      // console.log(events.value)
    });

    return { events };
  },
});
</script>

<style></style>
