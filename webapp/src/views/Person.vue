<template>
  <p>{{ person?.givenName }}</p>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { getPerson } from '../services/person'

export default defineComponent({
  data () {
    return {
      person: null,
      state: 0,
      errorMessage: ''
    }
  },
  mounted () {
    getPerson(this.$route.params.id).then(
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
