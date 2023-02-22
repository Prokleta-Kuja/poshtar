import { createApp } from "vue";
import {
  createRouter,
  createWebHistory,
  RouteLocationNormalized,
} from "vue-router";
import "~bootstrap";
import "./styles.scss";
import App from "./App.vue";
import Index from "./pages/Index.vue";
import IndexId from "./pages/IndexId.vue";
import Domain from "./pages/domain";

const parseId = (route: RouteLocationNormalized) => {
  let parsed = parseInt(route.params.id.toString());
  if (isNaN(parsed)) parsed = 0;

  return { ...route.params, id: parsed };
};
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
      name: "route.domainList",
      component: Domain.List,
    },
    {
      path: "/domains/:id(\\d+)",
      name: "route.domainEdit",
      component: Domain.Edit,
      props: parseId,
    },
    {
      path: "/domains/:id",
      name: "route.domainCreate",
      component: Domain.Create,
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
