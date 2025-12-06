<template>
  <CheckboxGroup v-model="roleIds" class="my-2 flex-wrap gap-x-8 gap-y-2">
    <div v-for="value in props.roles" :key="value.id" class="flex items-center gap-2">
      <Checkbox :inputId="`cb-role-${value.id}`" :value="value.groupTypeRoleId" />
      <label :for="`cb-role-${value.id}`">{{ value.name }}</label>
    </div>
  </CheckboxGroup>
</template>

<script setup lang="ts">
import Checkbox from 'primevue/checkbox';
import CheckboxGroup from 'primevue/checkboxgroup';
import { ref, watch } from 'vue';

const props = defineProps<{
  modelValue: number[],
  roles: { id: number, name: string, groupTypeRoleId: number }[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: number[]): void
}>()

function getInflatedRoleIds(roleIds: number[], roles: { groupTypeRoleId: number }[]) {
  // Remove invalid role IDs
  roleIds = roleIds.filter(rid => roles.some(r => r.groupTypeRoleId === rid))
  if (roleIds.length === 0) {
    return roles.map(r => r.groupTypeRoleId)
  } else {
    return roleIds
  }
}

const roleIds = ref(getInflatedRoleIds(props.modelValue, props.roles))
//const roleIds = ref<unknown[]>([])

watch(props, (newValue) => {
  const inflatedRoleIds = getInflatedRoleIds(newValue.modelValue, newValue.roles)
  // Only assign different values to avoid recursive updates
  if (JSON.stringify(roleIds.value) !== JSON.stringify(inflatedRoleIds)) {
    roleIds.value = inflatedRoleIds
  }
})

watch(roleIds, (newValue) => {
  if (newValue.length === props.roles.length) {
    // If all roles are selected, emit an empty array to indicate "all roles"
    emit('update:modelValue', [])
  } else {
    emit('update:modelValue', newValue)
  }
})
</script>