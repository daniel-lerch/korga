<template>
  <LoadingSpinner v-if="!loaded" :state="{ error }" />
  <div v-else class="container page-loaded-container">
    <h1>E-Mail-Verteiler</h1>
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
          <FilterNode
            v-if="dl.permittedRecipients !== null"
            :filter="dl.permittedRecipients"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import {
  DistributionList,
  PersonFilter,
  getDistributionLists,
} from "@/services/distribution-list";
import LoadingSpinner from "@/components/LoadingSpinner.vue";
import FilterNode from "@/components/FilterNode.vue";

export default defineComponent({
  components: { LoadingSpinner, FilterNode },
  setup() {
    const distributionLists = ref<DistributionList[]>([]);
    const loaded = ref(false);
    const error = ref<string | null>(null);

    onMounted(async () => {
      try {
        distributionLists.value.push(...(await getDistributionLists()));
        loaded.value = true;
      } catch (e) {
        error.value =
          "Die Verteilerlisten konnten nicht geladen werden. Bitte überprüfe deine Internetverbindung.";
      }
    });

    const shortText = function (filter: PersonFilter) {
      switch (filter.discriminator) {
        case "StatusFilter":
          return "Status: " + filter.statusName;
        case "GroupFilter": {
          const prefix = "Gruppe: " + filter.groupName;
          return filter.groupRoleName
            ? prefix + " (" + filter.groupRoleName + ")"
            : prefix;
        }
        case "GroupTypeFilter": {
          const prefix = "Gruppentyp: " + filter.groupTypeName;
          return filter.groupRoleName
            ? prefix + " (" + filter.groupRoleName + ")"
            : prefix;
        }
        case "SinglePerson":
          return "Person: " + filter.personFullName;
        default:
          return "Unbekannter Filtertyp: " + filter.discriminator;
      }
    };

    return {
      distributionLists,
      loaded,
      error,
      shortText,
    };
  },
});
</script>
