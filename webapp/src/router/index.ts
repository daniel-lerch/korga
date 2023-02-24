import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    redirect: "/events",
  },
  {
    path: "/events",
    name: "Events",
    component: () =>
      import(/* webpackChunkName: "events" */ "../views/EventList.vue"),
  },
  {
    path: "/event/:id/register",
    name: "Register",
    component: () =>
      import(/* webpackChunkName: "event" */ "../views/EventRegistration.vue"),
    props: true,
  },
  {
    path: "/admin",
    name: "Admin",
    component: () =>
      import(/* webpackChunkName: "event" */ "../views/Admin.vue"),
  },
  {
    path: "/event/:id",
    name: "Event",
    component: () =>
      import(/* webpackChunkName: "event" */ "../views/EventDetails.vue"),
    props: true,
  },
  {
    path: "/password",
    name: "Password",
    component: () =>
      import(/* webpackChunkName: "password" */ "../views/PasswordHash.vue"),
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
