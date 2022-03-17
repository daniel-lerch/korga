<template>
  <div class="container">
    <h1>Anmeldung</h1>
    <div class="card shadow">
      <div class="card-body">
        <form @submit.prevent="register" class="mw-100">
          <h2>{{ event?.name }}</h2>
          <div class="mb-3">
            <label for="givenName" class="form-label">Vorname</label>
            <input
              type="text"
              v-model="givenName"
              id="givenName"
              class="form-control"
              placeholder="Max"
              required
            />
          </div>
          <div class="mb-3">
            <label for="givenName" class="form-label">Nachname</label>
            <input
              type="text"
              v-model="familyName"
              id="familyName"
              class="form-control"
              placeholder="Mustermann"
              required
            />
          </div>
          <div>
            <div
              class="form-check"
              v-for="program in event?.programs"
              :key="program.id"
            >
              <input
                class="form-check-input"
                type="radio"
                name="exampleRadios"
                :id="`radio${program.id}`"
                :value="program.id"
                v-model="programId"
                :disabled="program.participants.length >= program.limit"
                required
              />
              <label class="form-check-label" :for="`radio${program.id}`">
                {{ program.name }} {{ program.participants.length }}/{{
                  program.limit
                }}
              </label>
            </div>
          </div>
          <div
            class="alert alert-warning alert-dismissible"
            role="alert"
            v-if="checkAlreadyRegisterd"
            style="margin-top: 15px"
          >
            Diese Person ist bereits schon zu diesem Event angemeldet
          </div>
          <button
            type="submit"
            class="btn btn-secondary mt-3 w-100"
            :disabled="full"
          >
            {{ checkAlreadyRegisterd ? "Trotzdem Anmelden" : "Anmelden" }}
          </button>
          <div class="alert alert-danger" role="alert" v-if="full">
            Alle Plätze sind bereits belegt.
          </div>
          <div
            class="alert alert-danger alert-dismissible"
            role="alert"
            v-else-if="error"
          >
            {{ error }}
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import router from "@/router";
import {
  EventResponse2,
  EventRegistrationRequest,
  getEvent,
  registerForEvent,
} from "@/services/event";
import { defineComponent, onMounted, ref, computed } from "vue";

export default defineComponent({
  props: {
    id: {
      type: String,
      required: true,
    },
  },
  setup(props) {
    const event = ref<EventResponse2 | null>(null);
    const givenName = ref("");
    const familyName = ref("");
    const programId = ref(0);
    const error = ref("");

    // Use computed value to avoid duplicate data which might be outdated.
    const full = computed(() => {
      if (event.value === null) return false;
      for (const program of event.value.programs) {
        if (program.participants.length < program.limit) return false;
      }
      return true;
    });

    onMounted(async () => {
      event.value = await getEvent(props.id);
      if (
        event.value.programs.length == 1 &&
        event.value.programs[0].participants.length <
          event.value.programs[0].limit
      ) {
        programId.value = event.value.programs[0].id;
      }
    });

    const getEventData = async function () {
      event.value = await getEvent(props.id);
    };

    const checkAlreadyRegisterd = computed(() => {
      if (event.value === null) return;
      let alreadyRegisterd = false;
      event.value.programs.forEach((ele) => {
        ele.participants.forEach((element) => {
          if (
            element.givenName.toLowerCase() == givenName.value.toLowerCase() &&
            element.familyName.toLowerCase() == familyName.value.toLowerCase()
          ) {
            alreadyRegisterd = true;
          }
        });
      });
      return alreadyRegisterd;
    });

    const register = async function () {
      if (givenName.value == "") return;
      if (familyName.value == "") return;
      const request: EventRegistrationRequest = {
        programId: programId.value,
        givenName: givenName.value,
        familyName: familyName.value,
      };

      const res = await registerForEvent(request);
      if (res) {
        error.value = "";
        event.value = await getEvent(props.id);
        router.push({ name: "Event", params: { id: event.value.id } });
      } else {
        error.value = "Es ist ein Fehler aufgetreten";
        getEventData();
      }
    };

    return {
      event,
      givenName,
      familyName,
      programId,
      error,
      full,
      register,
      checkAlreadyRegisterd,
    };
  },
});
</script>

<style scoped>
input {
  max-width: 400px;
}
.btn {
  margin-top: 12px;
  margin-bottom: 12px;
}

h1 {
  margin-top: 12px;
}
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
