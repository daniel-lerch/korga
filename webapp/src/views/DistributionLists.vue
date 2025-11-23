<template>
  <div class="px-4">
    <h1 class="text-2xl font-bold mb-4">E-Mail-Verteiler</h1>
    <div v-for="dl in distributionLists" :key="dl.id" class="py-3 border-b border-gray-200 last:border-b-0">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div class="md:w-1/4 lg:w-1/6">
          <h6 class="text-lg font-semibold">{{ dl.alias }}</h6>
        </div>
        <div class="flex items-center md:justify-center">
          <span v-if="dl.newsletter"
            class="inline-flex items-center bg-primary text-white text-sm px-3 py-1 rounded-full">
            Newsletter
          </span>
        </div>
        <div class="mt-2 md:mt-0 md:w-1/2">
          <ul class="list-none mb-0 space-y-1">
            <li v-for="filter in dl.permittedRecipients" :key="filter.id" class="text-sm text-gray-700">
              {{ shortText(filter) }}
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type {
  PersonFilter,
} from "@/services/distribution-list"
import { getDistributionLists } from "@/services/distribution-list"

const distributionLists = await getDistributionLists()

const shortText = function (filter: PersonFilter) {
  switch (filter.discriminator) {
    case "StatusFilter":
      return "Status: " + filter.statusName
    case "GroupFilter": {
      const prefix = "Gruppe: " + filter.groupName
      return filter.groupRoleName
        ? prefix + " (" + filter.groupRoleName + ")"
        : prefix
    }
    case "GroupTypeFilter": {
      const prefix = "Gruppentyp: " + filter.groupTypeName
      return filter.groupRoleName
        ? prefix + " (" + filter.groupRoleName + ")"
        : prefix
    }
    case "SinglePerson":
      return "Person: " + filter.personFullName
    default:
      return "Unbekannter Filtertyp: " + filter.discriminator
  }
}
</script>
