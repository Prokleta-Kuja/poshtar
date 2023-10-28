<script setup lang="ts">
import { reactive } from 'vue'
import { type RelayUM, RelayService, type RelayVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import Modal from '@/components/Modal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
import Text from '@/components/form/TextBox.vue'
import CheckBox from '@/components/form/CheckBox.vue'
export interface IEditRelay {
  model: RelayVM
  onUpdated: (updatedRelay?: RelayVM) => void
  shown?: boolean
}

const mapRelayModel = (m: RelayVM): RelayUM => ({
  name: m.name,
  host: m.host,
  username: m.username,
  port: m.port,
  disabled: m.disabled ? true : false
})
const props = defineProps<IEditRelay>()
const relay = reactive<IModelState<RelayUM>>({ model: mapRelayModel(props.model) })

const close = () => props.onUpdated()
const submit = () => {
  relay.submitting = true
  relay.error = undefined
  RelayService.updateRelay({ relayId: props.model.id, requestBody: relay.model })
    .then((r) => props.onUpdated(r))
    .catch((r) => (relay.error = r.body))
    .finally(() => (relay.submitting = false))
}
</script>
<template>
  <Modal v-if="relay.model" title="Edit relay" shown :onClose="close">
    <template #body>
      <form @submit.prevent="submit">
        <Text
          class="mb-3"
          label="Relay"
          autoFocus
          v-model="relay.model.name"
          required
          :error="relay.error?.errors?.name"
        />
        <CheckBox
          class="mb-3"
          label="Disabled"
          v-model="relay.model.disabled"
          :error="relay.error?.errors?.disabled"
        />
        <Text
          class="mb-3"
          label="Host"
          :placeholder="'smtp.example.com'"
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
          :autoComplete="'off'"
          v-model="relay.model.username"
          required
          :error="relay.error?.errors?.username"
        />
        <Text
          class="mb-3"
          label="Replace password"
          :autoComplete="'off'"
          :type="'password'"
          v-model="relay.model.newPassword"
          :error="relay.error?.errors?.newPassword"
        />
      </form>
    </template>
    <template #footer>
      <p v-if="relay.error" class="text-danger">{{ relay.error.message }}</p>
      <button class="btn btn-outline-danger" @click="close">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="relay.submitting"
        text="Save"
        loadingText="Saving"
        @click="submit"
      />
    </template>
  </Modal>
</template>
