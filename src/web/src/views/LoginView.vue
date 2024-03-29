<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { AuthService, type LoginModel } from '@/api'
import type IModelState from '@/components/form/modelState'
import TextBox from '@/components/form/TextBox.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import { useAuth } from '@/stores/auth.store'
import IntegerBox from '@/components/form/IntegerBox.vue'

const auth = useAuth()
const router = useRouter()
const loading = ref(true)
const state = reactive<IModelState<LoginModel>>({ model: { username: '', password: '' } })

const goToRoot = () => router.replace({ name: 'route.root' })
const login = () => {
  state.submitting = true
  AuthService.login({ requestBody: state.model })
    .then((r) => {
      auth.setLoginInfo(r), goToRoot()
    })
    .catch((r) => (state.error = r.body))
    .finally(() => (state.submitting = false))
}

if (!auth.isAuthenticated)
  AuthService.autoLogin()
    .then((r) => {
      auth.setLoginInfo(r), goToRoot()
    })
    .catch(() => {})
    .finally(() => (loading.value = false))
else goToRoot()
</script>
<template>
  <div class="row vh-100 g-0">
    <div class="col-lg-8 position-relative d-none d-lg-block">
      <div class="bg-holder"></div>
    </div>
    <div class="col-lg-4">
      <div class="row d-flex align-items-center justify-content-center h-100 g-0 px-4 px-sm-0">
        <div class="col col-sm-6 col-lg-8 col-xl-8">
          <div class="d-flex justify-content-center">
            <img src="/android-chrome-192x192.png" alt="phoenix" width="58" />
          </div>
          <template v-if="loading">
            <div class="text-center mt-5">
              <div class="spinner-border" role="status"></div>
            </div>
          </template>
          <template v-else>
            <div class="text-center mb-7">
              <h3 class="text-1000">Sign In</h3>
              <p class="text-700">Self-hosted email server</p>
            </div>
            <form @submit.prevent="login">
              <TextBox
                class="mb-3"
                label="Username"
                autoComplete="username"
                v-model="state.model.username"
                required
                name="username"
                autoFocus
              />
              <TextBox
                class="mb-3"
                label="Password"
                type="password"
                autoComplete="password"
                v-model="state.model.password"
                name="password"
                required
              />
              <IntegerBox
                class="mb-3"
                label="One Time Code"
                autoComplete="one-time-code"
                v-model="state.model.totp"
                name="totp"
              />
              <p v-if="state.error" class="text-danger">{{ state.error.message }}</p>
              <SpinButton
                class="btn-primary w-100 mb-3"
                :loading="state.submitting"
                text="Sign in"
                loadingText="Signing in"
                isSubmit
              />
            </form>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>
<style>
.bg-holder {
  background-image: url(/login.png);
  position: absolute;
  width: 100%;
  min-height: 100%;
  top: 0;
  left: 0;
  background-size: cover;
  background-position: center;
  overflow: hidden;
  will-change: transform, opacity, filter;
  backface-visibility: hidden;
  background-repeat: no-repeat;
  z-index: 0;
}
</style>
