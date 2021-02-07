<template>
  <Loading v-if="state.loaded === false" :state="state" />
  <div v-else class="container page-loaded-container pt-3" :class="{ deleted: deleted }">
    <div class="row">
      <div class="col">
        <h2>{{ title }}</h2>
      </div>
      <div v-if="person !== null && deleted === false" class="col-auto align-self-center">

        <button type="button" class="btn btn-outline-danger" @click="showModal = true">
          <FontAwesomeIcon icon="trash"></FontAwesomeIcon>
        </button>

        <Modal
          v-if="showModal"
          @close="showModal = false"
          @continue="archive"
          :title="'Archive ' + person.givenName + '?'"
          :body="person.givenName + ' ' + person.familyName + ' is about to be archived. This operation cannot be undone!'">
        </Modal>

      </div>
    </div>
    <div v-if="person !== null" class="row">
      <div class="col-md"><small>{{ creation }}</small></div>
      <div class="col-md text-md-right"><small>{{ deletion }}</small></div>
    </div>
    <hr v-if="person !== null">
    <form @submit.prevent="submit">
      <div class="form-row">
        <div class="form-group col-md-6">
          <label for="givenName">Given name</label>
          <input type="text" v-model="givenName" id="givenName" class="form-control" :disabled="deleted" required>
        </div>
        <div class="form-group col-md-6">
          <label for="familyName">Family name</label>
          <input type="text" v-model="familyName" id="familyName" class="form-control" :disabled="deleted" required>
        </div>
      </div>
      <div class="form-group">
        <label for="mailAddress">Mail address</label>
        <input type="email" v-model="mailAddress" id="mailAddress" class="form-control" :disabled="deleted">
      </div>
      <a @click.prevent="cancel" class="btn btn-secondary mr-2">Back</a>
      <button type="submit" class="btn btn-primary" :disabled="disabled">Save</button>
    </form>
    <div v-if="person !== null" :class="{ 'text-muted': deleted }">
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
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { PersonRequest, PersonResponse2, getPerson, createPerson, updatePerson, PersonMembership, PersonResponse, deletePerson } from '../services/person'
import Loading from '@/components/Loading.vue'
import Modal from '@/components/Modal.vue'

export default defineComponent({
  components: {
    FontAwesomeIcon,
    Loading,
    Modal
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
    const showModal = ref(false)

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

    const deleted = computed(() => {
      return person.value !== null && person.value.deletionTime !== null
    })

    const disabled = computed(() => {
      return (givenName.value === '' || familyName.value === '' ||
          (person.value !== null &&
          givenName.value === person.value.givenName &&
          familyName.value === person.value.familyName &&
          mailAddress.value === (person.value.mailAddress ?? '')))
    })

    function createTimeString (prepend: string, actionTime: Date | null | undefined, author: PersonResponse | null | undefined) {
      if (actionTime != null) {
        let msg = prepend + actionTime.toLocaleDateString()
        if (author != null) {
          msg += ' by ' + author.givenName + ' ' + author.familyName
        }
        return msg
      } else {
        return null
      }
    }

    const creation = computed(() => createTimeString('Created on ', person.value?.creationTime, person.value?.createdBy))
    const deletion = computed(() => createTimeString('Deleted on ', person.value?.deletionTime, person.value?.deletedBy))

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

    const submit = function () {
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
        // TODO: Show a toast message in case of 409 Conflict
        updatePerson(id, request).then(onResponse, onError)
      }
    }

    const archive = function () {
      const id = person.value?.id ?? -1
      if (id === -1) {
        console.warn('It should not be possible to archive a person if it is already archived.')
      } else {
        state.value.loaded = false
        deletePerson(id).then(onResponse, onError)
      }
    }

    const cancel = function () {
      router.back()
    }

    return {
      state,
      person,
      showModal,
      givenName,
      familyName,
      mailAddress,
      title,
      deleted,
      disabled,
      creation,
      deletion,
      memberships,
      submit,
      archive,
      cancel
    }
  }
})
</script>

<style scoped>
.deleted h2 {
  color: #50575e!important;
  text-decoration: line-through solid #6c757d;
}
.deleted label {
  color: #6c757d!important;
}
</style>
