<template>
  <div class="container">
    <h1>Dienstzuteilung</h1>
    <multiselect
      v-model="selectedServices"
      placeholder="Bitte Auswählen"
      label="name"
      track-by="id"
      :options="services"
      :multiple="true"
      @select="fetchServiceHistory"
      @remove="fetchServiceHistory"
      v-if="services?.length > 0"
    ></multiselect>

    <div>
      <label class="typo__label">Sortieren nach:</label>
      <multiselect
        v-model="selectedOption"
        :options="sortOptions"
        label="text"
        track-by="id"
        :searchable="false"
        :show-labels="false"
        :allow-empty="false"
        @select="sortServiceHistory"
        placeholder="Pick a value"
      ></multiselect>
    </div>

    <table class="table table-striped" v-if="serviceHistory">
      <thead>
        <tr>
          <th scope="col">#</th>
          <th scope="col">Vorname</th>
          <th scope="col">Nachname</th>
          <th scope="col">Datum</th>
          <th scope="col">Kommentar</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="(person, index) in serviceHistory"
          :key="index"
          :class="{
            requested: person.groups[0].groupMemberStatus === 'Requested',
            todelete: person.groups[0].groupMemberStatus === 'ToDelete',
          }"
        >
          <th scope="row">{{ index }}</th>
          <td>{{ person.firstName }}</td>
          <td>{{ person.lastName }}</td>
          <td>
            {{ person.serviceDates.map((ele) => ele.date).join(", ") }}
          </td>
          <td>{{ person.groups[0].comment }}</td>
        </tr>
      </tbody>
    </table>
    <div
      class="d-flex justify-content-center"
      v-if="!serviceHistory && selectedServices.length > 0"
    >
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ServiceHistory, Services } from "@/services/service"
import { onMounted, ref } from "vue"
import { getServiceHistory, getServices } from "@/services/service"
import Multiselect from "vue-multiselect"

const services = ref<Services[] | null>(null)
const serviceHistory = ref<ServiceHistory[] | null>(null)
const selectedServices = ref<Services[]>([])

const sortOptions = [
  { id: 0, text: "am längsten ohne Dienst" },
  { id: 1, text: "am wenigsten Dienste" },
]

const selectedOption = ref(sortOptions[0])

onMounted(async () => {
  services.value = await getServices()
})

const fetchServiceHistory = async function () {
  const ids = selectedServices.value.map((service) => service.id)
  if (ids.length === 0) {
    serviceHistory.value = null
    return
  }
  serviceHistory.value = await getServiceHistory(ids)
  sortServiceHistory()
}

const sortServiceHistory = function () {
  serviceHistory.value = serviceHistory.value?.sort((a, b) => {
    // Helper function to find the most recent past date
    const getLastPastServiceDate = (serviceDates) => {
      const pastDates = serviceDates
        .map((dateObj) => new Date(dateObj.date))
        .filter((date) => date <= new Date())

      if (pastDates.length === 0) {
        return new Date(0) // Return a very old date if there are no past dates
      }

      return new Date(Math.max(...pastDates))
    }

    const lastPastDateA = getLastPastServiceDate(a.serviceDates)
    const lastPastDateB = getLastPastServiceDate(b.serviceDates)

    return lastPastDateA - lastPastDateB
  })
  if (selectedOption.value.id === 1) {
    serviceHistory.value = serviceHistory.value?.sort((a, b) => {
      return a.serviceDates.length - b.serviceDates.length
    })
  }
}
</script>

<style src="node_modules/vue-multiselect/dist/vue-multiselect.css"></style>

<style scoped>
tr.requested td {
  color: var(--bs-secondary);
}

tr.todelete td {
  color: var(--bs-danger);
  text-decoration: line-through;
}
</style>
