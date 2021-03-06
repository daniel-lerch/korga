<template>
  <nav class="navbar navbar-light bg-light px-2">
    <form @submit.prevent class="row mx-0">
      <div class="col px-2">
        <input type="search" v-model="searchQuery" class="form-control" placeholder="Search">
      </div>
      <div class="col-auto px-2">
        <router-link :to="{ name: 'Person', params: { id: 'new' }}" class="btn btn-outline-primary">
          <FontAwesomeIcon icon="user-plus"></FontAwesomeIcon>
        </router-link>
      </div>
    </form>
  </nav>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="container page-loaded-container">
    <PersonRow v-for="person in visiblePeople" :key="person.id" :person="person" />
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from 'vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { PersonResponse, getPeople } from '../services/person'
import Loading from '@/components/Loading.vue'
import PersonRow from '@/components/PersonRow.vue'

export default defineComponent({
  components: {
    FontAwesomeIcon,
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
