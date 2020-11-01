<template>
  <nav class="navbar navbar-light bg-light">
    <form class="form-inline">
      <input type="search" v-model="searchQuery" class="form-control" placeholder="Search">
    </form>
  </nav>
  <div v-if="state !== 1" class="page-load-container">
    <div v-if="state === 0" class="spinner-border" role="status"></div>
    <div v-if="state === -1" class="alert alert-danger" role="alert">
      {{ errorMessage }}
    </div>
  </div>
  <div v-else class="container page-loaded-container">
    <GroupRow v-for="group in visibleGroups" :key="group.id" :group="group" />
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from 'vue'
import { getGroups, GroupResponse } from '../services/group'
import GroupRow from '@/components/GroupRow.vue'

export default defineComponent({
  components: {
    GroupRow
  },
  setup () {
    const groups = ref<GroupResponse[]>([])
    const state = ref(0)
    const errorMessage = ref('')
    const searchQuery = ref('')

    const visibleGroups = computed(() => {
      return groups.value.filter(
        group => group.name.toUpperCase().includes(searchQuery.value.toUpperCase()) ||
                 group.description.toUpperCase().includes(searchQuery.value.toUpperCase()))
    })

    onMounted(() => {
      document.title = 'Groups - Korga'
      getGroups().then(
        response => {
          state.value = 1
          groups.value.push(...response)
          document.title = 'Groups - Korga'
        },
        error => {
          state.value = -1
          errorMessage.value = error.toString()
          document.title = 'Error - Korga'
        })
    })

    return {
      state,
      errorMessage,
      searchQuery,
      visibleGroups
    }
  }
})
</script>
