// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  css: ['~/assets/main.scss'],
  devtools: { enabled: true },
  modules: [
    '@bootstrap-vue-next/nuxt',
    '@pinia/nuxt'
  ]
})
