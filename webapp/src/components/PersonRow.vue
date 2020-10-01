<template>
  <tr>
    <td>{{ person.givenName }}</td>
    <td>{{ person.familyName }}</td>
    <td><a v-if="person.mailAddress !== null" :href="`mailto:${person.mailAddress}`">{{ person.mailAddress }}</a></td>
  </tr>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Person } from '../services/person'

export default defineComponent({
  props: {
    person: {
      type: Person,
      required: true
    }
  }
})
</script>
