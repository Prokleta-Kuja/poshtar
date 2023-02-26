<script setup lang="ts">
import { reactive } from "vue";
import { UserLM, UserService } from "../../api";
import Search from '../../components/form/Search.vue'
import { Header, Pages, Sizes, ITableParams, initParams, updateParams } from "../../components/table"

interface IDomainParams extends ITableParams {
    searchTerm?: string;
}

const data = reactive<{ params: IDomainParams, items: UserLM[] }>({ params: initParams(), items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    UserService.getUsers({ ...data.params }).then(r => { data.items = r.items; updateParams(data.params, r) });
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
        <h1 class="display-6 me-3">Users</h1>
        <RouterLink :to="{ name: 'route.userCreate', params: { id: 'new' } }" type="button" class="btn btn-sm btn-success">
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
        <Search autoFocus class="me-3 mb-2" style="max-width:16rem" placeholder="Name, Description"
            v-model="data.params.searchTerm" :on-change="refresh" />
    </div>
    <div class="table-responsive">
        <table class="table">
            <thead>
                <tr>
                    <Header :params="data.params" :on-sort="refresh" column="name" />
                    <Header :params="data.params" :on-sort="refresh" column="description" />
                    <Header :params="data.params" :on-sort="refresh" column="isMaster" display="Master" />
                    <Header :params="data.params" :on-sort="refresh" column="quotaMegaBytes" display="Quota" />
                    <Header :params="data.params" :on-sort="refresh" column="addressCount" display="Address count" />
                    <Header :params="data.params" :on-sort="refresh" column="disabled" display="Disabled" />
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <td>
                        <RouterLink :to="{ name: 'route.userEdit', params: { id: item.id } }">{{ item.name }}</RouterLink>
                    </td>
                    <td>{{ item.description }}</td>
                    <td>
                        <svg v-if="item.isMaster" xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                            fill="currentColor" class="bi bi-check-lg text-success" viewBox="0 0 16 16">
                            <path
                                d="M12.736 3.97a.733.733 0 0 1 1.047 0c.286.289.29.756.01 1.05L7.88 12.01a.733.733 0 0 1-1.065.02L3.217 8.384a.757.757 0 0 1 0-1.06.733.733 0 0 1 1.047 0l3.052 3.093 5.4-6.425a.247.247 0 0 1 .02-.022Z" />
                        </svg>
                        <svg v-else xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                            class="bi bi-x-lg text-danger" viewBox="0 0 16 16">
                            <path
                                d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z" />
                        </svg>
                    </td>
                    <td>
                        <span v-if="item.quotaMegaBytes !== null">{{ item.quotaMegaBytes }} MB</span>
                        <svg v-else xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                            class="bi bi-infinity" viewBox="0 0 16 16">
                            <path
                                d="M5.68 5.792 7.345 7.75 5.681 9.708a2.75 2.75 0 1 1 0-3.916ZM8 6.978 6.416 5.113l-.014-.015a3.75 3.75 0 1 0 0 5.304l.014-.015L8 8.522l1.584 1.865.014.015a3.75 3.75 0 1 0 0-5.304l-.014.015L8 6.978Zm.656.772 1.663-1.958a2.75 2.75 0 1 1 0 3.916L8.656 7.75Z" />
                        </svg>
                    </td>
                    <td>{{ item.addressCount }}</td>
                    <td>{{ disabledText(item.disabled) }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages :params="data.params" :on-change="refresh" />
</template>