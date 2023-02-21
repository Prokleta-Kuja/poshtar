<script setup lang="ts">
import { reactive } from "vue";
import { DomainLM, DomainService } from "../api";
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

const query = () => refresh();
const clearSearch = () => { data.params.searchTerm = undefined; query(); }

refresh();
</script>
<template>
    <form class="d-flex flex-wrap" @submit.prevent="query">
        <div class="me-3 mb-2" style="width:8rem">
            <Sizes :params="data.params" :on-change="refresh" />
        </div>

        <div class="me-3 mb-2">
            <label for="search" class="form-label">Search</label>
            <div class="input-group">
                <input class="form-control" id="search" placeholder="Name, Host" v-model="data.params.searchTerm">
                <button class="btn btn-outline-secondary" type="submit">Search</button>
                <button class="btn btn-outline-secondary" type="button" @click.prevent="clearSearch">Clear</button>
            </div>
        </div>

    </form>
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