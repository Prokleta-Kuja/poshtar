<script setup lang="ts">
import DovecotIndicator from './components/DovecotIndicator.vue'
import TotpIndicator from './components/TotpIndicator.vue'
import ThreeDotsVerticalIcon from './components/icons/ThreeDotsVerticalIcon.vue'
import CalendarRangeFillIcon from './components/icons/CalendarRangeFillIcon.vue'
import BoxArrowRightIcon from './components/icons/BoxArrowRightIcon.vue'
import { useAuth } from './stores/auth.store'

const auth = useAuth()
auth.initialize()
</script>
<template>
  <template v-if="!auth.initialized">
    <div class="vh-100 d-flex justify-content-center align-items-center">
      <div class="spinner-border" role="status"></div>
    </div>
  </template>
  <template v-else-if="auth.isAuthenticated">
    <header class="navbar navbar-expand-lg bd-navbar sticky-top bg-dark">
      <nav class="container-xxl bd-gutter flex-wrap flex-lg-nowrap" aria-label="Main navigation">
        <div class="d-lg-none" style="width: 1.5rem"></div>

        <RouterLink class="navbar-brand p-0 me-0 me-lg-2" to="/"><b>poshtar</b></RouterLink>

        <div class="d-flex">
          <button
            class="navbar-toggler d-flex d-lg-none order-3 p-2"
            type="button"
            data-bs-toggle="offcanvas"
            data-bs-target="#appNavbar"
            aria-controls="appNavbar"
            aria-label="Toggle navigation"
          >
            <ThreeDotsVerticalIcon />
          </button>
        </div>

        <div
          class="offcanvas-lg offcanvas-end flex-grow-1"
          tabindex="-1"
          id="appNavbar"
          aria-labelledby="appNavbarOffcanvasLabel"
          data-bs-scroll="true"
        >
          <div class="offcanvas-header px-4 pb-0">
            <h5 class="offcanvas-title text-white" id="appNavbarOffcanvasLabel">poshtar</h5>
            <button
              type="button"
              class="btn-close btn-close-white"
              data-bs-dismiss="offcanvas"
              aria-label="Close"
              data-bs-target="#appNavbar"
            ></button>
          </div>

          <div class="offcanvas-body p-4 pt-0 p-lg-0">
            <hr class="d-lg-none text-white-50" />
            <ul class="navbar-nav flex-row flex-wrap">
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.users' }"
                  >Users</RouterLink
                >
              </li>
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.calendars' }"
                  >Calendars</RouterLink
                >
              </li>
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.domains' }"
                  >Domains</RouterLink
                >
              </li>
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.antispam' }"
                  >Antispam</RouterLink
                >
              </li>
            </ul>
            <hr class="d-lg-none text-white-50" />
            <ul class="navbar-nav flex-row flex-wrap ms-md-auto">
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <a class="nav-link py-2 px-0 px-lg-2" href="/jobs" title="Jobs">
                  <CalendarRangeFillIcon />
                  <small class="d-lg-none ms-2">Jobs</small>
                </a>
              </li>
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <DovecotIndicator />
              </li>
              <li
                v-if="!auth.hasOtp"
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
              >
                <TotpIndicator />
              </li>
              <li class="nav-item py-2 py-lg-1 col-12 col-lg-auto">
                <div class="vr d-none d-lg-flex h-100 mx-lg-2 text-white"></div>
                <hr class="d-lg-none my-2 text-white-50" />
              </li>
              <li
                class="nav-item col-6 col-lg-auto"
                data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas"
                title="Sign out"
              >
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.logout' }">
                  <span class="me-2">{{ auth.username }}</span>
                  <BoxArrowRightIcon />
                  <small class="d-lg-none ms-2">Sign out</small>
                </RouterLink>
              </li>
            </ul>
          </div>
        </div>
      </nav>
    </header>
    <div class="container mt-4">
      <RouterView v-slot="{ Component }">
        <Transition name="fade" mode="out-in">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </div>
  </template>
  <template v-else>
    <div class="container-fluid">
      <RouterView />
    </div>
  </template>
</template>
<style>
.bd-navbar {
  padding: 0.75rem 0;
  background-color: transparent;
  box-shadow:
    0 0.5rem 1rem rgba(0, 0, 0, 0.15),
    inset 0 -1px 0 rgba(255, 255, 255, 0.15);
}

.bd-gutter {
  --bs-gutter-x: 3rem;
}
</style>
