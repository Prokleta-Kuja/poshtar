<script setup lang="ts">
import { reactive } from "vue";
import { type DomainLM, DomainService } from "@/api";
import Search from '@/components/form/SearchBox.vue'
import { Header, Pages, Sizes, type ITableParams, initParams, updateParams } from "@/components/table"
import AddDomain from "@/modals/AddDomain.vue";

interface IDomainParams extends ITableParams {
    searchTerm?: string;
}

const data = reactive<{ params: IDomainParams, items: DomainLM[] }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    DomainService.getDomains({ ...data.params }).then(r => { data.items = r.items; updateParams(data.params, r) });
};

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
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <td>
                        <RouterLink :to="{ name: 'route.domainDetails', params: { id: item.id } }">{{ item.name }}
                        </RouterLink>
                    </td>
                    <td>{{ item.host }}</td>
                    <td>{{ item.username }}</td>
                    <td>{{ item.port }}</td>
                    <td>{{ disabledText(item.disabled) }}</td>
                    <td>{{ item.addressCount }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages :params="data.params" :on-change="refresh" />
</template>