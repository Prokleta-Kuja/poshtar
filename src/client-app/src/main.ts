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
import Domain from "./pages/domain";
import User from "./pages/user";

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
      name: "route.domains",
      component: Domain.Domains,
    },
    {
      path: "/domains/:id(\\d+)",
      name: "route.domainDetails",
      component: Domain.DomainDetails,
      props: parseId,
    },
    {
      path: "/users",
      name: "route.userList",
      component: User.List,
    },
    {
      path: "/users/:id(\\d+)",
      name: "route.userEdit",
      component: User.Edit,
      props: parseId,
    },
    {
      path: "/users/:id",
      name: "route.userCreate",
      component: User.Create,
    },
    { path: "/:pathMatch(.*)*", name: "NotFound", component: Index }, // NotFound
  ],
});

createApp(App).use(router).mount("#app");
