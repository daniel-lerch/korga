import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    redirect: "/service",
  },
  {
    path: "/service",
    name: "Service",
    component: () =>
      import(/* webpackChunkName: "service" */ "../views/ServiceList.vue"),
  },
  {
    path: "/distribution-lists",
    name: "DistributionLists",
    component: () =>
      import(
        /* webpackChunkName: "distribution-list" */ "../views/DistributionLists.vue"
      ),
  },
];

const router = createRouter({
  history: createWebHistory(window.resourceBasePath),
  routes,
});

export default router;
