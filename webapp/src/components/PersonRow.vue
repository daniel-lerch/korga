<template>
  <div class="row" :class="{ deleted: person.deleted }">
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
import { PersonResponse } from '../services/person'

export default defineComponent({
  props: {
    person: {
      type: Object as PropType<PersonResponse>,
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

<style scoped>
.deleted, .deleted a {
  color: #6c757d!important;
  text-decoration: line-through solid #6c757d;
}

.col {
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}
</style>
