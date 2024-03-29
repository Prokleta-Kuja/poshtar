<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type RelayCM, RelayService, type RelayVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import GeneralModal from '@/components/GeneralModal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
import Text from '@/components/form/TextBox.vue'
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'

export interface IAddRelay {
  onAdded?: (added: RelayVM) => void
}

const props = defineProps<IAddRelay>()
const blank = (): RelayCM => ({ name: '', host: '', port: 587, username: '', password: '' })
const shown = ref(false)
const relay = reactive<IModelState<RelayCM>>({ model: blank() })

const toggle = () => (shown.value = !shown.value)
const submit = () => {
  relay.submitting = true
  relay.error = undefined
  RelayService.createRelay({ requestBody: relay.model })
    .then((r) => {
      shown.value = false
      relay.model = blank()
      if (props.onAdded) props.onAdded(r)
    })
    .catch((r) => (relay.error = r.body))
    .finally(() => (relay.submitting = false))
}
</script>
<template>
  <button class="btn btn-success me-3" @click="toggle">
    <PlusLgIcon />
    Relay
  </button>
  <GeneralModal title="Add relay" :shown="shown" :onClose="toggle">
    <template #body>
      <form @submit.prevent="submit">
        <Text
          class="mb-3"
          label="Relay name"
          autoFocus
          v-model="relay.model.name"
          required
          :error="relay.error?.errors?.name"
        />
        <Text
          class="mb-3"
          label="Host"
          v-model="relay.model.host"
          required
          :error="relay.error?.errors?.host"
        />
        <IntegerBox
          class="mb-3"
          label="Port"
          v-model="relay.model.port"
          required
          :error="relay.error?.errors?.port"
        />
        <Text
          class="mb-3"
          label="Username"
          v-model="relay.model.username"
          required
          :error="relay.error?.errors?.username"
        />
        <Text
          class="mb-3"
          label="Password"
          :autoComplete="'off'"
          :type="'password'"
          v-model="relay.model.password"
          required
          :error="relay.error?.errors?.password"
        />
      </form>
    </template>
    <template #footer>
      <p v-if="relay.error" class="text-danger">{{ relay.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="relay.submitting"
        text="Add"
        loadingText="Adding"
        @click="submit"
      />
    </template>
  </GeneralModal>
</template>
