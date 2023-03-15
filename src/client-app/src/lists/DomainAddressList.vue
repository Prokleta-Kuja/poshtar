<script setup lang="ts">
import { reactive, watch } from "vue";
import { AddressLM, AddressService, AddressType } from "../api";
import Search from '../components/form/Search.vue'
import { Header, Pages, Sizes, ITableParams, initParams, updateParams } from "../components/table"

interface IAddressParams extends ITableParams {
    searchTerm?: string;
}

const props = defineProps<{ domainId: number, lastChange?: Date }>()
const data = reactive<{ params: IAddressParams, items: AddressLM[] }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    AddressService.getAddresses({ ...data.params, domainId: props.domainId })
        .then(r => { data.items = r.items; updateParams(data.params, r) });
};

watch(() => props.lastChange, () => refresh());

const remove = (addressId: number) => AddressService.deleteAddress({ addressId: addressId })
    .then(() => refresh())

const patternText = (type: AddressType, pattern: string, domain: string) => {
    switch (type) {
        case AddressType.CatchAll: return `***@${domain}`;
        case AddressType.Exact: return `${pattern}@${domain}`;
        case AddressType.Prefix: return `${pattern}***@${domain}`;
        case AddressType.Suffix: return `***${pattern}@${domain}`;
    }
}

const typeText = (type: AddressType) => {
    switch (type) {
        case AddressType.CatchAll: return 'Catch All';
        case AddressType.Exact: return 'Exact';
        case AddressType.Prefix: return 'Prefix';
        case AddressType.Suffix: return 'Suffix';
    }
}

const disabledText = (dateTime: string | null | undefined) => {
    if (!dateTime)
        return '-';
    var dt = new Date(dateTime);
    return dt.toLocaleString();
}

refresh();
</script>
<template>
    <div class="d-flex flex-wrap">
        <Sizes class="me-3 mb-2" style="max-width:8rem" :params="data.params" :on-change="refresh" />
        <Search autoFocus class="me-3 mb-2" style="max-width:16rem" placeholder="Pattern" v-model="data.params.searchTerm"
            :on-change="refresh" />
    </div>
    <div class="table-responsive">
        <table class="table">
            <thead>
                <tr>
                    <Header :params="data.params" :on-sort="refresh" column="pattern" />
                    <Header :params="data.params" :on-sort="refresh" column="type" />
                    <Header :params="data.params" :on-sort="refresh" column="description" />
                    <Header :params="data.params" :on-sort="refresh" column="disabled" display="Disabled" />
                    <Header :params="data.params" :on-sort="refresh" column="userCount" display="User count" />
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <td>
                        {{ patternText(item.type, item.pattern, item.domainName) }}
                    </td>
                    <td>{{ typeText(item.type) }}</td>
                    <td>{{ item.description }}</td>
                    <td>{{ disabledText(item.disabled) }}</td>
                    <td>{{ item.userCount }}</td>
                    <td class="text-end">
                        <button class="btn btn-danger" @click="remove(item.id)">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-x-lg" viewBox="0 0 16 16">
                                <path
                                    d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z" />
                            </svg>
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages class="mb-2" :params="data.params" :on-change="refresh" />
</template>