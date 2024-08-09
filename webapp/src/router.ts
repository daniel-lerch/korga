import type { RouteRecordRaw } from "vue-router"
import { createRouter, createWebHistory } from "vue-router"

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "Dashboard",
    component: () => import("./views/DashboardView.vue"),
  },
  {
    path: "/service-history",
    name: "ServiceHistory",
    component: () => import("./views/ServiceHistory.vue"),
  },
  {
    path: "/distribution-lists",
    name: "DistributionLists",
    component: () => import("./views/DistributionLists.vue"),
  },
  {
    path: "/permissions",
    name: "Permissions",
    component: () => import("./views/PermissionsView.vue"),
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.PROD ? window.basePath : "/"),
  routes,
})

export default router
