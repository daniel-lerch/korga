<template>
  <div class="container">
    <router-link
      :to="{ path: `/event/${id}/register` }"
      class="btn btn-primary mb-3 btnAn"
      :class="{
        disabled: !freeSeats,
      }"
      >Person anmelden</router-link
    >
    <h1>Teilnehmer: {{ event?.name }}</h1>
    <div v-for="program in event?.programs" :key="program.id">
      <h3>{{ program.name }}</h3>
      <table class="table table-striped">
        <thead>
          <tr>
            <th scope="col">Vorname</th>
            <th scope="col">Nachname</th>
            <th scope="col">Abmelden</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="participant in program.participants" :key="participant.id">
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

      const res = await deletePerson(id);
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
.btnAn {
  margin-top: 8px;
}
.table > :not(:first-child) {
  border-top: 0;
}
h3 {
  margin-top: 42px;
}
</style>
