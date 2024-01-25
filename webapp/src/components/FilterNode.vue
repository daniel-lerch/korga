<template>
  <ul v-if="filter.discriminator === 'LogicalOr'" class="or mb-2">
    <li v-for="child in filter.children" :key="child.id">
      <FilterNode :filter="child" />
    </li>
  </ul>
  <ul v-else-if="filter.discriminator === 'LogicalAnd'" class="and mb-2">
    <li v-for="child in filter.children" :key="child.id">
      <FilterNode :filter="child" />
    </li>
  </ul>
  <div v-else-if="filter.discriminator === 'StatusFilter'" class="mb-2">
    Status: {{ filter.statusName }}
  </div>
  <div v-else-if="filter.discriminator === 'GroupFilter'" class="mb-2">
    Gruppe: {{ filter.groupName }}
    <span v-if="filter.groupRoleName">({{ filter.groupRoleName }})</span>
  </div>
  <div v-else-if="filter.discriminator === 'GroupTypeFilter'" class="mb-2">
    Gruppentyp: {{ filter.groupTypeName }}
    <span v-if="filter.groupRoleName">({{ filter.groupRoleName }})</span>
  </div>
  <div v-else-if="filter.discriminator === 'SinglePerson'" class="mb-2">
    Person: {{ filter.personFullName }}
  </div>
</template>

<script lang="ts">
import { PersonFilter } from "@/services/distribution-list";
import { PropType, defineComponent } from "vue";

export default defineComponent({
  props: {
    filter: {
      type: Object as PropType<PersonFilter>,
      required: true,
    },
  },
});
</script>

<style scoped>
ul.or {
  list-style-type: "∪ ";
  padding-left: 1rem;
}
ul.and {
  list-style-type: "∩ ";
  padding-left: 1rem;
}
</style>
