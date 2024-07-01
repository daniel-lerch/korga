<template>
  <div class="container">
    <h1>Dienstzuteilung</h1>

    <select
      class="form-select"
      aria-label="Default select example"
      @change="fetchServiceHistory(($event.target as HTMLSelectElement).value)"
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
        <tr
          v-for="(person, index) in serviceHistory"
          :key="index"
          :class="{
            requested: person.groupMemberStatus === 'Requested',
            todelete: person.groupMemberStatus === 'ToDelete',
          }"
        >
          <th scope="row">{{ index }}</th>
          <td>{{ person.firstName }}</td>
          <td>{{ person.lastName }}</td>
          <td>{{ person.serviceDates.join(", ") }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { ServiceHistory, Services } from "@/services/service"
import { onMounted, ref } from "vue"
import { getServiceHistory, getServices } from "@/services/service"

const services = ref<Services[] | null>(null)
const serviceHistory = ref<ServiceHistory[] | null>(null)

onMounted(async () => {
  services.value = await getServices()
})

const fetchServiceHistory = async function (id: string) {
  serviceHistory.value = await getServiceHistory(parseInt(id))
}
</script>

<style scoped>
tr.requested td {
  color: var(--bs-secondary);
}

tr.todelete td {
  color: var(--bs-danger);
  text-decoration: line-through;
}
</style>
