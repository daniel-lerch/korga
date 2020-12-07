import { createApp } from 'vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faTrash, faUserPlus } from '@fortawesome/free-solid-svg-icons'
import App from './App.vue'
import router from './router'
import store from './store'

library.add(faTrash, faUserPlus)
createApp(App).use(store).use(router).mount('#app')
