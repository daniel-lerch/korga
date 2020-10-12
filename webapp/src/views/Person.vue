<template>
  <p>{{ person?.givenName }}</p>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Person2, getPerson } from '../services/person'

export default defineComponent({
  props: {
    id: {
      type: Number,
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
    getPerson(this.id).then(
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
