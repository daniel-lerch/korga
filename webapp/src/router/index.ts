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
      import(/* webpackChunkName: "events" */ "../views/Events.vue"),
  },
  {
    path: "/event/:id",
    name: "Event",
    component: () =>
      import(/* webpackChunkName: "events" */ "../views/Event.vue"),
  },
];

const router = createRouter({
  history: createWebHistory(window.resourceBasePath),
  routes,
});

export default router;
