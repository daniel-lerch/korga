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
    <PersonRow v-for="person in visiblePeople" :key="person.id" :person="person" />
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from 'vue'
import { PersonResponse, getPeople } from '../services/person'
import PersonRow from '@/components/PersonRow.vue'

export default defineComponent({
  components: {
    PersonRow
  },
  setup () {
    const people = ref<PersonResponse[]>([])
    const state = ref(0)
    const errorMessage = ref('')
    const searchQuery = ref('')

    const visiblePeople = computed(() => {
      return people.value.filter(
        person => person.givenName.toUpperCase().includes(searchQuery.value.toUpperCase()) ||
                  person.familyName.toUpperCase().includes(searchQuery.value.toUpperCase()))
    })

    onMounted(() => {
      document.title = 'People - Korga'
      getPeople().then(
        response => {
          state.value = 1
          people.value.push(...response)
          document.title = 'People - Korga'
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
      visiblePeople
    }
  }
})
</script>
