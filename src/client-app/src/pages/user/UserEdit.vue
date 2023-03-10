<script setup lang="ts">
import { reactive } from 'vue';
import { UserService, UserUM, UserVM, ValidationError } from '../../api';
import CheckBox from '../../components/form/CheckBox.vue';
import IntegerBox from '../../components/form/IntegerBox.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import Text from '../../components/form/TextBox.vue';

const props = defineProps<{ id: number }>()
const state = reactive<{ saving?: boolean, model?: UserUM, error?: ValidationError }>({});

const mapModel = (m: UserVM) => {
    state.model = {
        name: m.name,
        description: m.description,
        isMaster: m.isMaster,
        quota: m.quotaMegaBytes,
        disabled: m.disabled ? true : false,
    }
}

const submit = () => {
    if (!state.model)
        return;

    state.saving = true;
    state.error = undefined;
    UserService.updateUser({ userId: props.id, requestBody: state.model })
        .then(mapModel)
        .catch(r => state.error = r.body)
        .finally(() => state.saving = false);
};

UserService.getUser({ userId: props.id })
    .then(mapModel)
    .catch(r => state.error = r.body);

</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">Edit user</h1>
        <button class="btn btn-sm btn-secondary" @click="$router.back()">Back</button>
    </div>
    <div class="row">
        <form class="col-md-4" v-if="state.model" @submit.prevent="submit">
            <Text class="mb-3" label="Username" autoFocus v-model="state.model.name" required
                :error="state.error?.errors?.name" />
            <Text class="mb-3" label="Description" v-model="state.model.description"
                :error="state.error?.errors?.description" />
            <Text class="mb-3" label="Replace password" :autoComplete="'off'" :type="'password'"
                v-model="state.model.newPassword" :error="state.error?.errors?.newPassword" />
            <IntegerBox class="mb-3" label="Quota in MB" v-model="state.model.quota" :error="state.error?.errors?.quota" />
            <CheckBox class="mb-3" label="Master" v-model="state.model.isMaster" :error="state.error?.errors?.isMaster" />
            <CheckBox class="mb-3" label="Disabled" v-model="state.model.disabled" :error="state.error?.errors?.disabled" />
            <SpinButton class="btn-primary" :loading="state.saving" text="Save" loadingText="Saving" isSubmit />
        </form>
        <p v-else-if="state.error">{{ state.error.message }}</p>
        <p v-else>Loading...</p>
    </div>
</template>
