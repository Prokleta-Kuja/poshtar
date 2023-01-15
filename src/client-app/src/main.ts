import { createApp } from "vue";
import { createRouter, createWebHistory } from "vue-router";
import "./styles.scss";
import App from "./App.vue";
import Index from "./pages/Index.vue";
import IndexId from "./pages/IndexId.vue";
import "~bootstrap";

const router = createRouter({
  linkActiveClass: "active",
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "route.root",
      component: Index,
    },
    {
      path: "/domains",
      name: "route.domains",
      component: Index,
    },
    {
      path: "/domains/:id",
      name: "route.domain",
      component: IndexId,
    },
    {
      path: "/users",
      name: "route.users",
      component: Index,
    },
  ],
});

createApp(App).use(router).mount("#app");
