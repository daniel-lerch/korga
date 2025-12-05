<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-3xl">E-Mail-Verteiler</h1>
      <Button asChild v-slot="slotProps" type="button" severity="primary">
        <RouterLink :to="`/new`" :class="slotProps.class">
          <i class="pi pi-plus"></i>
          Neuer Verteiler
        </RouterLink>
      </Button>
    </div>
    <DataTable :value="distributionLists">
      <Column field="alias" header="Alias" />
      <Column field="recipientCount" header="Anzahl Empfänger" />
      <Column field="recipientsQuery" header="Empfänger">
        <template #body="slotProps">
          <PersonFilterCell :filter="slotProps.data.recipientsQuery" />
        </template>
      </Column>
      <Column>
        <template #body="{ data }">
          <Button asChild v-slot="slotProps" type="button" severity="secondary" variant="text">
            <RouterLink :to="`/${data.id}`" :class="slotProps.class" class="p-button-icon-only">
              <i class="pi pi-pencil"></i>
            </RouterLink>
          </Button>
        </template>
      </Column>
      <Column>
        <template #body="{ data }">
          <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="remove(data.id)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import DataTable from "primevue/datatable"
import Column from "primevue/column"
import Button from "primevue/button"
import { ref } from "vue"
import { getDistributionLists, deleteDistributionList } from "@/services/distribution-list"
import PersonFilterCell from "@/components/PersonFilterCell.vue"
import { useExtensionStore } from "@/stores/extension"

const extension = useExtensionStore()
if (extension.moduleId === 0) {
  await extension.load()
}
if (extension.accessToken === "") {
  await extension.login()
}

const distributionLists = ref(await getDistributionLists())

async function remove(id: number) {
  // TODO: Use PrimeVue ConfirmDialog
  try {
    await deleteDistributionList(id)
    distributionLists.value = distributionLists.value.filter((dl) => dl.id !== id)
  } catch (e) {
    console.log(e)
  }
}
</script>
