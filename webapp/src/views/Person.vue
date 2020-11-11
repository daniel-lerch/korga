<template>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="page-loaded-container">
    <form @submit="submit">
      <div class="form-group">
        <label for="givenName">Given name</label>
        <input type="text" v-model="person.givenName" id="givenName" class="form-control" required>
      </div>
      <div class="form-group">
        <label for="familyName">Family name</label>
        <input type="text" v-model="person.familyName" id="familyName" class="form-control" required>
      </div>
      <div class="form-group">
        <label for="mailAddress">Mail address</label>
        <input type="email" v-model="person.mailAddress" id="mailAddress" class="form-control">
      </div>
      <button type="submit" v-if="!exists" class="btn btn-primary">Create</button>
    </form>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from 'vue'
import { PersonResponse, getPerson, createPerson } from '../services/person'
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
    const person = ref<PersonResponse | null>(null)
    const state = ref({ loaded: false, error: null })
    const exists = ref(props.id !== 'new')

    onMounted(() => {
      if (exists.value) {
        document.title = 'Loading - Korga'
        getPerson(parseInt(props.id)).then(
          response => {
            person.value = response
            state.value.loaded = true
            document.title = response.givenName + ' ' + response.familyName + ' - Korga'
          },
          error => {
            state.value.error = error
            document.title = 'Error - Korga'
          })
      } else {
        document.title = 'Create person - Korga'
        person.value = {
          id: 0,
          givenName: '',
          familyName: '',
          mailAddress: null
        }
        state.value.loaded = true
      }
    })

    const submit = function (e: Event) {
      if (person.value === null) {
        throw new Error('You cannot submit a form without a data object')
      }
      if (exists.value) {
        // TODO implement update
      } else {
        state.value.loaded = false
        createPerson({
          givenName: person.value.givenName,
          familyName: person.value.familyName,
          mailAddress: person.value.mailAddress
        }).then(response => {
          person.value = response
          state.value.loaded = true
          document.title = response.givenName + ' ' + response.familyName + ' - Korga'
          // TODO change router path
        })
      }
      e.preventDefault()
    }

    return {
      person,
      state,
      exists,
      submit
    }
  }
})
</script>
