<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type AddressCM, AddressService, AddressType, DomainService } from '@/api'
import type IModelState from '@/components/form/modelState'
import GeneralModal from '@/components/GeneralModal.vue'
import SelectBox from '@/components/form/SelectBox.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import Text from '@/components/form/TextBox.vue'
import TypeaheadBox, { type KV } from '@/components/form/TypeaheadBox.vue'
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'

const props = defineProps<{ userId: number; onAdded?: () => void }>()
const shown = ref(false)
const address = reactive<IModelState<AddressCM>>({
  model: { domainId: 0, pattern: '', type: AddressType.Exact }
})

const addressTypes: { value: number; label: string }[] = [
  { value: AddressType.Exact, label: 'Exact' },
  { value: AddressType.Prefix, label: 'Prefix' },
  { value: AddressType.Suffix, label: 'Suffix' },
  { value: AddressType.CatchAll, label: 'CatchAll' }
]
const toggle = () => (shown.value = !shown.value)
const searchDomain = async (term: string): Promise<KV[]> => {
  const response = await DomainService.getDomains({
    size: 5,
    sortBy: 'name',
    ascending: true,
    searchTerm: term
  })
  return response.items.map((i) => ({ value: i.id, label: i.name }))
}
const submit = () => {
  if (!address.model) return

  address.submitting = true
  address.error = undefined
  if (address.model.type === AddressType.CatchAll) address.model.pattern = '*'
  AddressService.createAddress({ requestBody: address.model })
    .then((r) => {
      address.model.description = undefined
      address.model.pattern = ''
      link(r.id)
    })
    .catch((r) => {
      address.error = r.body
      address.submitting = false
    })
}
const link = (addressId: number) => {
  AddressService.addAddressUser({ addressId: addressId, userId: props.userId })
    .then(() => {
      if (props.onAdded) props.onAdded()
      shown.value = false
    })
    .catch((r) => (address.error = r.body))
    .finally(() => (address.submitting = false))
}
</script>
<template>
  <button class="btn btn-success" @click="toggle">
    <PlusLgIcon />
    Address
  </button>
  <GeneralModal title="Add user address" :shown="shown" :onClose="toggle">
    <template #body>
      <form @submit.prevent="submit">
        <TypeaheadBox
          class="mb-3"
          label="Domain"
          v-model="address.model.domainId"
          :error="address.error?.errors?.domainId"
          :onSearch="searchDomain"
          required
          autoFocus
        />
        <SelectBox
          class="mb-3"
          label="Type"
          v-model="address.model.type"
          :error="address.error?.errors?.type"
          :options="addressTypes"
          required
        />
        <Text
          v-if="address.model.type !== AddressType.CatchAll"
          class="mb-3"
          label="Pattern"
          v-model="address.model.pattern"
          required
          :error="address.error?.errors?.pattern"
        />
        <Text
          class="mb-3"
          label="Description"
          v-model="address.model.description"
          :error="address.error?.errors?.description"
        />
      </form>
    </template>
    <template #footer>
      <p v-if="address.error" class="text-danger">{{ address.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="address.submitting"
        text="Add"
        loadingText="Adding"
        @click="submit"
      />
    </template>
  </GeneralModal>
</template>
