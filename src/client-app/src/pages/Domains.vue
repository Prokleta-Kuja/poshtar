<script setup lang="ts">
import { reactive } from "vue";
import { DomainLM, DomainService } from "../api";
import Search from '../components/form/Search.vue'
import { Header, Pages, Sizes, ITableParams, initParams, updateParams } from "../components/table"

interface IDomainParams extends ITableParams {
    searchTerm?: string;
}

const data = reactive<{ loaded?: boolean, params: IDomainParams, items: DomainLM[] }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    DomainService.getDomains({ ...data.params }).then(r => { data.items = r.items; updateParams(data.params, r) });
};

const search = (term?: string) => { data.params.searchTerm = term; refresh(); }

refresh();
</script>
<template>
    <div class="d-flex flex-wrap">
        <Sizes class="me-3 mb-2" style="max-width:8rem" :params="data.params" :on-change="refresh" />
        <Search autoFocus class="me-3 mb-2" style="max-width:16rem" v-model="data.params.searchTerm" :on-change="refresh" />
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
                    <td>{{ item.name }}</td>
                    <td>{{ item.host }}</td>
                    <td>{{ item.userCount }}</td>
                    <td>{{ item.addressCount }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages :params="data.params" :on-change="refresh" />
</template>