<script setup lang="ts">
import { reactive, ref } from 'vue';
import { type DomainUM, DomainService, type DomainVM } from '@/api';
import type IModelState from '@/components/form/modelState';
import Modal from '@/components/Modal.vue';
import SpinButton from '@/components/form/SpinButton.vue';
import IntegerBox from '@/components/form/IntegerBox.vue';
import Text from '@/components/form/TextBox.vue';
import CheckBox from '@/components/form/CheckBox.vue';
export interface IEditDomain {
    model: DomainVM,
    onUpdated?: (updatedDomain: DomainVM) => void
}

const mapDomainModel = (m: DomainVM): DomainUM =>
({
    name: m.name,
    host: m.host,
    username: m.username,
    port: m.port,
    disabled: m.disabled ? true : false,
})
const props = defineProps<IEditDomain>()
const shown = ref(false)
const domain = reactive<IModelState<DomainUM>>({ model: mapDomainModel(props.model) })

const toggle = () => shown.value = !shown.value
const submit = () => {
    domain.submitting = true;
    domain.error = undefined;
    DomainService.updateDomain({ domainId: props.model.id, requestBody: domain.model })
        .then(r => {
            shown.value = false;
            if (props.onUpdated)
                props.onUpdated(r);
        })
        .catch(r => domain.error = r.body)
        .finally(() => domain.submitting = false);
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
        Domain
    </button>
    <Modal v-if="domain.model" title="Edit domain" :shown="shown" :onClose="toggle">
        <template #body>
            <form @submit.prevent="submit">
                <Text class="mb-3" label="Domain" autoFocus v-model="domain.model.name" required
                    :error="domain.error?.errors?.name" />
                <CheckBox class="mb-3" label="Disabled" v-model="domain.model.disabled"
                    :error="domain.error?.errors?.disabled" />
                <fieldset>
                    <legend>Outgoing SMTP server</legend>
                    <Text class="mb-3" label="Host" :placeholder="'mail.example.com'" v-model="domain.model.host" required
                        :error="domain.error?.errors?.host" />
                    <IntegerBox class="mb-3" label="Port" v-model="domain.model.port" required
                        :error="domain.error?.errors?.port" />
                    <Text class="mb-3" label="Username" :autoComplete="'off'" v-model="domain.model.username" required
                        :error="domain.error?.errors?.username" />
                    <Text class="mb-3" label="Replace password" :autoComplete="'off'" :type="'password'"
                        v-model="domain.model.newPassword" :error="domain.error?.errors?.newPassword" />
                </fieldset>
            </form>
        </template>
        <template #footer>
            <p v-if="domain.error" class="text-danger">{{ domain.error.message }}</p>
            <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
            <SpinButton class="btn-primary" :loading="domain.submitting" text="Save" loadingText="Saving" @click="submit" />
        </template>
    </Modal>
</template>