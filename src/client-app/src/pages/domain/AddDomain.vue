<script setup lang="ts">
import { reactive, ref } from 'vue';
import { DomainCM, DomainService } from '../../api';
import IModelState from '../../components/form/modelState';
import Modal from '../../components/Modal.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import IntegerBox from '../../components/form/IntegerBox.vue';
import Text from '../../components/form/TextBox.vue';
import { useRouter } from 'vue-router';

const router = useRouter()
const blank = (): DomainCM => ({ name: '', host: '', port: 587, username: '', password: '' })
const shown = ref(false)
const domain = reactive<IModelState<DomainCM>>({ model: blank() })

const toggle = () => shown.value = !shown.value
const submit = () => {
    domain.submitting = true;
    domain.error = undefined;
    DomainService.createDomain({ requestBody: domain.model })
        .then(r => router.push({ name: 'route.domainEdit', params: { id: r.id } }))
        .catch(r => domain.error = r.body)
        .finally(() => domain.submitting = false);
};
</script>
<template>
    <button class="btn btn-success" @click="toggle">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus"
            viewBox="0 0 16 16">
            <path
                d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4z" />
        </svg>
        Add
    </button>
    <Modal title="Add domain" :shown="shown" :onClose="toggle">
        <template #body>
            <form @submit.prevent="submit">
                <Text class="mb-3" label="Domain" autoFocus v-model="domain.model.name" required
                    :error="domain.error?.errors?.name" />
                <fieldset>
                    <legend>Outgoing SMTP server</legend>
                    <Text class="mb-3" label="Host" v-model="domain.model.host" required
                        :error="domain.error?.errors?.host" />
                    <IntegerBox class="mb-3" label="Port" v-model="domain.model.port" required
                        :error="domain.error?.errors?.port" />
                    <Text class="mb-3" label="Username" v-model="domain.model.username" required
                        :error="domain.error?.errors?.username" />
                    <Text class="mb-3" label="Password" :autoComplete="'off'" :type="'password'"
                        v-model="domain.model.password" required :error="domain.error?.errors?.password" />
                </fieldset>
                <p v-if="domain.error" class="text-danger">{{ domain.error.message }}</p>
            </form>
        </template>
        <template #footer>
            <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
            <SpinButton class="btn-primary" :loading="domain.submitting" text="Add" loadingText="Adding" @click="submit" />
        </template>
    </Modal>
</template>