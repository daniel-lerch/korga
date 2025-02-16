<template>
  <LoadingSpinner v-if="!loaded" :state="{ error }" />
  <div v-else class="container page-loaded-container">
    <h1>Verteilerlisten</h1>
    <div class="container">
      <div
        v-for="dl in distributionLists"
        :key="dl.id"
        class="row border-bottom mb-2"
      >
        <div class="col-8 col-md-3 col-lg-2">
          <h6>{{ dl.alias }}</h6>
        </div>
        <div class="col-auto">
          <span
            class="badge rounded-pill bg-primary"
            :class="{ invisible: !dl.newsletter }"
          >
            Newsletter
          </span>
        </div>
        <div class="col-12 col-md-6">
          <PersonFilter :filter-list="dl.permittedRecipients" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import type { DistributionList } from "@/services/distribution-list"
import { defineComponent, onMounted, ref } from "vue"
import { getDistributionLists } from "@/services/distribution-list"
import LoadingSpinner from "@/components/LoadingSpinner.vue"
import PersonFilter from "@/components/PersonFilter.vue"

export default defineComponent({
  components: { LoadingSpinner, PersonFilter },
  setup() {
    const distributionLists = ref<DistributionList[]>([])
    const loaded = ref(false)
    const error = ref<string | null>(null)

    onMounted(async () => {
      try {
        distributionLists.value.push(...(await getDistributionLists()))
        loaded.value = true
      } catch (e) {
        error.value =
          "Die Verteilerlisten konnten nicht geladen werden. Bitte überprüfe deine Internetverbindung."
      }
    })

    return {
      distributionLists,
      loaded,
      error,
    }
  },
})
</script>
