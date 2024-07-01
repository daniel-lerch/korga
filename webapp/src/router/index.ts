import type { RouteRecordRaw } from "vue-router"
import { createRouter, createWebHistory } from "vue-router"

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    redirect: "/service",
  },
  {
    path: "/service",
    name: "Service",
    component: () => import("../views/ServiceList.vue"),
  },
  {
    path: "/distribution-lists",
    name: "DistributionLists",
    component: () => import("../views/DistributionLists.vue"),
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.PROD ? window.basePath : "/"),
  routes,
})

export default router
