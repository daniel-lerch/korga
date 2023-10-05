<template>
  <div class="container">
    <h1>Events</h1>
    <div v-for="event in events" :key="event.id" class="card mb-3 shadow">
      <div class="card-body">
        <router-link class="h2 card-title subdued" :to="{ name: 'event-id', params: { id: event.id } }">
          {{ event.name }}
        </router-link>
        <ul>
          <li v-for="program in event.programs" :key="program.id" :class="{ disabled: program.count >= program.limit }">
            {{ program.name }} {{ program.count }}/{{ program.limit }}
          </li>
        </ul>
        <router-link :to="{ name: 'event-id-register', params: { id: event.id } }" class="btn btn-success btn-emphasize mt-3 w-100"
          :class="{
            disabled: full(event),
          }">
          Anmelden</router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

interface EventResponse {
  id: number;
  name: string;
  programs: {
    id: number;
    name: string;
    limit: number;
    count: number;
  }[];
  memberCount: number;
}

const { data: events } = await useFetch<EventResponse[]>('https://lerchen.net/korga/api/events')

function full(event: EventResponse) {
  if (event === null) return false;
  for (const program of event.programs) {
    if (program.count < program.limit) return false;
  }
  return true;
};

</script>
