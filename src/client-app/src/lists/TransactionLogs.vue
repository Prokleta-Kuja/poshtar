<script setup lang="ts">
import { reactive } from 'vue';
import { type LogEntryLM, TransactionService } from '@/api';
import Search from '@/components/form/SearchBox.vue'
import { Header, Pages, Sizes, type ITableParams, initParams, updateParams } from '@/components/table';

interface ITransactionLogsParams extends ITableParams {
    searchTerm?: string;
}

const props = defineProps<{ transactionId: number }>()
const data = reactive<{ params: ITransactionLogsParams, items: LogEntryLM[] }>({ params: initParams(), items: [] });
data.params.sortBy = "timestamp";
data.params.ascending = true;

const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    TransactionService.getLogs({ ...data.params, transactionId: props.transactionId })
        .then(r => { data.items = r.items; updateParams(data.params, r) });
};

const timestampText = (dateTime: string | null | undefined) => {
    if (!dateTime)
        return '-';
    var dt = new Date(dateTime);
    return dt.toLocaleTimeString();
}

refresh();
</script>
<template>
    <div class="d-flex flex-wrap">
        <Sizes class="me-3 mb-2" style="max-width:8rem" :params="data.params" :on-change="refresh" />
        <Search class="me-3 mb-2" style="max-width:16rem" placeholder="Message, data" v-model="data.params.searchTerm"
            :on-change="refresh" />
    </div>
    <div class="table-responsive">
        <table class="table table-sm">
            <thead>
                <tr>
                    <Header :params="data.params" :on-sort="refresh" column="timestamp" />
                    <Header :params="data.params" :on-sort="refresh" column="message" />
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id" class="align-middle">
                    <td>{{ timestampText(item.timestamp) }}</td>
                    <td>{{ item.message }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages class="mb-2" :params="data.params" :on-change="refresh" />
</template>