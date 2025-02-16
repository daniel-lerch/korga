<template>
  <LoadingSpinner v-if="!loaded" :state="{ error }" />
  <div v-else class="container page-loaded-container">
    <h1>Berechtigungen</h1>
    <div v-for="permission in permissions" :key="permission.key" class="row border-bottom mb-2">
      <div class="col-8 col-md-3 col-lg-2">
        <h6>{{ permission.key }}</h6>
      </div>
      <div class="col-12 col-md-6">
        <PersonFilter :filter-list="permission.personFilters" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { getPermissions, type Permission } from "@/services/permission"
import { onMounted, ref } from "vue"
import LoadingSpinner from "@/components/LoadingSpinner.vue"
import PersonFilter from "@/components/PersonFilter.vue"

const permissions = ref<Permission[]>([])
const loaded = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    permissions.value.push(...(await getPermissions()))
    loaded.value = true
  } catch (e) {
    error.value =
      "Die Berechtigungen konnten nicht geladen werden. Bitte überprüfe deine Internetverbindung."
  }
})
</script>
