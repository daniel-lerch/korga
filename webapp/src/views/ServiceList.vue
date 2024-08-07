<template>
  <div class="container">
    <h1>Dienstzuteilung</h1>
    <div class="selection-bar">
      <multiselect
        class="selection-service"
        v-model="selectedServices"
        placeholder="Bitte Auswählen"
        label="name"
        track-by="id"
        :options="services"
        :multiple="true"
        :close-on-select="false"
        @select="fetchServiceHistory"
        @remove="fetchServiceHistory"
        v-if="services != null && services.length > 0"
      ></multiselect>

      <div class="selection-sort">
        <label>Sortieren nach:</label>
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
            inactive: person.groups.some(
              (group) => group.groupMemberStatus !== 'Active'
            ),
          }"
        >
          <th scope="row">{{ index }}</th>
          <td>{{ person.firstName }}</td>
          <td>{{ person.lastName }}</td>
          <td>
            {{ person.serviceDates.map((ele) => ele.date).join(", ") }}
            <span
              v-if="
                person.groups[0].groupMemberStatus !== 'Active' &&
                person.groups.every(
                  (g) =>
                    g.groupMemberStatus === person.groups[0].groupMemberStatus
                )
              "
              class="badge rounded-pill"
              :class="{
                'text-bg-danger':
                  person.groups[0].groupMemberStatus === 'To_Delete',
                'text-bg-secondary':
                  person.groups[0].groupMemberStatus === 'Requested',
              }"
              >{{ person.groups[0].groupMemberStatus }}</span
            >
            <span
              v-else
              v-for="group in person.groups.filter(
                (g) => g.groupMemberStatus !== 'Active'
              )"
              v-bind:key="group.groupName"
              class="badge rounded-pill"
              :class="{
                'text-bg-danger': group.groupMemberStatus === 'To_Delete',
                'text-bg-secondary': group.groupMemberStatus === 'Requested',
              }"
            >
              {{ group.groupName }}: {{ group.groupMemberStatus }}
            </span>
          </td>
          <td>
            {{
              person.groups
                .filter((group) => group.comment !== "")
                .map((group) => `${group.groupName}: ${group.comment}`)
                .join(", ")
            }}
          </td>
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
import type { ServiceDate, ServiceHistory, Service } from "@/services/service"
import { onMounted, ref } from "vue"
import { getServiceHistory, getServices } from "@/services/service"
import Multiselect from "vue-multiselect"

const services = ref<Service[] | null>(null)
const serviceHistory = ref<ServiceHistory[] | null>(null)
const selectedServices = ref<Service[]>([])

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
  if (!serviceHistory.value) {
    return
  }

  serviceHistory.value = serviceHistory.value.sort((a, b) => {
    // Helper function to find the most recent past date
    const getLastPastServiceDate = (serviceDates: ServiceDate[]): Date => {
      // Filter out all dates that are in the future
      const pastDates = serviceDates
        .map((dateObj) => new Date(dateObj.date))
        .filter((date) => date <= new Date())

      // If there are no past dates, return a very old date
      if (pastDates.length === 0) {
        return new Date(0)
      }

      // Return the most recent past date
      return new Date(Math.max(...pastDates.map((date) => date.getTime())))
    }

    const lastPastDateA = getLastPastServiceDate(a.serviceDates)
    const lastPastDateB = getLastPastServiceDate(b.serviceDates)

    return lastPastDateA.getTime() - lastPastDateB.getTime()
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
tr.inactive td {
  color: var(--bs-secondary);
}

tr.todelete td {
  color: var(--bs-danger);
  text-decoration: line-through;
}

.selection-bar {
  display: flex;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.selection-sort {
  display: flex;
  align-items: center;
  flex-grow: 1;
  width: 400px;
}

.selection-service {
  flex-grow: 2;
}
</style>
