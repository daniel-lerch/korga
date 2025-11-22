<template>
  <div class="container page-loaded-container">
    <h1>E-Mail-Verteiler</h1>
    <div class="container">
      <div v-for="dl in distributionLists" :key="dl.id" class="row border-bottom mb-2">
        <div class="col-8 col-md-3 col-lg-2">
          <h6>{{ dl.alias }}</h6>
        </div>
        <div class="col-auto">
          <span class="badge rounded-pill bg-primary" :class="{ invisible: !dl.newsletter }">
            Newsletter
          </span>
        </div>
        <div class="col-12 col-md-6">
          <ul class="list-unstyled mb-2">
            <li v-for="filter in dl.permittedRecipients" :key="filter.id">
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
