<script setup lang="ts">
import { reactive } from "vue";
import { DomainLM, DomainService } from "../../api";
import Search from '../../components/form/Search.vue'
import { Header, Pages, Sizes, ITableParams, initParams, updateParams } from "../../components/table"

interface IDomainParams extends ITableParams {
    searchTerm?: string;
}

const data = reactive<{ params: IDomainParams, items: DomainLM[] }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    DomainService.getDomains({ ...data.params }).then(r => { data.items = r.items; updateParams(data.params, r) });
};

refresh();
</script>
<template>
    <div class="d-flex align-items-center flex-wrap">
        <h1 class="display-6 me-3">Domains</h1>
        <RouterLink :to="{ name: 'route.domainCreate', params: { id: 'new' } }" type="button"
            class="btn btn-sm btn-success">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-lg"
                viewBox="0 0 16 16">
                <path fill-rule="evenodd"
                    d="M8 2a.5.5 0 0 1 .5.5v5h5a.5.5 0 0 1 0 1h-5v5a.5.5 0 0 1-1 0v-5h-5a.5.5 0 0 1 0-1h5v-5A.5.5 0 0 1 8 2Z" />
            </svg>
            Add
        </RouterLink>
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
                    <Header :params="data.params" :on-sort="refresh" column="host" unsortable />
                    <Header :params="data.params" :on-sort="refresh" column="userCount" display="User count" />
                    <Header :params="data.params" :on-sort="refresh" column="addressCount" display="Address count" />
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <td>
                        <RouterLink :to="{ name: 'route.domainEdit', params: { id: item.id } }">{{ item.name }}</RouterLink>
                    </td>
                    <td>{{ item.host }}</td>
                    <td>{{ item.userCount }}</td>
                    <td>{{ item.addressCount }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages :params="data.params" :on-change="refresh" />
</template>