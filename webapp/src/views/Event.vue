<template>
  <form @submit.prevent="register" class="container">
    <h1>{{ event?.name }}</h1>
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
    <button type="submit" class="btn btn-primary">Anmelden</button>
  </form>
</template>

<script lang="ts">
import router from "@/router";
import {
  EventResponse2,
  EventResponse3,
  getEvent,
  registerForEvent,
} from "@/services/event";
import { defineComponent, onMounted, ref } from "vue";

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
    onMounted(async () => {
      event.value = await getEvent(props.id);
      if (event.value.programs.length == 1) {
        programId.value = event.value.programs[0].id;
      }
    });

    const register = async function () {
      if (givenName.value == "") return;
      if (familyName.value == "") return;
      console.log("register");
      const request: EventResponse3 = {
        programId: programId.value,
        givenName: givenName.value,
        familyName: familyName.value,
        conflict: false,
      };
      try {
        const res = await registerForEvent(request);
        // console.log(res);
      } catch (err) {
        console.log(err);
      } finally {
        event.value = await getEvent(props.id);
        router.push({ name: "List", params: { id: event.value.id } });
      }
    };

    return {
      event,
      givenName,
      familyName,
      programId,
      register,
    };
  },
});
</script>

<style>
form {
  max-width: 400px;
  margin: 5px;
}
</style>
