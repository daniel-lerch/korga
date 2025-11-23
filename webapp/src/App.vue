<template>
  <div class="flex flex-col min-h-screen">
    <header>
      <nav class="max-w-7xl mx-auto px-4">
        <div class="flex items-center justify-between h-14">
          <div class="flex items-center gap-3">
            <router-link to="/" class="flex items-center gap-2 no-underline">
              <img src="/brand.png" alt="Logo" width="24" height="24" class="inline-block" />
              <span class="text-3xl">Mailist</span>
            </router-link>
          </div>
          <div class="nav-links flex items-center gap-2">
            <router-link to="/" class="px-3 py-2 rounded">E-Mail-Verteiler</router-link>
            <router-link to="/setup" class="px-3 py-2 rounded">Einstellungen</router-link>
          </div>
        </div>
      </nav>
    </header>
    <main class="grow">
      <div v-if="extension.moduleId === 0">Loading...</div>
      <Suspense v-else>
        <template #default>
          <router-view v-bind="$attrs" />
        </template>
        <template #fallback>
          <div>Loading...</div>
        </template>
      </Suspense>
    </main>
    <footer>
      <div class="max-w-7xl mx-auto px-4 py-3 text-gray-600">
        <small class="text-sm text-gray-500">
          Copyright &copy; 2022-2025 Daniel Lerch and Benjamin Stieler
        </small>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { useExtensionStore } from "./stores/extension"

const churchtoolsUrl = window.settings?.base_url ?? import.meta.env.VITE_CHURCHTOOLS_URL
churchtoolsClient.setBaseUrl(churchtoolsUrl)

const extension = useExtensionStore()
</script>
