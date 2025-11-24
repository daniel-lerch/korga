<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-2xl font-bold">E-Mail-Verteiler</h1>
      <router-link to="/create" class="inline-block">
        <button type="button"
          class="ml-3 inline-flex items-center px-3 py-2 bg-primary text-white rounded hover:bg-blue-700">
          Neuer Verteiler
        </button>
      </router-link>
    </div>
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
          <span class="ml-3 inline-flex items-center text-sm text-gray-700" :title="dl.recipientCountTime ?? ''">
            <strong class="mr-1">{{ dl.recipientCount ?? '-' }}</strong>
            <span class="text-xs text-gray-500">Empfänger</span>
          </span>
        </div>
        <div class="mt-2 md:mt-0 md:w-1/2">
          {{ recipientsLabel[dl.id] ?? JSON.stringify(dl.recipientsQuery) }}
          <span class="text-sm text-gray-500 ml-2">
            ({{ recipientsIsParsed[dl.id] ? 'Parsed' : 'Custom query' }})
          </span>
        </div>
        <div class="flex items-center md:justify-end">
          <button type="button" @click="remove(dl.id)"
            class="ml-3 inline-flex items-center px-3 py-2 bg-red-600 text-white rounded hover:bg-red-700">
            Löschen
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { getDistributionLists, deleteDistributionList } from "@/services/distribution-list"
import type { DistributionList } from "@/services/distribution-list"
import { getMailistFilters, type PersonFilter } from "@/services/churchquery"

const distributionLists = ref<DistributionList[]>([])
const recipientsLabel = ref<Record<number, string>>({})
const recipientsIsParsed = ref<Record<number, boolean>>({})

function formatFilters(filters: PersonFilter[] | null): string {
  if (filters === null) return "Custom query"
  const parts: string[] = []
  for (const f of filters) {
    if (f === null) {
      parts.push("Unknown")
      continue
    }
    if (f.kind === "person") {
      parts.push(`Person: ${f.personId}`)
    } else if (f.kind === "group") {
      const rolePart = f.roleIds && f.roleIds.length > 0 ? ` roles: ${f.roleIds.join(",")}` : ""
      parts.push(`Group: ${f.groupId}${rolePart}`)
    } else {
      parts.push("Unknown")
    }
  }
  return parts.join(", ")
}

async function load() {
  distributionLists.value = await getDistributionLists()

  for (const dl of distributionLists.value) {
    try {
      const parsed = getMailistFilters(dl.recipientsQuery)
      recipientsIsParsed.value[dl.id] = parsed !== null
      recipientsLabel.value[dl.id] = parsed !== null ? formatFilters(parsed) : (dl.recipientsQuery ? JSON.stringify(dl.recipientsQuery) : "Custom query")
    } catch (err) {
      console.log(err)
      recipientsIsParsed.value[dl.id] = false
      recipientsLabel.value[dl.id] = dl.recipientsQuery ? JSON.stringify(dl.recipientsQuery) : "Custom query"
    }
  }
}

await load()

async function remove(id: number) {
  try {
    await deleteDistributionList(id)
    distributionLists.value = distributionLists.value.filter((dl) => dl.id !== id)
    delete recipientsLabel.value[id]
    delete recipientsIsParsed.value[id]
  } catch (e) {
    console.log(e)
  }
}
</script>
