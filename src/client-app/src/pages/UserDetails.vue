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
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-link"
                    viewBox="0 0 16 16">
                    <path
                        d="M6.354 5.5H4a3 3 0 0 0 0 6h3a3 3 0 0 0 2.83-4H9c-.086 0-.17.01-.25.031A2 2 0 0 1 7 10.5H4a2 2 0 1 1 0-4h1.535c.218-.376.495-.714.82-1z" />
                    <path
                        d="M9 5.5a3 3 0 0 0-2.83 4h1.098A2 2 0 0 1 9 6.5h3a2 2 0 1 1 0 4h-1.535a4.02 4.02 0 0 1-.82 1H12a3 3 0 1 0 0-6H9z" />
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
    <p v-else>Loading...</p>
</template>
