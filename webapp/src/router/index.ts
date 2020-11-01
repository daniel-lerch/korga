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
  },
  {
    path: '/groups',
    name: 'Groups',
    component: () => import('../views/Groups.vue')
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router
