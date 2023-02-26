<script setup lang="ts">
import { reactive } from 'vue';
import { DomainService, DomainUM, DomainVM, ValidationError } from '../../api';
import CheckBox from '../../components/form/CheckBox.vue';
import IntegerBox from '../../components/form/IntegerBox.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import Text from '../../components/form/TextBox.vue';

const props = defineProps<{ id: number }>()
const state = reactive<{ saving?: boolean, model?: DomainUM, error?: ValidationError }>({});

const mapModel = (m: DomainVM) => {
    state.model = {
        name: m.name,
        host: m.host,
        username: m.username,
        port: m.port,
        isSecure: m.isSecure,
    }
}

const submit = () => {
    if (!state.model)
        return;

    state.saving = true;
    state.error = undefined;
    DomainService.updateDomain({ domainId: props.id, requestBody: state.model })
        .then(mapModel)
        .catch(r => state.error = r.body)
        .finally(() => state.saving = false);
};

DomainService.getDomain({ domainId: props.id })
    .then(mapModel)
    .catch(r => state.error = r.body);

</script>
<template>
    <h1 class="display-6">Edit domain</h1>
    <div class="row">
        <form class="col-md-4" v-if="state.model" @submit.prevent="submit">
            <Text class="mb-3" label="Domain" :placeholder="'example.com'" autoFocus v-model="state.model.name" required
                :error="state.error?.errors?.name" />
            <fieldset>
                <legend>Outgoing SMTP server</legend>
                <Text class="mb-3" label="Host" :placeholder="'mail.example.com'" v-model="state.model.host" required
                    :error="state.error?.errors?.host" />
                <Text class="mb-3" label="Username" :autoComplete="'off'" v-model="state.model.username" required
                    :error="state.error?.errors?.username" />
                <Text class="mb-3" label="Replace password" :autoComplete="'off'" :type="'password'"
                    v-model="state.model.newPassword" :error="state.error?.errors?.newPassword" />
                <IntegerBox class="mb-3" label="Port" v-model="state.model.port" required
                    :error="state.error?.errors?.port" />
                <CheckBox class="mb-3" label="Use TLS" v-model="state.model.isSecure"
                    :error="state.error?.errors?.isSecure" />
                <SpinButton class="btn-primary" :loading="state.saving" text="Save" loadingText="Saving" isSubmit />
            </fieldset>
        </form>
        <p v-else-if="state.error">{{ state.error.message }}</p>
        <p v-else>Loading...</p>
</div></template>
