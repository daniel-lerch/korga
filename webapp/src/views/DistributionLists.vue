<template>
  <Loading v-if="!loaded" :state="{ error }" />
  <div v-else class="container page-loaded-container">
    <h1>E-Mail-Verteiler</h1>
    <div class="container">
      <div v-for="dl in distributionLists" :key="dl.id" class="row">
        <div class="col-sm-4 col-md-3">
          <h6>{{ dl.alias }}</h6>
          <span v-if="dl.newsletter" class="badge rounded-pill bg-primary">
            Newsletter
          </span>
        </div>
        <div class="col">
          <ul class="list-unstyled">
            <li v-for="filter in dl.filters" :key="filter.id">
              {{ shortText(filter) }}
            </li>
          </ul>
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
import Loading from "@/components/Loading.vue";

export default defineComponent({
  components: { Loading },
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
