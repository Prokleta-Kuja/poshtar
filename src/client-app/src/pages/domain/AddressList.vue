<script setup lang="ts">
import { reactive, watch } from "vue";
import { AddressLM, AddressService, AddressType } from "../../api";
import Search from '../../components/form/Search.vue'
import { Header, Pages, Sizes, ITableParams, initParams, updateParams } from "../../components/table"

interface IAddressParams extends ITableParams {
    searchTerm?: string;
}

const props = defineProps<{ id: number, lastChange?: Date }>()
const data = reactive<{ params: IAddressParams, items: AddressLM[] }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    AddressService.getAddresses({ ...data.params, domainId: props.id })
        .then(r => { data.items = r.items; updateParams(data.params, r) });
};

watch(() => props.lastChange, () => refresh());

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
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <td>
                        <RouterLink :to="{ name: 'route.domainDetails', params: { id: item.id } }"> TODO LINK {{
                            item.pattern
                        }}</RouterLink>
                    </td>
                    <td>{{ typeText(item.type) }}</td>
                    <td>{{ item.description }}</td>
                    <td>{{ disabledText(item.disabled) }}</td>
                    <td>{{ item.userCount }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages class="mb-2" :params="data.params" :on-change="refresh" />
</template>