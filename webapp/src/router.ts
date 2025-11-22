import type { RouteRecordRaw } from "vue-router"
import { createRouter, createWebHistory } from "vue-router"
import { useExtensionStore } from "./stores/extension"

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "DistributionLists",
    component: () => import("./views/DistributionLists.vue"),
  },
  {
    path: "/setup",
    name: "Setup",
    component: () => import("./views/ExtensionSetup.vue"),
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach(async (to, from, next) => {
  const extension = useExtensionStore()
  try {
    if (extension.moduleId === 0) {
      await extension.load()
    }

    // If the extension hasn't been configured (no backendUrl), force the
    // user to the setup page unless they're already heading there.
    const backendUrl = extension.backendUrl ?? ""
    if (backendUrl.trim() === "" && to.path !== "/setup") {
      return next({ path: "/setup" })
    }

    next()
  } catch (e) {
    if (e instanceof Error) {
      next(e)
    } else {
      next(new Error("Unknown error during extension load: " + e))
    }
  }
})

export default router
