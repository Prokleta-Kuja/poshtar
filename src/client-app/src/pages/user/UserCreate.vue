<script setup lang="ts">
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import IntegerBox from '../../components/form/IntegerBox.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import Text from '../../components/form/TextBox.vue';
import { UserCM, UserService, ValidationError } from '../../api';

const router = useRouter()
const state = reactive<{ saving?: boolean, model?: UserCM, error?: ValidationError }>({ model: { name: '', password: '', isMaster: false } });
const submit = () => {
    if (!state.model)
        return;

    state.saving = true;
    state.error = undefined;
    UserService.createUser({ requestBody: state.model })
        .then(r => router.push({ name: 'route.userEdit', params: { id: r.id } }))
        .catch(r => state.error = r.body)
        .finally(() => state.saving = false);
};
</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">New user</h1>
        <button class="btn btn-sm btn-secondary" @click="$router.back()">Back</button>
    </div>
    <div class="row">
        <form class="col-md-4" v-if="state.model" @submit.prevent="submit">
            <Text class="mb-3" label="Username" autoFocus v-model="state.model.name" required
                :error="state.error?.errors?.name" />
            <Text class="mb-3" label="Description" v-model="state.model.description"
                :error="state.error?.errors?.description" />
            <Text class="mb-3" label="Password" :autoComplete="'off'" :type="'password'" v-model="state.model.password"
                :error="state.error?.errors?.password" required />
            <IntegerBox class="mb-3" label="Quota in MB" v-model="state.model.quota" :error="state.error?.errors?.quota" />
            <CheckBox class="mb-3" label="Master" v-model="state.model.isMaster" :error="state.error?.errors?.isMaster" />
            <SpinButton class="btn-primary" :loading="state.saving" text="Save" loadingText="Saving" isSubmit />
        </form>
        <p v-else-if="state.error">{{ state.error.message }}</p>
        <p v-else>Loading...</p>
    </div>
</template>
