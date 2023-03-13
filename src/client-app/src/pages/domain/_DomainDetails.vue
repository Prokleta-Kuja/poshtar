<script setup lang="ts">
import { reactive, ref } from 'vue';
import { DomainService, DomainUM, DomainVM } from '../../api';
import CheckBox from '../../components/form/CheckBox.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import Text from '../../components/form/TextBox.vue';
import IntegerBox from '../../components/form/IntegerBox.vue';
import IModelState from '../../components/form/modelState';
import Modal from '../../components/Modal.vue';
import AddressList from './AddressList.vue';
import AddAddress from './AddAddress.vue';

const props = defineProps<{ id: number }>()
const modal = ref(false)
const domain = reactive<IModelState<DomainUM>>({ loading: true, model: undefined! });

const toggle = () => modal.value = !modal.value
const mapDomainModel = (m: DomainVM) =>
    domain.model = {
        name: m.name,
        host: m.host,
        username: m.username,
        port: m.port,
        disabled: m.disabled ? true : false,
    }
const submit = () => {
    domain.submitting = true;
    domain.error = undefined;
    DomainService.updateDomain({ domainId: props.id, requestBody: domain.model })
        .then(r => { mapDomainModel(r); modal.value = false; })
        .catch(r => domain.error = r.body)
        .finally(() => domain.submitting = false);
};

DomainService.getDomain({ domainId: props.id })
    .then(mapDomainModel)
    .catch(r => domain.error = r.body)
    .finally(() => domain.loading = false);
</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">
            <span v-if="!domain.model || modal">Domain</span>
            <span v-else>{{ domain.model.name }}</span>
            details
        </h1>
        <button class="btn btn-sm btn-secondary me-3" @click="$router.back()">Back</button>
        <template v-if="domain.model">
            <button class="btn btn-primary me-3" @click="toggle">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                    class="bi bi-pencil-square" viewBox="0 0 16 16">
                    <path
                        d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z" />
                    <path fill-rule="evenodd"
                        d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z" />
                </svg>
                Domain
            </button>
            <AddAddress :domain-id="props.id" />
        </template>
    </div>
    <Modal v-if="domain.model" title="Edit domain" :shown="modal" :onClose="toggle">
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
    <AddressList :id="props.id" />
</template>
