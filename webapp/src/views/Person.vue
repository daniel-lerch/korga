<template>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="container page-loaded-container pt-3">
    <h2>{{ title }}</h2>
    <form @submit="submit">
      <div class="form-row">
        <div class="form-group col-md-6">
          <label for="givenName">Given name</label>
          <input type="text" v-model="givenName" id="givenName" class="form-control" required>
        </div>
        <div class="form-group col-md-6">
          <label for="familyName">Family name</label>
          <input type="text" v-model="familyName" id="familyName" class="form-control" required>
        </div>
      </div>
      <div class="form-group">
        <label for="mailAddress">Mail address</label>
        <input type="email" v-model="mailAddress" id="mailAddress" class="form-control">
      </div>
      <a @click="cancel" class="btn btn-secondary mr-2">Back</a>
      <button type="submit" class="btn btn-primary" :disabled="disabled">Save</button>
    </form>
    <div v-if="person !== null">
      <hr>
      <p v-if="person.memberships.length === 0">No group memberships so far</p>
      <h5 v-if="person.memberships.length > 0">Group memberships</h5>
      <ul v-if="person.memberships.length > 0" class="list-group">
        <li v-for="group in memberships.values()" :key="group[0].groupId" class="list-group-item">
          {{ group[0].groupName }}
          <span v-for="membership in group" :key="membership.id" class="badge badge-primary badge-pill">
            {{ membership.roleName }}
          </span>
        </li>
      </ul>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { PersonRequest, PersonResponse2, getPerson, createPerson, updatePerson, PersonMembership } from '../services/person'
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
    const state = ref({ loaded: false, error: null as Error | null })
    const person = ref<PersonResponse2 | null>(null)

    const givenName = ref('')
    const familyName = ref('')
    const mailAddress = ref('')

    function onResponse (response: PersonResponse2) {
      state.value.loaded = true
      document.title = response.givenName + ' ' + response.familyName + ' - Korga'

      if (person.value === null) {
        router.replace({ name: 'Person', params: { id: response.id.toString() } })
      }

      givenName.value = response.givenName
      familyName.value = response.familyName
      mailAddress.value = response.mailAddress ?? ''
      person.value = response
    }

    function onError (error: Error) {
      state.value.error = error
      document.title = 'Error - Korga'
    }

    onMounted(() => {
      if (props.id !== 'new') {
        document.title = 'Loading - Korga'
        getPerson(parseInt(props.id)).then(onResponse, onError)
      } else {
        document.title = 'Create person - Korga'
        state.value.loaded = true
      }
    })

    const title = computed(() => {
      if (person.value === null) {
        return 'Create a person'
      } else {
        return person.value.givenName + ' ' + person.value.familyName
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

    const memberships = computed(() => {
      if (person.value !== null) {
        return person.value.memberships.reduce(function (result, element) {
          const group = result.get(element.groupId)
          if (group !== undefined) {
            group.push(element)
          } else {
            result.set(element.groupId, [element])
          }
          return result
        }, new Map<number, PersonMembership[]>())
      } else {
        return null
      }
    })

    const submit = function (e: Event) {
      const request: PersonRequest = {
        givenName: givenName.value,
        familyName: familyName.value,
        mailAddress: mailAddress.value === '' ? null : mailAddress.value
      }
      state.value.loaded = false
      const id = person.value?.id ?? -1
      if (id === -1) {
        createPerson(request).then(onResponse, onError)
      } else {
        updatePerson(id, request).then(onResponse, onError)
      }
      e.preventDefault()
    }

    const cancel = function (e: Event) {
      router.back()
      e.preventDefault()
    }

    return {
      state,
      person,
      givenName,
      familyName,
      mailAddress,
      title,
      disabled,
      memberships,
      submit,
      cancel
    }
  }
})
</script>
