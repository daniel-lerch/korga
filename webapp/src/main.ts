import { createApp } from "vue"
import { createPinia } from "pinia"
import App from "./App.vue"
import router from "./router"
import "bootstrap"
import "./custom_styles.scss"
createApp(App).use(router).use(createPinia()).mount("#app")
