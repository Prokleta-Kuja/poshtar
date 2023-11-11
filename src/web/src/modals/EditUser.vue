<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type UserUM, UserService, type UserVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import GeneralModal from '@/components/GeneralModal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
import Text from '@/components/form/TextBox.vue'
import CheckBox from '@/components/form/CheckBox.vue'
import PencilSquareIcon from '@/components/icons/PencilSquareIcon.vue'
export interface IEditUser {
  model: UserVM
  onUpdated?: (updatedDomain: UserVM) => void
}

const mapUserModel = (m: UserVM): UserUM => ({
  name: m.name,
  isMaster: m.isMaster,
  description: m.description,
  quota: m.quotaMegaBytes,
  disabled: m.disabled ? true : false
})
const props = defineProps<IEditUser>()
const shown = ref(false)
const user = reactive<IModelState<UserUM>>({ model: mapUserModel(props.model) })

const toggle = () => (shown.value = !shown.value)
const submit = () => {
  user.submitting = true
  user.error = undefined
  UserService.updateUser({ userId: props.model.id, requestBody: user.model })
    .then((r) => {
      shown.value = false
      if (props.onUpdated) props.onUpdated(r)
    })
    .catch((r) => (user.error = r.body))
    .finally(() => (user.submitting = false))
}
</script>
<template>
  <button class="btn btn-primary me-3" @click="toggle">
    <PencilSquareIcon />
    User
  </button>
  <GeneralModal v-if="user.model" title="Edit user" :shown="shown" :onClose="toggle">
    <template #body>
      <form @submit.prevent="submit">
        <Text
          class="mb-3"
          label="Username"
          autoFocus
          v-model="user.model.name"
          required
          :error="user.error?.errors?.name"
        />
        <Text
          class="mb-3"
          label="Description"
          v-model="user.model.description"
          :error="user.error?.errors?.description"
        />
        <Text
          class="mb-3"
          label="Replace password"
          :autoComplete="'off'"
          :type="'password'"
          v-model="user.model.newPassword"
          :error="user.error?.errors?.newPassword"
        />
        <IntegerBox
          class="mb-3"
          label="Quota in MB"
          v-model="user.model.quota"
          :error="user.error?.errors?.quota"
        />
        <CheckBox
          class="mb-3"
          label="Master"
          v-model="user.model.isMaster"
          :error="user.error?.errors?.isMaster"
        />
        <CheckBox
          class="mb-3"
          label="Disabled"
          v-model="user.model.disabled"
          :error="user.error?.errors?.disabled"
        />
        <CheckBox
          class="mb-3"
          label="Clear One Time Code Key"
          v-model="user.model.clearOtpKey"
          :error="user.error?.errors?.clearOtpKey"
        />
      </form>
    </template>
    <template #footer>
      <p v-if="user.error" class="text-danger">{{ user.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="user.submitting"
        text="Save"
        loadingText="Saving"
        @click="submit"
      />
    </template>
  </GeneralModal>
</template>
