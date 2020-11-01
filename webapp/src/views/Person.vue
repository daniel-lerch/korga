<template>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="page-loaded-container">
    Vorname: {{ person.givenName }}<br>
    Nachname: {{ person.familyName }}
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from 'vue'
import { PersonResponse2, getPerson } from '../services/person'
import Loading from '@/components/Loading.vue'

export default defineComponent({
  components: {
    Loading
  },
  props: {
    id: {
      type: String,
      required: true
    }
  },
  setup (props) {
    const person = ref<PersonResponse2 | null>(null)
    const state = ref({ loaded: false, error: null })

    onMounted(() => {
      document.title = 'Loading - Korga'
      getPerson(parseInt(props.id)).then(
        response => {
          state.value.loaded = true
          person.value = response
          document.title = response.givenName + ' ' + response.familyName + ' - Korga'
        },
        error => {
          state.value.error = error
          document.title = 'Error - Korga'
        })
    })

    return {
      person,
      state
    }
  }
})
</script>
