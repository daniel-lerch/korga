<template>
  <h1>Admin</h1>
  <div v-for="event in events" :key="event.id">
    <h2>{{ event.name }}</h2>
    <button
      type="button"
      class="btn btn-primary"
      @click="getEventData(event.id)"
    >
      zeige Teilnehmer
    </button>
  </div>
  <div v-for="program in event?.programs" :key="program.id">
    <h3>{{ program.name }}</h3>
    <table class="table table-striped">
      <thead>
        <tr>
          <th scope="col">Vorname</th>
          <th scope="col">Nachname</th>
          <th scope="col">Delete</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="participant in program.participants" :key="participant.id">
          <td>{{ participant.givenName }}</td>
          <td>{{ participant.familyName }}</td>
          <td>
            <button
              type="button"
              class="btn btn-outline-danger"
              @click="
                removeParticipant(
                  participant.id,
                  participant.givenName,
                  participant.familyName
                )
              "
            >
              Delete
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import {
  EventResponse,
  EventResponse2,
  getEvents,
  getEvent,
} from "@/services/event";
import { deletePerson } from "@/services/event";

export default defineComponent({
  setup() {
    const events = ref<EventResponse[]>([]);
    const event = ref<EventResponse2 | null>(null);
    const curId = ref(-1);

    onMounted(async () => {
      events.value.push(...(await getEvents()));
      // console.log(events.value)
    });

    const getEventData = async function (id: number) {
      event.value = await getEvent(id.toString());
      curId.value = id;
    };

    const removeParticipant = async function (
      id: string,
      givenName: string,
      familyName: string
    ) {
      if (!confirm(`Wollen sie wirklich ${givenName} ${familyName} abmelden?`))
        return;

      await deletePerson(id);

      getEventData(curId.value);
    };

    return {
      events,
      event,
      getEventData,
      removeParticipant,
      curId,
    };
  },
});
</script>

<style scoped>
.table > :not(:first-child) {
  border-top: 0;
}
</style>
