<template>
  <div class="container">
    <h1>Events</h1>
    <div v-for="event in events" :key="event.id" class="card mb-3">
      <div class="card-body">
        <h2 @click="$router.push(`/event/${event.id}`)" class="card-title">
          {{ event.name }}
        </h2>
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
          :to="{ name: `Register`, params: { id: event.id } }"
          class="btn btn-primary"
          :class="{
            disabled:
              event.programs.length < 1 ||
              (event.programs.length == 1 &&
                event.programs[0].count >= event.programs[0].limit),
          }"
          >Anmelden</router-link
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

    return { events };
  },
});
</script>

<style scoped>
h2:hover {
  text-decoration: underline;
  cursor: pointer;
}
h1 {
  margin-top: 12px;
}
.disabled {
  /* color: lightgray; */
  opacity: 0.5;
  pointer-events: none;
}
</style>
