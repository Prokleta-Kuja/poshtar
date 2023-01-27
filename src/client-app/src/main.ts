import { createApp } from "vue";
import { createRouter, createWebHistory } from "vue-router";
import "./styles.scss";
import App from "./App.vue";
import Index from "./views/Index.vue";
import IndexId from "./views/IndexId.vue";
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
