<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type UserCM, UserService } from '@/api'
import type IModelState from '@/components/form/modelState'
import GeneralModal from '@/components/GeneralModal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
import Text from '@/components/form/TextBox.vue'
import CheckBox from '@/components/form/CheckBox.vue'
import { useRouter } from 'vue-router'
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'

const router = useRouter()
const blank = (): UserCM => ({ name: '', isMaster: false, password: '' })
const shown = ref(false)
const user = reactive<IModelState<UserCM>>({ model: blank() })

const toggle = () => (shown.value = !shown.value)
const submit = () => {
  user.submitting = true
  user.error = undefined
  UserService.createUser({ requestBody: user.model })
    .then((r) => router.push({ name: 'route.userDetails', params: { id: r.id } }))
    .catch((r) => (user.error = r.body))
    .finally(() => (user.submitting = false))
}
</script>
<template>
  <button class="btn btn-success" @click="toggle">
    <PlusLgIcon />
    Add
  </button>
  <GeneralModal title="Add user" :shown="shown" :onClose="toggle">
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
          label="Password"
          :autoComplete="'off'"
          :type="'password'"
          v-model="user.model.password"
          :error="user.error?.errors?.password"
          required
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
      </form>
    </template>
    <template #footer>
      <p v-if="user.error" class="text-danger">{{ user.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
      <SpinButton
        class="btn-primary"
        :loading="user.submitting"
        text="Add"
        loadingText="Adding"
        @click="submit"
      />
    </template>
  </GeneralModal>
</template>
