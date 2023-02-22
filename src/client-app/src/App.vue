<script setup lang="ts">

</script>
<template>
  <header class="navbar navbar-expand-lg bd-navbar sticky-top">
    <nav class="container-xxl bd-gutter flex-wrap flex-lg-nowrap" aria-label="Main navigation">
      <div class="d-lg-none" style="width: 1.5rem;"></div>

      <RouterLink class="navbar-brand p-0 me-0 me-lg-2" to="/"><b>poshtar</b></RouterLink>

      <div class="d-flex">
        <button class="navbar-toggler d-flex d-lg-none order-3 p-2" type="button" data-bs-toggle="offcanvas"
          data-bs-target="#bdNavbar" aria-controls="bdNavbar" aria-label="Toggle navigation">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
            class="bi bi-three-dots-vertical" viewBox="0 0 16 16">
            <path
              d="M9.5 13a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0z" />
          </svg>
        </button>
      </div>

      <div class="offcanvas-lg offcanvas-end flex-grow-1" tabindex="-1" id="bdNavbar"
        aria-labelledby="bdNavbarOffcanvasLabel" data-bs-scroll="true">
        <div class="offcanvas-header px-4 pb-0">
          <h5 class="offcanvas-title text-white" id="bdNavbarOffcanvasLabel">poshtar</h5>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="offcanvas" aria-label="Close"
            data-bs-target="#bdNavbar"></button>
        </div>

        <div class="offcanvas-body p-4 pt-0 p-lg-0">
          <hr class="d-lg-none text-white-50">
          <ul class="navbar-nav flex-row flex-wrap bd-navbar-nav">
            <li class="nav-item col-6 col-lg-auto">
              <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.domainList' }">Domains</RouterLink>
            </li>
            <li class="nav-item col-6 col-lg-auto">
              <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.users' }">Users</RouterLink>
            </li>
          </ul>
        </div>
      </div>
    </nav>
  </header>
  <div class="container">
    <RouterView />
  </div>
</template>