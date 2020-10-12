<template>
  <div v-if="state !== 1" class="page-load-container">
    <div v-if="state === 0" class="spinner-border" role="status"></div>
    <div v-if="state === -1" class="alert alert-danger" role="alert">
      {{ errorMessage }}
    </div>
  </div>
  <div v-else class="page-loaded-container">
    Vorname: {{ person.givenName }}<br>
    Nachname: {{ person.familyName }}
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Person2, getPerson } from '../services/person'

export default defineComponent({
  props: {
    id: {
      type: String,
      required: true
    }
  },
  data () {
    return {
      person: null as Person2 | null,
      state: 0,
      errorMessage: ''
    }
  },
  mounted () {
    getPerson(parseInt(this.id)).then(
      response => {
        this.state = 1
        this.person = response
      },
      error => {
        this.state = -1
        this.errorMessage = error.toString()
      })
  }
})
</script>
