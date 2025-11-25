<template>
  <div class="flex flex-col min-h-screen">
    <header>
      <Menubar :model="items" class="m-2">
        <template #start>
          <router-link to="/" class="flex items-center gap-2 no-underline">
            <img src="/brand.png" alt="Logo" width="24" height="24" class="inline-block" />
            <span class="text-2xl font-semibold text-inherit">Mailist</span>
          </router-link>
        </template>

        <template #item="{ item, props, hasSubmenu }">
          <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
            <a v-bind="props.action" :href="href" @click="navigate">
              <span v-if="item.icon" :class="item.icon" />
              <span>{{ item.label }}</span>
            </a>
          </router-link>
          <a v-else v-bind="props.action" :href="item.url" :target="item.target">
            <span v-if="item.icon" :class="item.icon" />
            <span>{{ item.label }}</span>
            <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down" />
          </a>
        </template>
      </Menubar>
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
import Menubar from "primevue/menubar"
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { useExtensionStore } from "./stores/extension"
import { ref } from "vue"

const churchtoolsUrl = window.settings?.base_url ?? import.meta.env.VITE_CHURCHTOOLS_URL
churchtoolsClient.setBaseUrl(churchtoolsUrl)

const extension = useExtensionStore()

const items = ref([
  {
    label: "E-Mail-Verteiler",
    icon: "pi pi-fw pi-inbox",
    route: "/",
  },
  {
    label: "Einstellungen",
    icon: "pi pi-fw pi-cog",
    route: "/setup",
  },
])

</script>
