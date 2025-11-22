<template>
  <header>
    <nav class="navbar navbar-expand-sm navbar-dark bg-primary">
      <div class="container-fluid">
        <router-link to="/" class="navbar-brand">
          <img src="/brand.png" alt="Logo" width="24" height="24" class="d-inline-block align-text-top mx-1" />
          Korga
        </router-link>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navbarSupportedContent">
          <ul class="navbar-nav">
            <router-link to="/" class="nav-link">
              E-Mail-Verteiler
            </router-link>
          </ul>
          <ul class="navbar-nav">
            <router-link to="/setup" class="nav-link">
              Einstellungen
            </router-link>
          </ul>
        </div>
      </div>
    </nav>
  </header>
  <main class="content">
    <div v-if="extension.moduleId === 0">Loading...</div>
    <Suspense v-else>
      <template #default>
        <router-view v-bind="$attrs" />
      </template>
      <template #fallback>
        <div class="page-load-container">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
      </template>
    </Suspense>
  </main>
  <footer>
    <div>
      <small>
        Copyright &copy; 2022-2025 Daniel Lerch and Benjamin Stieler
      </small>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { useExtensionStore } from "./stores/extension"
import { Suspense } from "vue"

const churchtoolsUrl = window.settings?.base_url ?? import.meta.env.VITE_CHURCHTOOLS_URL
churchtoolsClient.setBaseUrl(churchtoolsUrl)

const extension = useExtensionStore()

</script>

<style>
div#app {
  display: flex;
  flex-flow: column;
  min-height: 100vh;
}

main.content {
  flex: 1;
  overflow: auto;
}

footer {
  min-height: 40px;
  padding: 8px 16px;
}

/* Apply Bootstrap active link design to active router-link */
.navbar-light .navbar-nav a.router-link-active {
  color: rgba(0, 0, 0, 0.9) !important;
}

.navbar-dark .navbar-nav a.router-link-active {
  color: #fff !important;
}

a.subdued {
  color: inherit;
  text-decoration-line: none;
}

a.subdued:hover {
  text-decoration-line: underline;
}

.page-load-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
  animation: fadein 1.4s;
}

.page-loaded-container {
  animation: fadein 0.7s;
}

.page-load-container .spinner-border {
  width: 3rem;
  height: 3rem;
}

@keyframes fadein {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}
</style>
