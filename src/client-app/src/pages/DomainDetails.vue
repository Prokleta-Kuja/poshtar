<script setup lang="ts">
import { reactive, ref } from 'vue';
import { DomainService, type DomainVM, type PlainError } from '@/api';
import AddressList from '@/lists/DomainAddressList.vue';
import AddDomainAddress from '@/modals/AddDomainAddress.vue';
import EditDomain from '@/modals/EditDomain.vue';

const props = defineProps<{ id: number }>()
const addressChange = ref(new Date)
const domain = reactive<{ error?: PlainError, value?: DomainVM }>({});

const updateDomain = (updatedDomain: DomainVM) => domain.value = updatedDomain;
const updateAddresses = () => addressChange.value = new Date;

DomainService.getDomain({ domainId: props.id })
    .then(r => domain.value = r)
    .catch(r => domain.error = r.body);
</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">
            <span v-if="!domain.value">Domain</span>
            <span v-else>{{ domain.value.name }}</span>
            details
        </h1>
        <button class="btn btn-sm btn-secondary me-3" @click="$router.back()">Back</button>
        <template v-if="domain.value">
            <EditDomain :model="domain.value" @updated="updateDomain" />
            <AddDomainAddress :domain-id="props.id" @added="updateAddresses" />
        </template>
    </div>
    <template v-if="domain.value">
        <AddressList :domain-id="props.id" :last-change="addressChange" />
    </template>
    <p v-else-if="domain.error" class="text-danger">{{ domain.error.message }}</p>
    <p v-else>Loading...</p>
</template>
