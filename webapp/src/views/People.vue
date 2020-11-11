<template>
  <nav class="navbar navbar-light bg-light">
    <form @submit.prevent class="form-inline my-2 my-lg-0">
      <input type="search" v-model="searchQuery" class="form-control mr-sm-2" placeholder="Search">
      <router-link :to="{ name: 'Person', params: { id: 'new' }}" class="btn btn-outline-primary my-2 my-sm-0">Create a person</router-link>
    </form>
  </nav>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="container page-loaded-container">
    <PersonRow v-for="person in visiblePeople" :key="person.id" :person="person" />
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from 'vue'
import { PersonResponse, getPeople } from '../services/person'
import Loading from '@/components/Loading.vue'
import PersonRow from '@/components/PersonRow.vue'

export default defineComponent({
  components: {
    Loading,
    PersonRow
  },
  setup () {
    const people = ref<PersonResponse[]>([])
    const state = ref({ loaded: false, error: null })
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
          state.value.loaded = true
          people.value.push(...response)
          document.title = 'People - Korga'
        },
        error => {
          state.value.error = error
          document.title = 'Error - Korga'
        })
    })

    return {
      state,
      searchQuery,
      visiblePeople
    }
  }
})
</script>
