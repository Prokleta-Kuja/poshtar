<script setup lang="ts">
import { reactive, ref } from 'vue';
import { UserService, type UserVM, type PlainError, type DomainLM, DomainService } from '@/api';
import UserAddressList from '@/lists/UserAddressList.vue';
import Modal from '@/components/Modal.vue';
import AddUserAddress from '@/modals/AddUserAddress.vue'
import EditUser from '@/modals/EditUser.vue';

const props = defineProps<{ id: number }>()
const addressChange = ref(new Date)
const user = reactive<{ error?: PlainError, value?: UserVM }>({});
const domains = ref<DomainLM[]>([])
const shownLink = ref(false)

const toggleLink = () => {
    shownLink.value = !shownLink.value;
    if (!shownLink.value)
        addressChange.value = new Date;
}
const addressChanged = () => addressChange.value = new Date;
const updateUser = (updatedUser: UserVM) => user.value = updatedUser;

UserService.getUser({ userId: props.id })
    .then(r => user.value = r)
    .catch(r => user.error = r.body);

DomainService.getDomains({ size: 100 }).then(r => domains.value = r.items)
</script>
<template>
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
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-link-45deg"
                    viewBox="0 0 16 16">
                    <path
                        d="M4.715 6.542 3.343 7.914a3 3 0 1 0 4.243 4.243l1.828-1.829A3 3 0 0 0 8.586 5.5L8 6.086a1.002 1.002 0 0 0-.154.199 2 2 0 0 1 .861 3.337L6.88 11.45a2 2 0 1 1-2.83-2.83l.793-.792a4.018 4.018 0 0 1-.128-1.287z" />
                    <path
                        d="M6.586 4.672A3 3 0 0 0 7.414 9.5l.775-.776a2 2 0 0 1-.896-3.346L9.12 3.55a2 2 0 1 1 2.83 2.83l-.793.792c.112.42.155.855.128 1.287l1.372-1.372a3 3 0 1 0-4.243-4.243L6.586 4.672z" />
                </svg>
                Address
            </button>
            <AddUserAddress :userId="props.id" :on-added="addressChanged" />
        </template>
    </div>
    <template v-if="user.value">
        <UserAddressList :user-id="props.id" :last-change="addressChange" />
        <Modal title="Link address to user" width="lg" :shown="shownLink" :onClose="toggleLink">
            <template #body>
                <UserAddressList v-if="shownLink" :not-user-id="user.value.id" />
            </template>
            <template #footer>
                <button class="btn btn-outline-danger" @click="toggleLink">Close</button>
            </template>
        </Modal>
    </template>
    <p v-else-if="user.error" class="text-danger">{{ user.error.message }}</p>
    <p v-else>Loading...</p></template>
