<template>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="page-loaded-container">
    <form @submit="submit">
      <div class="form-group">
        <label for="givenName">Given name</label>
        <input type="text" v-model="givenName" id="givenName" class="form-control" required>
      </div>
      <div class="form-group">
        <label for="familyName">Family name</label>
        <input type="text" v-model="familyName" id="familyName" class="form-control" required>
      </div>
      <div class="form-group">
        <label for="mailAddress">Mail address</label>
        <input type="email" v-model="mailAddress" id="mailAddress" class="form-control">
      </div>
      <a @click="cancel" class="btn btn-secondary">Back</a>
      <button type="submit" class="btn btn-primary" :disabled="disabled">Save</button>
    </form>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { PersonResponse2, getPerson, createPerson } from '../services/person'
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
    const router = useRouter()
    const state = ref({ loaded: false, error: null })
    const person = ref<PersonResponse2 | null>(null)

    const givenName = ref('')
    const familyName = ref('')
    const mailAddress = ref('')

    let exists = props.id !== 'new'

    function onResponse (response: PersonResponse2) {
      state.value.loaded = true
      document.title = response.givenName + ' ' + response.familyName + ' - Korga'
      givenName.value = response.givenName
      familyName.value = response.familyName
      mailAddress.value = response.mailAddress ?? ''
      person.value = response

      if (!exists) {
        router.replace({ name: 'Person', params: { id: response.id.toString() } })
        exists = true
      }
    }

    onMounted(() => {
      if (exists) {
        document.title = 'Loading - Korga'
        getPerson(parseInt(props.id)).then(
          onResponse,
          error => {
            state.value.error = error
            document.title = 'Error - Korga'
          })
      } else {
        document.title = 'Create person - Korga'
        state.value.loaded = true
      }
    })

    const disabled = computed(() => {
      if (givenName.value === '' || familyName.value === '' ||
          (person.value !== null &&
          givenName.value === person.value.givenName &&
          familyName.value === person.value.familyName &&
          mailAddress.value === (person.value.mailAddress ?? ''))) {
        return true
      } else {
        return false
      }
    })

    const submit = function (e: Event) {
      if (exists) {
        // TODO implement update
      } else {
        state.value.loaded = false
        createPerson({
          givenName: givenName.value,
          familyName: familyName.value,
          mailAddress: mailAddress.value === '' ? null : mailAddress.value
        }).then(
          onResponse,
          error => {
            state.value.error = error
            document.title = 'Error - Korga'
          })
      }
      e.preventDefault()
    }

    const cancel = function (e: Event) {
      router.back()
      e.preventDefault()
    }

    return {
      state,
      givenName,
      familyName,
      mailAddress,
      disabled,
      submit,
      cancel
    }
  }
})
</script>
