import type { RouteRecordRaw } from "vue-router"
import { createRouter, createWebHistory } from "vue-router"

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "DistributionLists",
    component: () => import("./views/DistributionLists.vue"),
  },
  {
    path: "/create",
    name: "CreateDistributionList",
    component: () => import("./views/CreateDistributionList.vue"),
  },
  {
    path: "/setup",
    name: "Setup",
    component: () => import("./views/ExtensionSetup.vue"),
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

export default router
