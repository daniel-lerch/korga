<template>
  <div class="container">
    <h1>Events</h1>
    <div v-for="event in events" :key="event.id" class="card mb-3 shadow">
      <div class="card-body">
        <router-link
          class="h2 card-title subdued"
          :to="{ name: 'Event', params: { id: event.id } }"
        >
          {{ event.name }}
        </router-link>
        <ul>
          <li
            v-for="program in event.programs"
            :key="program.id"
            :class="{ disabled: program.count >= program.limit }"
          >
            {{ program.name }} {{ program.count }}/{{ program.limit }}
          </li>
        </ul>
        <router-link
          :to="{ name: 'Register', params: { id: event.id } }"
          class="btn btn-success btn-emphasize mt-3 w-100"
          :class="{
            disabled: full(event),
          }"
        >
          Anmelden</router-link
        >
      </div>
    </div>
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

    const full = function (event: EventResponse) {
      if (event === null) return false;
      for (const program of event.programs) {
        if (program.count < program.limit) return false;
      }
      return true;
    };

    return { events, full };
  },
});
</script>

<style scoped>
.h2 {
  display: block;
}
.disabled {
  /* color: lightgray; */
  opacity: 0.5;
  pointer-events: none;
}

.container {
  max-width: 800px;
}
</style>
