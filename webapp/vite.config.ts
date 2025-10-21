import { fileURLToPath, URL } from "node:url"

import { defineConfig, loadEnv, ProxyOptions } from "vite"
import vue from "@vitejs/plugin-vue"
import vueDevTools from "vite-plugin-vue-devtools"

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())

  const proxy = {} as Record<string, string | ProxyOptions>
  const options = {
    target: env.VITE_API_ORIGIN,
    changeOrigin: false,
  }
  proxy[env.VITE_API_BASE_PATH + "api"] = options
  return {
    plugins: [
      vue(),
      vueDevTools()
    ],
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
