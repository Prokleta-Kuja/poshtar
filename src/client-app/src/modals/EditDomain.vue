<script setup lang="ts">
import { reactive } from 'vue';
import { type DomainUM, DomainService, type DomainVM } from '@/api';
import type IModelState from '@/components/form/modelState';
import Modal from '@/components/Modal.vue';
import SpinButton from '@/components/form/SpinButton.vue';
import Text from '@/components/form/TextBox.vue';
import CheckBox from '@/components/form/CheckBox.vue';
import SelectBox from '@/components/form/SelectBox.vue';

export interface IEditDomain {
    model: DomainVM,
    relays: { value: number, label: string }[],
    onUpdated: (updatedDomain?: DomainVM) => void,
    shown?: boolean,
}

const mapDomainModel = (m: DomainVM): DomainUM =>
({
    name: m.name,
    relayId: m.relayId,
    disabled: m.disabled ? true : false,
})
const props = defineProps<IEditDomain>()
const domain = reactive<IModelState<DomainUM>>({ model: mapDomainModel(props.model) })

const toggle = () => props.onUpdated();
const submit = () => {
    domain.submitting = true;
    domain.error = undefined;
    DomainService.updateDomain({ domainId: props.model.id, requestBody: domain.model })
        .then(r => props.onUpdated(r))
        .catch(r => domain.error = r.body)
        .finally(() => domain.submitting = false);
};
</script>
<template>
    <Modal v-if="domain.model" title="Edit domain" shown :onClose="toggle">
        <template #body>
            <form @submit.prevent="submit">
                <Text class="mb-3" label="Domain" autoFocus v-model="domain.model.name" required
                    :error="domain.error?.errors?.name" />
                <CheckBox class="mb-3" label="Disabled" v-model="domain.model.disabled"
                    :error="domain.error?.errors?.disabled" />
                <SelectBox class="mb-3" label="Relay" v-model="domain.model.relayId" :error="domain.error?.errors?.relayId"
                    :options="props.relays" undefined-label="No relay. Local routing only." />
            </form>
        </template>
        <template #footer>
            <p v-if="domain.error" class="text-danger">{{ domain.error.message }}</p>
            <button class="btn btn-outline-danger" @click="toggle">Cancel</button>
            <SpinButton class="btn-primary" :loading="domain.submitting" text="Save" loadingText="Saving" @click="submit" />
        </template>
    </Modal>
</template>