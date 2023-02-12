import { createApp } from "vue";
import { createRouter, createWebHistory } from "vue-router";
import "~bootstrap";
import "./styles.scss";
import App from "./App.vue";
import Index from "./pages/Index.vue";
import IndexId from "./pages/IndexId.vue";
import Domains from "./pages/Domains.vue";

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
      component: Domains,
    },
    {
      path: "/domains/:id(\\d+)",
      name: "route.domain",
      component: IndexId,
      props: true,
    },
    {
      path: "/users",
      name: "route.users",
      component: Index,
    },
    { path: "/:pathMatch(.*)*", name: "NotFound", component: Index }, // NotFound
  ],
});

createApp(App).use(router).mount("#app");
