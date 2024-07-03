import { fileURLToPath, URL } from "node:url"

import { defineConfig, loadEnv } from "vite"
import vue from "@vitejs/plugin-vue"

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())

  const proxy = {} as { [key: string]: string }
  proxy[env.VITE_API_BASE_PATH + "api"] = env.VITE_API_ORIGIN
  proxy[env.VITE_API_BASE_PATH + "signin-oidc"] = env.VITE_API_ORIGIN
  proxy[env.VITE_API_BASE_PATH + "signout-callback-oidc"] = env.VITE_API_ORIGIN

  return {
    plugins: [vue()],
    resolve: {
      alias: {
        "@": fileURLToPath(new URL("./src", import.meta.url)),
      },
    },
    server: {
      proxy: proxy,
    },
    experimental: {
      renderBuiltUrl(filename, { hostType }) {
        if (hostType === "js")
          return { runtime: `window.basePath + ${JSON.stringify(filename)}` }
        else return "/__base_path__/" + filename
      },
    },
  }
})
