<template>
  <div class="container">
    <h1>Service List</h1>

    <select
      class="form-select"
      aria-label="Default select example"
      @change="fetchServiceHistory($event.target.value)"
    >
      <option disabled selected>Bitte auswählen</option>
      <option
        v-for="(service, index) in services"
        :key="index"
        :value="service.id"
      >
        {{ service.name }}
      </option>
    </select>

    <table class="table table-striped" v-if="serviceHistory">
      <thead>
        <tr>
          <th scope="col">#</th>
          <th scope="col">Vorname</th>
          <th scope="col">Nachname</th>
          <th scope="col">Datum</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(person, index) in serviceHistory" :key="index">
          <th scope="row">{{ index }}</th>
          <td>{{ person.firstName }}</td>
          <td>{{ person.lastName }}</td>
          <td>{{ person.serviceDates.join(", ") }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import {
  getServiceHistory,
  getServices,
  ServiceHistory,
  Services,
} from "@/services/service";

interface Props {
  id: string;
}

export default defineComponent({
  props: {
    id: {
      type: String,
      required: true,
    },
  },
  setup(props: Props) {
    const services = ref<Services[] | null>(null);
    const serviceHistory = ref<ServiceHistory[] | null>(null);

    onMounted(async () => {
      services.value = await getServices();
    });

    const fetchServiceHistory = async function (id: number) {
      console.log(id);
      serviceHistory.value = await getServiceHistory(id);
    };

    return {
      services,
      props,
      fetchServiceHistory,
      serviceHistory,
    };
  },
});
</script>

<style scoped></style>
