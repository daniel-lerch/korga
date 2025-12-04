<template>
  <div class="flex flex-col gap-2">
    <div v-if="filters.length === 0" class="py-2 italic mx-auto">
      Füge Filterkriterien hinzu um Empfänger auszuwählen.
    </div>
    <div v-for="(filter, index) in filters" :key="index">
      <div v-if="filter.kind === 'person'">
        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-user"></i>
          </InputGroupAddon>
          <Select v-model="filter.personId" :options="persons" filter optionLabel="name" optionValue="id"
            placeholder="Person auswählen" />
          <InputGroupAddon>
            <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="removeFilter(index)" />
          </InputGroupAddon>
        </InputGroup>
      </div>
      <div v-else-if="filter.kind === 'group'">
        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-users"></i>
          </InputGroupAddon>
          <Select v-model="filter.groupId" :options="groups" filter optionLabel="name" optionValue="id"
            placeholder="Gruppe auswählen" />
          <InputGroupAddon>
            <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="removeFilter(index)" />
          </InputGroupAddon>
        </InputGroup>
        <RoleCheckboxGroup v-model="filter.roleIds" :roles="groups.find(g => g.id === filter.groupId)?.roles || []" />
      </div>
    </div>
    <ButtonGroup>
      <Button type="button" label="Gruppe" icon="pi pi-plus" @click="addGroupFilter" />
      <Button type="button" label="Person" icon="pi pi-plus" @click="addSinglePersonFilter" />
    </ButtonGroup>
  </div>
</template>

<script setup lang="ts">
import { getChurchQueryFilter, getMailistFilters, type GroupFilter, type SinglePersonFilter } from "@/services/churchquery";
import type { Group, GroupRole, Person } from "@/utils/ct-types";
import { churchtoolsClient } from "@churchtools/churchtools-client";
import Button from "primevue/button";
import ButtonGroup from "primevue/buttongroup";
import InputGroup from "primevue/inputgroup";
import InputGroupAddon from "primevue/inputgroupaddon";
import Select from "primevue/select";
import { onMounted, ref, watch } from "vue";
import RoleCheckboxGroup from "./RoleCheckboxGroup.vue";

const props = defineProps({
  modelValue: {
    required: true,
  }
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: unknown): void
}>()

const parsedFilters = getMailistFilters(props.modelValue)
const filters = ref<(SinglePersonFilter | GroupFilter)[]>(parsedFilters || [])

const persons = ref<{ id: number, name: string }[]>([])
const groups = ref<{ id: number, name: string, roles?: GroupRole[] }[]>([])

onMounted(() => {
  churchtoolsClient.getAllPages<Person>("/persons").then((data) => persons.value = data.map(p => {
    if (p.nickname)
      return { id: p.id, name: `${p.firstName} (${p.nickname}) ${p.lastName}` }
    else
      return { id: p.id, name: `${p.firstName} ${p.lastName}` }
  }))
  churchtoolsClient.getAllPages<Group>("/groups", { include: ["roles"] }).then((data) => groups.value = data.map(g => ({ id: g.id, name: g.name, roles: g.roles })))
})

watch(filters, (newValue) => {
  const filter = getChurchQueryFilter(newValue)
  emit('update:modelValue', filter)
}, { deep: true })

function addSinglePersonFilter() {
  filters.value.push({ kind: "person", personId: 0 })
}

function addGroupFilter() {
  filters.value.push({ kind: "group", groupId: 0, roleIds: [] })
}

function removeFilter(index: number) {
  filters.value.splice(index, 1)
}
</script>