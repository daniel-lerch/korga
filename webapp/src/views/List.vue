<template>
  <h1>Teilnehmer: {{ event?.name }}</h1>
  <div v-for="program in event?.programs" :key="program.id">
    <h3>{{ program.name }}</h3>
    <table class="table">
      <thead>
        <tr>
          <th scope="col">#</th>
          <th scope="col">Vorname</th>
          <th scope="col">Nachname</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="participant in program.participants" :key="participant.id">
          <th scope="row">1</th>
          <td>{{ participant.givenName }}</td>
          <td>{{ participant.familyName }}</td>
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
  props: {
    id: {
      type: String,
      required: true,
    },
  },
  setup(props) {
    const event = ref<EventResponse2 | null>(null);
    const curId = ref(-1);

    onMounted(async () => {
      event.value = await getEvent(props.id);
    });

    const getEventData = async function (id: number) {
      event.value = await getEvent(id.toString());
      curId.value = id;
    };

    return {
      event,
      getEventData,
      curId,
      props,
    };
  },
});
</script>

<style></style>
