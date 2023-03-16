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
import Login from "./pages/Login.vue";
import Domains from "./pages/Domains.vue";
import DomainDetails from "./pages/DomainDetails.vue";
import Users from "./pages/Users.vue";
import UserDetails from "./pages/UserDetails.vue";

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
      path: "/login",
      name: "route.login",
      component: Login,
    },
    {
      path: "/domains",
      name: "route.domains",
      component: Domains,
    },
    {
      path: "/domains/:id(\\d+)",
      name: "route.domainDetails",
      component: DomainDetails,
      props: parseId,
    },
    {
      path: "/users",
      name: "route.users",
      component: Users,
    },
    {
      path: "/users/:id(\\d+)",
      name: "route.userDetails",
      component: UserDetails,
      props: parseId,
    },
    { path: "/:pathMatch(.*)*", name: "NotFound", component: Index }, // NotFound
  ],
});

createApp(App).use(router).mount("#app");
