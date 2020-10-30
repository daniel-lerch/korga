<template>
  <div v-if="state !== 1" class="page-load-container">
    <div v-if="state === 0" class="spinner-border" role="status"></div>
    <div v-if="state === -1" class="alert alert-danger" role="alert">
      {{ errorMessage }}
    </div>
  </div>
  <div v-else class="container page-loaded-container">
    <PersonRow v-for="person in people" :key="person.id" :person="person" />
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { PersonResponse, getPeople } from '../services/person'
import PersonRow from '@/components/PersonRow.vue'

export default defineComponent({
  components: {
    PersonRow
  },
  data () {
    return {
      people: new Array<PersonResponse>(),
      state: 0,
      errorMessage: ''
    }
  },
  mounted () {
    document.title = 'People - Korga'
    getPeople().then(
      response => {
        this.state = 1
        this.people.push(...response)
        document.title = 'People - Korga'
      },
      error => {
        this.state = -1
        this.errorMessage = error.toString()
        document.title = 'Error - Korga'
      })
  }
})
</script>
