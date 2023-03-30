<script setup lang="ts">
import { reactive } from "vue";
import { type DomainLM, DomainService } from "@/api";
import Search from '@/components/form/SearchBox.vue'
import { Header, Pages, Sizes, type ITableParams, initParams, updateParams } from "@/components/table"
import AddDomain from "@/modals/AddDomain.vue";
import ConfirmationModal from "@/components/ConfirmationModal.vue";

interface IDomainParams extends ITableParams {
    searchTerm?: string;
}

const data = reactive<{ params: IDomainParams, items: DomainLM[], delete?: DomainLM }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    DomainService.getDomains({ ...data.params }).then(r => { data.items = r.items; updateParams(data.params, r) });
};
const showDelete = (domain: DomainLM) => data.delete = domain;
const hideDelete = () => data.delete = undefined;
const deleteDomain = () => {
    if (!data.delete)
        return;

    DomainService.deleteDomain({ domainId: data.delete.id })
        .then(() => {
            refresh();
            hideDelete();
        })
        .catch(() => {/* TODO: show error */ })
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
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">Domains</h1>
        <AddDomain />
    </div>
    <div class="d-flex flex-wrap">
        <Sizes class="me-3 mb-2" style="max-width:8rem" :params="data.params" :on-change="refresh" />
        <Search autoFocus class="me-3 mb-2" style="max-width:16rem" placeholder="Name, Host"
            v-model="data.params.searchTerm" :on-change="refresh" />
    </div>
    <div class="table-responsive">
        <table class="table">
            <thead>
                <tr>
                    <Header :params="data.params" :on-sort="refresh" column="name" />
                    <Header :params="data.params" :on-sort="refresh" column="host" display="Upstream host" />
                    <Header :params="data.params" :on-sort="refresh" column="username" display="Upstream user" />
                    <Header :params="data.params" :on-sort="refresh" column="port" display="Upstream port" />
                    <Header :params="data.params" :on-sort="refresh" column="disabled" display="Disabled" />
                    <Header :params="data.params" :on-sort="refresh" column="addressCount" display="Address count" />
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id" class="align-middle">
                    <td>
                        <RouterLink :to="{ name: 'route.domainDetails', params: { id: item.id } }">{{ item.name }}
                        </RouterLink>
                    </td>
                    <td>{{ item.host }}</td>
                    <td>{{ item.username }}</td>
                    <td>{{ item.port }}</td>
                    <td>{{ disabledText(item.disabled) }}</td>
                    <td>{{ item.addressCount }}</td>
                    <td class="text-end p-1">
                        <div class="btn-group" role="group">
                            <button class="btn btn-sm btn-danger" title="Delete" @click="showDelete(item)">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    class="bi bi-x-lg" viewBox="0 0 16 16">
                                    <path
                                        d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z" />
                                </svg>
                            </button>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages :params="data.params" :on-change="refresh" />
    <ConfirmationModal v-if="data.delete" title="Domain deletion" :onClose="hideDelete" :onConfirm="deleteDomain" shown>
        Are you sure you want to remove domain <b>{{ data.delete.name }}</b>?
    </ConfirmationModal>
</template>