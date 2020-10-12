<template>
  <div class="row">
    <div class="col">
      <router-link :to="link" class="subdued">{{ person.givenName }}</router-link>
    </div>
    <div class="col">
      <router-link :to="link" class="subdued">{{ person.familyName }}</router-link>
    </div>
    <div class="col">
      <a v-if="person.mailAddress !== null" :href="`mailto:${person.mailAddress}`">{{ person.mailAddress }}</a>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType } from 'vue'
import { Person } from '../services/person'

export default defineComponent({
  props: {
    person: {
      type: Object as PropType<Person>,
      required: true
    }
  },
  data () {
    return {
      link: {
        name: 'Person',
        params: {
          id: this.person.id
        }
      }
    }
  }
})
</script>
