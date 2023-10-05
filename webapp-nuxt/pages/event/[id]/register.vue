<template>
  <div class="container">
    <h1>Anmeldung</h1>
    <div class="card shadow">
      <div class="card-body">
        <form @submit.prevent="register" class="mw-100">
          <h2>{{ event?.name }}</h2>
          <div class="mb-3">
            <label for="givenName" class="form-label">Vorname</label>
            <input
              type="text"
              v-model="givenName"
              id="givenName"
              class="form-control"
              placeholder="Max"
              required
            />
          </div>
          <div class="mb-3">
            <label for="givenName" class="form-label">Nachname</label>
            <input
              type="text"
              v-model="familyName"
              id="familyName"
              class="form-control"
              placeholder="Mustermann"
              required
            />
          </div>
          <div>
            <div
              class="form-check"
              v-for="program in event?.programs"
              :key="program.id"
            >
              <input
                class="form-check-input"
                type="radio"
                name="exampleRadios"
                :id="`radio${program.id}`"
                :value="program.id"
                v-model="programId"
                :disabled="program.participants.length >= program.limit"
                required
              />
              <label class="form-check-label" :for="`radio${program.id}`">
                {{ program.name }} {{ program.participants.length }}/{{
                  program.limit
                }}
              </label>
            </div>
          </div>
          <div
            class="alert alert-warning alert-dismissible"
            role="alert"
            v-if="alreadyRegistered"
            style="margin-top: 15px"
          >
            Diese Person ist bereits zu diesem Event angemeldet
          </div>
          <button
            type="submit"
            class="btn btn-emphasize mt-3 w-100"
            :class="{
              'btn-success': !alreadyRegistered,
              'btn-danger': alreadyRegistered,
            }"
            :disabled="full"
          >
            {{ alreadyRegistered ? "Trotzdem Anmelden" : "Anmelden" }}
          </button>
          <div class="alert alert-danger" role="alert" v-if="full">
            Alle Plätze sind bereits belegt.
          </div>
          <div
            class="alert alert-danger alert-dismissible"
            role="alert"
            v-else-if="error"
          >
            {{ error }}
          </div>
        </form>
      </div>
    </div>
  </div>
</template>