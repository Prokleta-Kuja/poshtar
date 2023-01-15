import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "~bootstrap": "../node_modules/bootstrap/js/index.esm.js",
    },
  },
});
