<script setup lang="ts">
import { reactive, ref } from 'vue';
import { AddressCM, AddressService, AddressType } from '../api';
import IModelState from '../components/form/modelState';
import Modal from '../components/Modal.vue';
import SelectBox from '../components/form/SelectBox.vue';
import SpinButton from '../components/form/SpinButton.vue';
import Text from '../components/form/TextBox.vue';

const props = defineProps<{ domainId: number, onAdded?: () => void }>()
const blank = (): AddressCM => ({ domainId: props.domainId, pattern: '', type: AddressType.Exact })
const shown = ref(false)
const address = reactive<IModelState<AddressCM>>({ model: blank() })

const addressTypes: { value: number, label: string }[] = [
    { value: AddressType.Exact, label: 'Exact' },
    { value: AddressType.Prefix, label: 'Prefix' },
    { value: AddressType.Suffix, label: 'Suffix' },
    { value: AddressType.CatchAll, label: 'CatchAll' },
];
const toggle = () => shown.value = !shown.value
const submit = () => {
    if (!address.model)
        return;

    address.submitting = true;
    address.error = undefined;
    AddressService.createAddress({ requestBody: address.model })
        .then(r => {
            address.model = blank();
            if (props.onAdded)
                props.onAdded();
            shown.value = false;
        })
        .catch(r => address.error = r.body)
        .finally(() => address.submitting = false);
}
</script>
<template>
    <button class="btn btn-success" @click="toggle">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-lg"
            viewBox="0 0 16 16">
            <path fill-rule="evenodd"
                d="M8 2a.5.5 0 0 1 .5.5v5h5a.5.5 0 0 1 0 1h-5v5a.5.5 0 0 1-1 0v-5h-5a.5.5 0 0 1 0-1h5v-5A.5.5 0 0 1 8 2Z" />
        </svg>
        Address
    </button>
    <Modal title="Add domain address" :shown="shown" :onClose="toggle">
        <template #body>
            <form @submit.prevent="submit">
                <SelectBox class="mb-3" label="Type" v-model="address.model.type" :error="address.error?.errors?.type"
                    :options="addressTypes" required autoFocus />
                <Text v-if="address.model.type !== AddressType.CatchAll" class="mb-3" label="Pattern"
                    v-model="address.model.pattern" required :error="address.error?.errors?.pattern" />
                <Text class="mb-3" label="Description" v-model="address.model.description"
                    :error="address.error?.errors?.description" />
            </form>
        </template>
        <template #footer>
            <p v-if="address.error" class="text-danger">{{ address.error.message }}</p>
            <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
            <SpinButton class="btn-primary" :loading="address.submitting" text="Add" loadingText="Adding" @click="submit" />
        </template>
    </Modal>
</template>