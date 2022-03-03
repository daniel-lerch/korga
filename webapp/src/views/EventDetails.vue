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
    const freeSeats = ref(true);

    onMounted(async () => {
      getEventData();
    });

    const removeParticipant = async function (
      id: string,
      givenName: string,
      familyName: string
    ) {
      if (!confirm(`Wollen sie wirklich ${givenName} ${familyName} abmelden?`))
        return;

      try {
        const res = await deletePerson(id);
        console.log(res);
        if (res == false) {
          console.log("o");
        }
      } catch (err) {
        console.log("err");
      }

      event.value = await getEvent(props.id);
    };

    const getEventData = async function () {
      event.value = await getEvent(props.id);
      if (
        event.value?.programs.length == 1 &&
        event.value?.programs[0].participants.length >=
          event.value?.programs[0].limit
      ) {
        freeSeats.value = false;
      } else {
        freeSeats.value = true;
      }
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
