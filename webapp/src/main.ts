import { createPinia } from "pinia"
import { createApp } from "vue"
import App from "./App.vue"
import PrimeVue from "primevue/config"
import ConfirmationService from "primevue/confirmationservice"
import Aura from "@primeuix/themes/aura"
import router from "./router"

const app = createApp(App)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
  },
})
app.use(ConfirmationService)
app.use(router)
app.use(createPinia())
app.mount("#app")
