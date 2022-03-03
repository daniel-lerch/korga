<template>
  <nav class="navbar navbar-light bg-light">
    <form @submit.prevent class="form-inline">
      <input
        type="search"
        v-model="searchQuery"
        class="form-control"
        placeholder="Search"
      />
    </form>
  </nav>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="container page-loaded-container">
    <GroupRow v-for="group in visibleGroups" :key="group.id" :group="group" />
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from "vue";
import { getGroups, GroupResponse } from "../services/group";
import Loading from "@/components/Loading.vue";
import GroupRow from "@/components/GroupRow.vue";

export default defineComponent({
  components: {
    Loading,
    GroupRow,
  },
  setup() {
    const groups = ref<GroupResponse[]>([]);
    const state = ref({ loaded: false, error: null });
    const searchQuery = ref("");

    const visibleGroups = computed(() => {
      return groups.value.filter(
        (group) =>
          group.name.toUpperCase().includes(searchQuery.value.toUpperCase()) ||
          group.description
            .toUpperCase()
            .includes(searchQuery.value.toUpperCase())
      );
    });

    onMounted(() => {
      document.title = "Groups - Korga";
      getGroups().then(
        (response) => {
          state.value.loaded = true;
          groups.value.push(...response);
          document.title = "Groups - Korga";
        },
        (error) => {
          state.value.error = error;
          document.title = "Error - Korga";
        }
      );
    });

    return {
      state,
      searchQuery,
      visibleGroups,
    };
  },
});
</script>
