<script setup lang="ts">
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import Text from '../../components/form/TextBox.vue';
import IntegerBox from '../../components/form/IntegerBox.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import { DomainCM, DomainService, ValidationError } from '../../api';

const router = useRouter()
const state = reactive<{ saving?: boolean, model: DomainCM, error?: ValidationError }>({ model: { name: 'example.com', host: '', username: '', password: '', port: 587 } });
const submit = () => {
    if (!state.model)
        return;

    state.saving = true;
    state.error = undefined;
    DomainService.createDomain({ requestBody: state.model })
        .then(r => router.push({ name: 'route.domainEdit', params: { id: r.id } }))
        .catch(r => state.error = r.body)
        .finally(() => state.saving = false);
};
</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">New domain</h1>
        <button class="btn btn-sm btn-secondary" @click="$router.back()">Back</button>
    </div>
    <div class="row">
        <div class="col-md"></div>
        <form class="col-md-4" v-if="state.model" @submit.prevent="submit">
            <Text class="mb-3" label="Domain" autoFocus v-model="state.model.name" required
                :error="state.error?.errors?.name" />
            <fieldset>
                <legend>Outgoing SMTP server</legend>
                <Text class="mb-3" label="Host" v-model="state.model.host" required :error="state.error?.errors?.host" />
                <IntegerBox class="mb-3" label="Port" v-model="state.model.port" required
                    :error="state.error?.errors?.port" />
                <Text class="mb-3" label="Username" v-model="state.model.username" required
                    :error="state.error?.errors?.username" />
                <Text class="mb-3" label="Password" :autoComplete="'off'" :type="'password'" v-model="state.model.password"
                    required :error="state.error?.errors?.password" />
            </fieldset>
            <SpinButton class="btn-primary" :loading="state.saving" text="Add" loadingText="Adding" isSubmit />
        </form>
        <p v-else-if="state.error">{{ state.error.message }}</p>
        <div class="col-md"></div>
    </div>
</template>
