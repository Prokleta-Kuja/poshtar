import { createApp } from 'vue'
import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router'
import App from './App.vue'
import IndexView from './views/IndexView.vue'
import LoginView from './views/LoginView.vue'
import LogoutView from './views/LogoutView.vue'
import UsersView from './views/UsersView.vue'
import UserDetailsView from './views/UserDetailsView.vue'
import DomainsView from './views/DomainsView.vue'
import DomainDetailsView from './views/DomainDetailsView.vue'
import AntiSpamView from './views/AntiSpamView.vue'
import CalendarsView from './views/CalendarsView.vue'
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
  scrollBehavior: (to, from, savedPosition) => {
    if (savedPosition) {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve(savedPosition)
        }, 250)
      })
    } else {
      return { top: 0 }
    }
  },
  routes: [
    {
      path: '/',
      name: 'route.root',
      component: IndexView
    },
    {
      path: '/login',
      name: 'route.login',
      component: LoginView
    },
    {
      path: '/logout',
      name: 'route.logout',
      component: LogoutView
    },
    {
      path: '/users',
      name: 'route.users',
      component: UsersView
    },
    {
      path: '/users/:id(\\d+)',
      name: 'route.userDetails',
      component: UserDetailsView,
      props: parseId
    },
    {
      path: '/domains',
      name: 'route.domains',
      component: DomainsView
    },
    {
      path: '/domains/:id(\\d+)',
      name: 'route.domainDetails',
      component: DomainDetailsView,
      props: parseId
    },
    {
      path: '/anti-spam',
      name: 'route.antispam',
      component: AntiSpamView
    },
    {
      path: '/calendars',
      name: 'route.calendars',
      component: CalendarsView
    },
    { path: '/:pathMatch(.*)*', name: 'NotFound', component: IndexView } // NotFound
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
