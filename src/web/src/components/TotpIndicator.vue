<script setup lang="ts">
import { ref, reactive } from 'vue'
import GeneralModal from '@/components/GeneralModal.vue'
import type IModelState from './form/modelState'
import { AuthService, type TotpCM, type TotpVM } from '@/api'
import IntegerBox from './form/IntegerBox.vue'
import SpinButton from './form/SpinButton.vue'
import { useAuth } from '@/stores/auth.store'
import QrCodeIcon from './icons/QrCodeIcon.vue'
import CircleFillIcon from './icons/CircleFillIcon.vue'

const auth = useAuth()
const shown = ref(false)
const view = reactive<IModelState<TotpVM>>({ model: { chunkedSecret: '', qr: '' } })
const create = reactive<IModelState<TotpCM>>({ model: { chunkedSecret: '' } })
const hide = () => (shown.value = false)
const show = () => {
  view.loading = true
  view.error = undefined
  AuthService.getTotp()
    .then((r) => {
      view.model.qr = r.qr
      view.model.chunkedSecret = r.chunkedSecret
      create.model.chunkedSecret = r.chunkedSecret
      view.loading = false
    })
    .catch((r) => (view.error = r.body))
    .finally(() => (shown.value = true))
}
const copySecret = () => navigator.clipboard.writeText(view.model.chunkedSecret)
const submit = () => {
  create.submitting = true
  create.error = undefined
  AuthService.saveTotp({ requestBody: create.model })
    .then(() => {
      auth.hasOtp = true
      hide()
    })
    .catch((r) => (create.error = r.body))
    .finally(() => (create.submitting = false))
}
</script>
<template>
  <div class="nav-link py-2 px-0 px-lg-2 pointer" title="Add TOTP" @click="show">
    <QrCodeIcon :class="'text-danger'" />
    <small class="d-lg-none ms-2">Add TOTP</small>
  </div>
  <GeneralModal title="Time-based One-Time Password" :onClose="hide" :shown="shown">
    <template #body>
      <template v-if="view.loading">
        <div class="text-center mt-5">
          <div class="spinner-border" role="status"></div>
        </div>
      </template>
      <span v-else-if="view.error" class="text-danger">{{ view.error }}</span>
      <template v-else>
        <span
          >Scan the following QR code with authenticator app or use the secret bellow to add to your
          password manager.</span
        >
        <img class="rounded mx-auto d-block mt-3" :src="view.model.qr" />

        <label class="form-label">Secret code</label>
        <div class="input-group mb-3">
          <input type="text" class="form-control" :value="create.model.chunkedSecret" readonly />
          <button
            class="btn btn-outline-secondary"
            type="button"
            title="Copy to clipboard"
            @click="copySecret"
          >
            <CircleFillIcon />
          </button>
        </div>
        <form @submit.prevent="submit">
          <IntegerBox
            class="mb-3"
            label="One Time Code"
            autoComplete="one-time-code"
            v-model="create.model.code"
            required
            help="Enter the code from authenticator app for the added secret"
            :error="create.error?.errors?.code"
          />
        </form>
      </template>
    </template>
    <template #footer>
      <p v-if="create.error" class="text-danger">{{ create.error.message }}</p>
      <button class="btn btn-outline-danger" @click="hide">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="create.submitting"
        text="Save"
        loadingText="Saving"
        @click="submit"
      />
    </template>
  </GeneralModal>
</template>
