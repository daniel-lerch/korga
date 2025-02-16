<template>
  <ul class="list-unstyled mb-2">
    <li v-for="filter in filterList" :key="filter.id">
      {{ shortText(filter) }}
    </li>
  </ul>
</template>

<script setup lang="ts">
import type { PersonFilter } from "@/services/filter"
import type { PropType } from "vue";

defineProps({
  filterList: {
    type: Object as PropType<PersonFilter[]>,
    required: true,
  },
})

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
