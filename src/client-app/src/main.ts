import { createApp } from 'vue'
import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router'
import App from './App.vue'
import Index from './pages/Index.vue'
import Login from './pages/Login.vue'
import Logout from './pages/Logout.vue'
import Domains from './pages/Domains.vue'
import DomainDetails from './pages/DomainDetails.vue'
import Users from './pages/Users.vue'
import UserDetails from './pages/UserDetails.vue'
import { createPinia } from 'pinia'
import { useAuth } from './stores/auth.store'

import './assets/app.css'
import './assets/bootstrap.min.css'
import './assets/bootstrap.bundle.min.js'

const parseId = (route: RouteLocationNormalized) => {
  let parsed = parseInt(route.params.id.toString())
  if (isNaN(parsed)) parsed = 0

  return { ...route.params, id: parsed }
}
const pinia = createPinia()
const router = createRouter({
  linkActiveClass: 'active',
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'route.root',
      component: Index
    },
    {
      path: '/login',
      name: 'route.login',
      component: Login
    },
    {
      path: '/logout',
      name: 'route.logout',
      component: Logout
    },
    {
      path: '/domains',
      name: 'route.domains',
      component: Domains
    },
    {
      path: '/domains/:id(\\d+)',
      name: 'route.domainDetails',
      component: DomainDetails,
      props: parseId
    },
    {
      path: '/users',
      name: 'route.users',
      component: Users
    },
    {
      path: '/users/:id(\\d+)',
      name: 'route.userDetails',
      component: UserDetails,
      props: parseId
    },
    { path: '/:pathMatch(.*)*', name: 'NotFound', component: Index } // NotFound
  ]
})

const publicPages = ['/login', '/logout']
router.beforeEach(async (to) => {
  const auth = useAuth()
  const authRequired = !publicPages.includes(to.path)

  // Must wait for auth to intialize before making a decision
  while (!auth.initialized) await new Promise((f) => setTimeout(f, 500))

  if (!auth.isAuthenticated && authRequired) return { name: 'route.login' }
})

const app = createApp(App)
app.use(router).use(pinia).mount('#app')
