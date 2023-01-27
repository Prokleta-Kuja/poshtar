<script setup lang="ts">

</script>
<template>
  <header class="navbar navbar-expand-lg bd-navbar sticky-top">
    <nav class="container-xxl bd-gutter flex-wrap flex-lg-nowrap" aria-label="Main navigation">
      <div class="d-lg-none" style="width: 1.5rem;"></div>

      <router-link class="navbar-brand p-0 me-0 me-lg-2" to="/"><b>poshtar</b></router-link>

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
              <router-link class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.domains' }">Domains</router-link>
            </li>
            <li class="nav-item col-6 col-lg-auto">
              <router-link class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'route.users' }">Users</router-link>
            </li>
          </ul>

          <!-- <hr class="d-lg-none text-white-50">

          <ul class="navbar-nav flex-row flex-wrap ms-md-auto">
            <li class="nav-item col-6 col-lg-auto">
              <a class="nav-link py-2 px-0 px-lg-2" href="https://github.com/twbs" target="_blank" rel="noopener">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" class="navbar-nav-svg"
                  viewBox="0 0 512 499.36" role="img">
                  <title>GitHub</title>
                  <path fill="currentColor" fill-rule="evenodd"
                    d="M256 0C114.64 0 0 114.61 0 256c0 113.09 73.34 209 175.08 242.9 12.8 2.35 17.47-5.56 17.47-12.34 0-6.08-.22-22.18-.35-43.54-71.2 15.49-86.2-34.34-86.2-34.34-11.64-29.57-28.42-37.45-28.42-37.45-23.27-15.84 1.73-15.55 1.73-15.55 25.69 1.81 39.21 26.38 39.21 26.38 22.84 39.12 59.92 27.82 74.5 21.27 2.33-16.54 8.94-27.82 16.25-34.22-56.84-6.43-116.6-28.43-116.6-126.49 0-27.95 10-50.8 26.35-68.69-2.63-6.48-11.42-32.5 2.51-67.75 0 0 21.49-6.88 70.4 26.24a242.65 242.65 0 0 1 128.18 0c48.87-33.13 70.33-26.24 70.33-26.24 14 35.25 5.18 61.27 2.55 67.75 16.41 17.9 26.31 40.75 26.31 68.69 0 98.35-59.85 120-116.88 126.32 9.19 7.9 17.38 23.53 17.38 47.41 0 34.22-.31 61.83-.31 70.23 0 6.85 4.61 14.81 17.6 12.31C438.72 464.97 512 369.08 512 256.02 512 114.62 397.37 0 256 0z">
                  </path>
                </svg>
                <small class="d-lg-none ms-2">GitHub</small>
              </a>
            </li>
            <li class="nav-item py-2 py-lg-1 col-12 col-lg-auto">
              <div class="vr d-none d-lg-flex h-100 mx-lg-2 text-white"></div>
              <hr class="d-lg-none my-2 text-white-50">
            </li>
            <li class="nav-item col-6 col-lg-auto">
              <a class="nav-link py-2 px-0 px-lg-2" href="https://twitter.com/getbootstrap" target="_blank"
                rel="noopener">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                  class="bi bi-whatsapp text-success" viewBox="0 0 16 16" aria-label="Wapp">
                  <path
                    d="M13.601 2.326A7.854 7.854 0 0 0 7.994 0C3.627 0 .068 3.558.064 7.926c0 1.399.366 2.76 1.057 3.965L0 16l4.204-1.102a7.933 7.933 0 0 0 3.79.965h.004c4.368 0 7.926-3.558 7.93-7.93A7.898 7.898 0 0 0 13.6 2.326zM7.994 14.521a6.573 6.573 0 0 1-3.356-.92l-.24-.144-2.494.654.666-2.433-.156-.251a6.56 6.56 0 0 1-1.007-3.505c0-3.626 2.957-6.584 6.591-6.584a6.56 6.56 0 0 1 4.66 1.931 6.557 6.557 0 0 1 1.928 4.66c-.004 3.639-2.961 6.592-6.592 6.592zm3.615-4.934c-.197-.099-1.17-.578-1.353-.646-.182-.065-.315-.099-.445.099-.133.197-.513.646-.627.775-.114.133-.232.148-.43.05-.197-.1-.836-.308-1.592-.985-.59-.525-.985-1.175-1.103-1.372-.114-.198-.011-.304.088-.403.087-.088.197-.232.296-.346.1-.114.133-.198.198-.33.065-.134.034-.248-.015-.347-.05-.099-.445-1.076-.612-1.47-.16-.389-.323-.335-.445-.34-.114-.007-.247-.007-.38-.007a.729.729 0 0 0-.529.247c-.182.198-.691.677-.691 1.654 0 .977.71 1.916.81 2.049.098.133 1.394 2.132 3.383 2.992.47.205.84.326 1.129.418.475.152.904.129 1.246.08.38-.058 1.171-.48 1.338-.943.164-.464.164-.86.114-.943-.049-.084-.182-.133-.38-.232z" />
                </svg>
                <small class="d-lg-none ms-2">Wapp</small>
              </a>
            </li>
          </ul> -->
        </div>
      </div>
    </nav>
  </header>
  <div class="container">
    <router-view />
  </div>
</template>