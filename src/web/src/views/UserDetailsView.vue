<script setup lang="ts">
import { reactive, ref } from 'vue'
import { UserService, type UserVM, type PlainError, type DomainLM, DomainService } from '@/api'
import UserAddressList from '@/lists/UserAddressList.vue'
import GeneralModal from '@/components/GeneralModal.vue'
import AddUserAddress from '@/modals/AddUserAddress.vue'
import EditUser from '@/modals/EditUser.vue'
import Link45Icon from '@/components/icons/Link45Icon.vue'

const props = defineProps<{ id: number }>()
const addressChange = ref(new Date())
const user = reactive<{ error?: PlainError; value?: UserVM }>({})
const domains = ref<DomainLM[]>([])
const shownLink = ref(false)

const toggleLink = () => {
  shownLink.value = !shownLink.value
  if (!shownLink.value) addressChange.value = new Date()
}
const addressChanged = () => (addressChange.value = new Date())
const updateUser = (updatedUser: UserVM) => (user.value = updatedUser)

UserService.getUser({ userId: props.id })
  .then((r) => (user.value = r))
  .catch((r) => (user.error = r.body))

DomainService.getDomains({ size: 100 }).then((r) => (domains.value = r.items))
</script>
<template>
  <main>
    <div class="d-flex align-items-center flex-wrap">
      <h1 class="display-6 me-3">
        <span v-if="!user.value">User</span>
        <span v-else>{{ user.value.name }}</span>
        details
      </h1>
      <button class="btn btn-sm btn-secondary me-3" @click="$router.back()">Back</button>
      <template v-if="user.value">
        <EditUser :model="user.value" @updated="updateUser" />
        <button class="btn btn-warning me-3" @click="toggleLink">
          <Link45Icon />
          Address
        </button>
        <AddUserAddress :userId="props.id" :on-added="addressChanged" />
      </template>
    </div>
    <template v-if="user.value">
      <UserAddressList :user-id="props.id" :last-change="addressChange" />
      <GeneralModal
        title="Link address to user"
        width="lg"
        :shown="shownLink"
        :onClose="toggleLink"
      >
        <template #body>
          <UserAddressList v-if="shownLink" :not-user-id="user.value.id" />
        </template>
        <template #footer>
          <button class="btn btn-outline-danger" @click="toggleLink">Close</button>
        </template>
      </GeneralModal>
    </template>
    <p v-else-if="user.error" class="text-danger">{{ user.error.message }}</p>
    <p v-else>Loading...</p>
  </main>
</template>
