<template>
  <div class="container">
    <div class="bg-white my-4">
      <router-link :to="{ name: 'event-id-register', params: { id: event?.id } }"
        class="btn btn-success btn-emphasize w-100 shadow" :class="{
          disabled: !freeSeats,
        }">Person anmelden</router-link>
    </div>
    <h1>Teilnehmer: {{ event?.name }}</h1>
    <div v-for="program in event?.programs" :key="program.id" class="card my-4 shadow">
      <div class="card-body">
        <h3 class="card-title">{{ program.name }}</h3>
        <table class="w-100">
          <thead>
            <tr>
              <th class="w-50" scope="col">Vorname</th>
              <th class="w-50" scope="col">Nachname</th>
              <th scope="col">Abmelden</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="participant in program.participants" :key="participant.id">
              <td>{{ participant.givenName }}</td>
              <td>{{ participant.familyName }}</td>
              <th scope="row">
                <button type="button" class="btn btn-outline-danger" @click="
                  removeParticipant(
                    participant.id,
                    participant.givenName,
                    participant.familyName
                  )
                  ">
                  Abmelden
                </button>
              </th>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

interface EventResponse2 {
  id: number;
  name: string;
  programs: {
    id: number;
    name: string;
    limit: number;
    participants: {
      id: string;
      givenName: string;
      familyName: string;
    }[];
  }[];
}

const route = useRoute()

const { data: event, execute } = await useFetch<EventResponse2>('https://lerchen.net/korga/api/event/' + route.params.id)

const freeSeats = computed(() => {
  if (event.value === null) return true;
  for (const program of event.value.programs) {
    if (program.participants.length < program.limit) return true;
  }
  return false;
});

async function removeParticipant(id: string, givenName: string, familyName: string) {
  if (!confirm(`Wollen Sie wirklich ${givenName} ${familyName} abmelden?`))
    return;

  await $fetch("https://lerchen.net/korga/api/events/participant/" + id, { method: "DELETE" });

  await execute();
};

</script>
