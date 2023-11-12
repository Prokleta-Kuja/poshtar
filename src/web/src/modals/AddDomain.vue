<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type DomainCM, DomainService, type DomainVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import GeneralModal from '@/components/GeneralModal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import Text from '@/components/form/TextBox.vue'
import SelectBox from '@/components/form/SelectBox.vue'
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'

export interface IAddDomain {
  relays: { value: number; label: string }[]
  onAdded?: (added: DomainVM) => void
}

const props = defineProps<IAddDomain>()
const blank = (): DomainCM => ({ name: '' })
const shown = ref(false)
const domain = reactive<IModelState<DomainCM>>({ model: blank() })

const toggle = () => (shown.value = !shown.value)
const submit = () => {
  domain.submitting = true
  domain.error = undefined
  DomainService.createDomain({ requestBody: domain.model })
    .then((r) => {
      shown.value = false
      domain.model = blank()
      if (props.onAdded) props.onAdded(r)
    })
    .catch((r) => (domain.error = r.body))
    .finally(() => (domain.submitting = false))
}
</script>
<template>
  <button class="btn btn-success me-3" @click="toggle">
    <PlusLgIcon />
    Domain
  </button>
  <GeneralModal title="Add domain" :shown="shown" :onClose="toggle">
    <template #body>
      <form @submit.prevent="submit">
        <Text
          class="mb-3"
          label="Domain"
          autoFocus
          v-model="domain.model.name"
          required
          :error="domain.error?.errors?.name"
        />
        <SelectBox
          class="mb-3"
          label="Relay"
          v-model="domain.model.relayId"
          :error="domain.error?.errors?.relayId"
          :options="props.relays"
          undefined-label="No relay. Local routing only."
        />
      </form>
    </template>
    <template #footer>
      <p v-if="domain.error" class="text-danger">{{ domain.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="domain.submitting"
        text="Add"
        loadingText="Adding"
        @click="submit"
      />
    </template>
  </GeneralModal>
</template>
