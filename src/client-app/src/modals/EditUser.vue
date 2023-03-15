<script setup lang="ts">
import { reactive, ref } from 'vue';
import { UserUM, UserService, UserVM } from '../api';
import IModelState from '../components/form/modelState';
import Modal from '../components/Modal.vue';
import SpinButton from '../components/form/SpinButton.vue';
import IntegerBox from '../components/form/IntegerBox.vue';
import Text from '../components/form/TextBox.vue';
import CheckBox from '../components/form/CheckBox.vue';
export interface IEditUser {
    model: UserVM,
    onUpdated?: (updatedDomain: UserVM) => void
}

const mapUserModel = (m: UserVM): UserUM =>
({
    name: m.name,
    isMaster: m.isMaster,
    description: m.description,
    quota: m.quotaMegaBytes,
    disabled: m.disabled ? true : false,
})
const props = defineProps<IEditUser>()
const shown = ref(false)
const user = reactive<IModelState<UserUM>>({ model: mapUserModel(props.model) })

const toggle = () => shown.value = !shown.value
const submit = () => {
    user.submitting = true;
    user.error = undefined;
    UserService.updateUser({ userId: props.model.id, requestBody: user.model })
        .then(r => {
            shown.value = false;
            if (props.onUpdated)
                props.onUpdated(r);
        })
        .catch(r => user.error = r.body)
        .finally(() => user.submitting = false);
};
</script>
<template>
    <button class="btn btn-primary me-3" @click="toggle">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pencil-square"
            viewBox="0 0 16 16">
            <path
                d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z" />
            <path fill-rule="evenodd"
                d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z" />
        </svg>
        User
    </button>
    <Modal v-if="user.model" title="Edit user" :shown="shown" :onClose="toggle">
        <template #body>
            <form @submit.prevent="submit">
                <Text class="mb-3" label="Username" autoFocus v-model="user.model.name" required
                    :error="user.error?.errors?.name" />
                <Text class="mb-3" label="Description" v-model="user.model.description"
                    :error="user.error?.errors?.description" />
                <Text class="mb-3" label="Replace password" :autoComplete="'off'" :type="'password'"
                    v-model="user.model.newPassword" :error="user.error?.errors?.newPassword" />
                <IntegerBox class="mb-3" label="Quota in MB" v-model="user.model.quota"
                    :error="user.error?.errors?.quota" />
                <CheckBox class="mb-3" label="Master" v-model="user.model.isMaster" :error="user.error?.errors?.isMaster" />
                <CheckBox class="mb-3" label="Disabled" v-model="user.model.disabled"
                    :error="user.error?.errors?.disabled" />
            </form>
        </template>
        <template #footer>
            <p v-if="user.error" class="text-danger">{{ user.error.message }}</p>
            <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
            <SpinButton class="btn-primary" :loading="user.submitting" text="Save" loadingText="Saving" @click="submit" />
        </template>
    </Modal>
</template>