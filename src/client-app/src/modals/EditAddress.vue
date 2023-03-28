<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { type AddressUM, AddressService, type AddressVM, AddressType } from '@/api';
import type IModelState from '@/components/form/modelState';
import Modal from '@/components/Modal.vue';
import SpinButton from '@/components/form/SpinButton.vue';
import Text from '@/components/form/TextBox.vue';
import SelectBox from '@/components/form/SelectBox.vue';
import CheckBox from '@/components/form/CheckBox.vue';
export interface IEditAddress {
    model?: AddressVM,
    onUpdated?: (updatedAddress?: AddressVM) => void
}

const mapUserModel = (m?: AddressVM): AddressUM =>
({
    domainId: m?.domainId ?? 0,
    pattern: m?.pattern ?? '',
    type: m?.type ?? AddressType.Exact,
    description: m?.description ?? '',
    disabled: m?.disabled ? true : false,
})
const props = defineProps<IEditAddress>()
const shown = ref(false)
const address = reactive<IModelState<AddressUM>>({ model: mapUserModel(props.model) })

watch(() => props.model, () => {
    address.error = undefined;
    address.submitting = undefined;
    address.model = mapUserModel(props.model);
})

const addressTypes: { value: number, label: string }[] = [
    { value: AddressType.Exact, label: 'Exact' },
    { value: AddressType.Prefix, label: 'Prefix' },
    { value: AddressType.Suffix, label: 'Suffix' },
    { value: AddressType.CatchAll, label: 'CatchAll' },
];

const close = () => {
    if (props.onUpdated)
        props.onUpdated()
}
const submit = () => {
    if (!props.model)
        return;
    address.submitting = true;
    address.error = undefined;
    AddressService.updateAddress({ addressId: props.model.id, requestBody: address.model })
        .then(r => {
            shown.value = false;
            if (props.onUpdated)
                props.onUpdated(r);
        })
        .catch(r => address.error = r.body)
        .finally(() => address.submitting = false);
};
</script>
<template>
    <Modal v-if="props.model" title="Edit address" shown :onClose="close">
        <template #body>
            <form @submit.prevent="submit">
                <SelectBox class="mb-3" label="Type" v-model="address.model.type" :error="address.error?.errors?.type"
                    :options="addressTypes" required autoFocus />
                <Text v-if="address.model.type !== AddressType.CatchAll" class="mb-3" label="Pattern"
                    v-model="address.model.pattern" required :error="address.error?.errors?.pattern" />
                <Text class="mb-3" label="Description" v-model="address.model.description"
                    :error="address.error?.errors?.description" />
                <CheckBox class="mb-3" label="Disabled" v-model="address.model.disabled"
                    :error="address.error?.errors?.disabled" />
            </form>
        </template>
        <template #footer>
            <p v-if="address.error" class="text-danger">{{ address.error.message }}</p>
            <button class="btn btn-outline-danger" @click="close">Cancel</button>
            <SpinButton class="btn-primary" :loading="address.submitting" text="Save" loadingText="Saving"
                @click="submit" />
        </template>
    </Modal>
</template>