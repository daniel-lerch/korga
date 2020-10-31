import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'
import People from '../views/People.vue'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/people',
    name: 'People',
    component: People
  },
  {
    path: '/person/:id',
    name: 'Person',
    component: () => import('../views/Person.vue'),
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router
