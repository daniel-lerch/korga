<template>
  <div class="container">
    <div class="bg-white my-4">
      <router-link
        :to="{ path: `/event/${id}/register` }"
        class="btn btn-success btn-emphasize w-100 shadow"
        :class="{
          disabled: !freeSeats,
        }"
        >Person anmelden</router-link
      >
    </div>
    <h1>Teilnehmer: {{ event?.name }}</h1>
    <div
      v-for="program in event?.programs"
      :key="program.id"
      class="card my-4 shadow"
    >
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
            <tr
              v-for="participant in program.participants"
              :key="participant.id"
            >
              <td>{{ participant.givenName }}</td>
              <td>{{ participant.familyName }}</td>
              <th scope="row">
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

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from "vue";
import { deletePerson, EventResponse2, getEvent } from "@/services/event";

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

    // Use computed value to avoid duplicate data which might be outdated.
    const freeSeats = computed(() => {
      if (event.value === null) return true;
      for (const program of event.value.programs) {
        if (program.participants.length < program.limit) return true;
      }
      return false;
    });

    onMounted(async () => {
      event.value = await getEvent(props.id);
    });

    const removeParticipant = async function (
      id: string,
      givenName: string,
      familyName: string
    ) {
      if (!confirm(`Wollen sie wirklich ${givenName} ${familyName} abmelden?`))
        return;

      await deletePerson(id);
      event.value = await getEvent(props.id);
    };

    return {
      event,
      curId,
      props,
      freeSeats,
      removeParticipant,
    };
  },
});
</script>

<style scoped>
.table > :not(:first-child) {
  border-top: 0;
}
.container {
  max-width: 800px;
}
table {
  border-collapse: separate;
  border-spacing: 0 0.3em;
}
</style>
