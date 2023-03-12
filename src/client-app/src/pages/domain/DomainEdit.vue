<script setup lang="ts">
import { reactive } from 'vue';
import { AddressCM, AddressService, AddressType, DomainService, DomainUM, DomainVM } from '../../api';
import CheckBox from '../../components/form/CheckBox.vue';
import SpinButton from '../../components/form/SpinButton.vue';
import Text from '../../components/form/TextBox.vue';
import SelectBox from '../../components/form/SelectBox.vue';
import IntegerBox from '../../components/form/IntegerBox.vue';
import IModelState from '../../components/form/modelState';
import AddressList from './AddressList.vue';

const props = defineProps<{ id: number }>()
const domain = reactive<IModelState<DomainUM>>({ loading: true });
const address = reactive<IModelState<AddressCM>>({})

const mapDomainModel = (m: DomainVM) =>
    domain.model = {
        name: m.name,
        host: m.host,
        username: m.username,
        port: m.port,
        disabled: m.disabled ? true : false,
    }

const emptyAddress = () => address.model = { domainId: props.id, pattern: '', type: AddressType.Exact }

const addressTypes: { value: number, label: string }[] = [
    { value: AddressType.Exact, label: 'Exact' },
    { value: AddressType.Prefix, label: 'Prefix' },
    { value: AddressType.Suffix, label: 'Suffix' },
    { value: AddressType.CatchAll, label: 'CatchAll' },
];

const submitDomain = () => {
    if (!domain.model)
        return;

    domain.submitting = true;
    domain.error = undefined;
    DomainService.updateDomain({ domainId: props.id, requestBody: domain.model })
        .then(mapDomainModel)
        .catch(r => domain.error = r.body)
        .finally(() => domain.submitting = false);
};

const submitAddress = () => {
    if (!address.model)
        return;

    address.submitting = true;
    address.error = undefined;
    AddressService.createAddress({ requestBody: address.model })
        .then(emptyAddress)
        .catch(r => address.error = r.body)
        .finally(() => address.submitting = false);
}

DomainService.getDomain({ domainId: props.id })
    .then(mapDomainModel)
    .catch(r => domain.error = r.body)
    .finally(() => domain.loading = false);

emptyAddress();

</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">Edit domain</h1>
        <button class="btn btn-sm btn-secondary" @click="$router.back()">Back</button>
    </div>
    <div class="row">
        <div class="col-md-4">
            <form v-if="domain.model" @submit.prevent="submitDomain">
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
                <SpinButton class="btn-primary" :loading="domain.submitting" text="Save" loadingText="Saving" isSubmit />
            </form>
            <p v-else-if="domain.error">{{ domain.error.message }}</p>
            <p v-else>Loading...</p>
        </div>
        <div class="col-md"></div>
        <div class="col-md-4">
            <form v-if="domain.model && address.model" @submit.prevent="submitAddress">
                <fieldset>
                    <legend>New domain address</legend>
                    <SelectBox class="mb-3" label="Type" v-model="address.model.type" :error="address.error?.errors?.type"
                        :options="addressTypes" required />
                    <Text v-if="address.model.type !== AddressType.CatchAll" class="mb-3" label="Pattern"
                        v-model="address.model.pattern" required :error="address.error?.errors?.pattern" />
                    <Text class="mb-3" label="Description" v-model="address.model.description"
                        :error="address.error?.errors?.description" />
                </fieldset>
                <SpinButton class="btn-primary" :loading="address.submitting" text="Add" loadingText="Adding" isSubmit />
            </form>
        </div>
    </div>
    <AddressList :id="props.id" />
</template>
