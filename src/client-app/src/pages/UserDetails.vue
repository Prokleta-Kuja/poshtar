<script setup lang="ts">
import { reactive, ref } from 'vue';
import { UserService, UserVM, PlainError, DomainLM, DomainService } from '../api';
import UserAddressList from '../lists/UserAddressList.vue';
import Modal from '../components/Modal.vue';
import EditUser from '../modals/EditUser.vue';

const props = defineProps<{ id: number }>()
const addressChange = ref(new Date)
const user = reactive<{ error?: PlainError, value?: UserVM }>({});
const domains = ref<DomainLM[]>([])
const shown = ref(false)

const toggle = () => {
    shown.value = !shown.value;
    if (!shown.value)
        addressChange.value = new Date;
}
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
            <button class="btn btn-success" @click="toggle">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-lg"
                    viewBox="0 0 16 16">
                    <path fill-rule="evenodd"
                        d="M8 2a.5.5 0 0 1 .5.5v5h5a.5.5 0 0 1 0 1h-5v5a.5.5 0 0 1-1 0v-5h-5a.5.5 0 0 1 0-1h5v-5A.5.5 0 0 1 8 2Z" />
                </svg>
                Address
            </button>
        </template>
    </div>
    <template v-if="user.value">
        <UserAddressList :user-id="props.id" :last-change="addressChange" />
        <Modal title="Add user address" width="lg" :shown="shown" :onClose="toggle">
            <template #body>
                <UserAddressList v-if="shown" :not-user-id="user.value.id" />
            </template>
            <template #footer>
                <button class="btn btn-outline-danger" @click="toggle">Close</button>
            </template>
        </Modal>
    </template>
    <p v-else-if="user.error" class="text-danger">{{ user.error.message }}</p>
    <p v-else>Loading...</p>
</template>
