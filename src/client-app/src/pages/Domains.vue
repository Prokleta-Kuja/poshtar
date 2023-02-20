<script setup lang="ts">
import { reactive } from "vue";
import { DomainLM, DomainService } from "../api";
import { Header, Pages, Sizes } from "../components/table"
import IListResponse from "../interfaces/IListResponse";

const data = reactive<{ loaded?: boolean, params: IListResponse, domains: DomainLM[] }>({ params: { page: 1, size: 25, total: 0, ascending: false }, domains: [] });
const refresh = (params?: IListResponse) => {
    if (params)
        data.params = params;

    DomainService.getDomains({ ...data.params }).then(r => data.domains = r.items);
};

refresh();
</script>
<template>
    <Sizes :params="data.params" :on-change="refresh" all />
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
            <tr v-for="domain in data.domains" :key="domain.id">
                <td>{{ domain.name }}</td>
                <td>{{ domain.host }}</td>
                <td>{{ domain.userCount }}</td>
                <td>{{ domain.addressCount }}</td>
            </tr>
        </tbody>
    </table>
    <Pages :params="data.params" :on-change="refresh" />
</template>