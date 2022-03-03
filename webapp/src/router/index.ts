import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";
import People from "../views/People.vue";

const routes: Array<RouteRecordRaw> = [
  {
    path: "/people",
    name: "People",
    component: People,
  },
  {
    path: "/person/:id",
    name: "Person",
    component: () =>
      import(/* webpackChunkName: "person" */ "../views/Person.vue"),
    props: true,
  },
  {
    path: "/groups",
    name: "Groups",
    component: () =>
      import(/* webpackChunkName: "groups" */ "../views/Groups.vue"),
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
    name: "List",
    component: () =>
      import(/* webpackChunkName: "event" */ "../views/EventDetails.vue"),
    props: true,
  },
];

const router = createRouter({
  history: createWebHistory(window.resourceBasePath),
  routes,
});

export default router;
