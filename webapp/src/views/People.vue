<template>
  <div>
    <div v-if="errorMessage !== ''">{{ errorMessage }}</div>
    <table>
      <th>Given name</th>
      <th>Family name</th>
      <th>Mail address</th>
      <PersonRow v-for="person in people" :key="person.id" v-bind:person="person" />
    </table>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Person, getPeople } from '../services/person'
import PersonRow from '@/components/PersonRow.vue'

export default defineComponent({
  components: {
    PersonRow
  },
  data () {
    return {
      people: new Array<Person>(),
      errorMessage: ''
    }
  },
  mounted () {
    getPeople().then(response => this.people.push(...response), error => { this.errorMessage = error.toString() })
  }
})
</script>
