<script setup lang="ts">
import { reactive } from 'vue';
import Modal from '@/components/Modal.vue';
import TransactionLogs from '@/lists/TransactionLogs.vue'
import TransactionRecipients from '@/lists/TransactionRecipients.vue';
import Search from '@/components/form/SearchBox.vue'
import { Header, Pages, Sizes, type ITableParams, initParams, updateParams } from "@/components/table"
import { type TransactionLM, TransactionService } from '@/api';

interface ITransactionParams extends ITableParams {
    searchTerm?: string;
    connectionId?: string;
}

const show = reactive<{ logsId?: number, recipientsId?: number }>({})
const data = reactive<{ params: ITransactionParams, items: TransactionLM[] }>({ params: { ...initParams(), sortBy: "start" }, items: [] });
const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    TransactionService.getTransactions({ ...data.params })
        .then(r => { data.items = r.items; updateParams(data.params, r) })
        .catch(() => {/* TODO: show error  */ });
};
const remove = (transactionId: number) => {
    TransactionService.deleteTransaction({ transactionId })
        .then(() => refresh())
        .catch(() => {/* TODO: show error */ })
}
const showLogs = (transactionId: number) => show.logsId = transactionId;
const hideLogs = () => show.logsId = undefined;
const showRecipients = (transactionId: number) => show.recipientsId = transactionId;
const hideRecipients = () => show.recipientsId = undefined;
const timeText = (dateTime: string | null | undefined) => {
    if (!dateTime)
        return '-';
    var dt = new Date(dateTime);
    return dt.toLocaleString();
}

refresh();
</script>
<template>
    <h1 class="display-6 me-3">Transactions</h1>
    <Modal title="Logs" width="lg" :shown="show.logsId !== undefined" :onClose="hideLogs">
        <template #body>
            <TransactionLogs v-if="show.logsId" :transactionId="show.logsId" />
        </template>
        <template #footer>
            <button class="btn btn-outline-danger" @click="hideLogs">Close</button>
        </template>
    </Modal>
    <Modal title="Recipients" width="lg" :shown="show.recipientsId !== undefined" :onClose="hideRecipients">
        <template #body>
            <TransactionRecipients v-if="show.recipientsId" :transactionId="show.recipientsId" />
        </template>
        <template #footer>
            <button class="btn btn-outline-danger" @click="hideRecipients">Close</button>
        </template>
    </Modal>

    <div class="d-flex flex-wrap">
        <Sizes class="me-3 mb-2" style="max-width:8rem" :params="data.params" :on-change="refresh" />
        <Search autoFocus class="me-3 mb-2" style="max-width:16rem" placeholder="Client, from"
            v-model="data.params.searchTerm" :on-change="refresh" />
        <Search class="me-3 mb-2" style="max-width:16rem" label="Connection Id" v-model="data.params.connectionId"
            :on-change="refresh" />
    </div>
    <div class="table-responsive">
        <table class="table">
            <thead>
                <Header :params="data.params" :on-sort="refresh" column="connectionId" />
                <Header :params="data.params" :on-sort="refresh" column="client" />
                <Header :params="data.params" :on-sort="refresh" column="start" />
                <Header :params="data.params" :on-sort="refresh" column="end" />
                <Header :params="data.params" :on-sort="refresh" column="from" />
                <Header :params="data.params" :on-sort="refresh" column="complete" />
                <th></th>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <td class="user-select-all">{{ item.connectionId }}</td>
                    <td>{{ item.client }}</td>
                    <td>{{ timeText(item.start) }}</td>
                    <td>{{ timeText(item.end) }}</td>
                    <td>
                        <span>{{ item.from }}</span>
                        <span v-if="item.username">- {{ item.username }}</span>
                    </td>
                    <td>
                        <svg v-if="item.complete" xmlns="http://www.w3.org/2000/svg" width="16" height="16"
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
                        <div class="btn-group" role="group">
                            <button class="btn btn-secondary" @click="showLogs(item.id)" title="Logs">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    class="bi bi-stack" viewBox="0 0 16 16">
                                    <path
                                        d="m14.12 10.163 1.715.858c.22.11.22.424 0 .534L8.267 15.34a.598.598 0 0 1-.534 0L.165 11.555a.299.299 0 0 1 0-.534l1.716-.858 5.317 2.659c.505.252 1.1.252 1.604 0l5.317-2.66zM7.733.063a.598.598 0 0 1 .534 0l7.568 3.784a.3.3 0 0 1 0 .535L8.267 8.165a.598.598 0 0 1-.534 0L.165 4.382a.299.299 0 0 1 0-.535L7.733.063z" />
                                    <path
                                        d="m14.12 6.576 1.715.858c.22.11.22.424 0 .534l-7.568 3.784a.598.598 0 0 1-.534 0L.165 7.968a.299.299 0 0 1 0-.534l1.716-.858 5.317 2.659c.505.252 1.1.252 1.604 0l5.317-2.659z" />
                                </svg>
                            </button>
                            <button v-if="item.complete" class="btn btn-secondary" @click="showRecipients(item.id)"
                                title="Recipients">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    class="bi bi-postcard" viewBox="0 0 16 16">
                                    <path fill-rule="evenodd"
                                        d="M2 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H2ZM1 4a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1V4Zm7.5.5a.5.5 0 0 0-1 0v7a.5.5 0 0 0 1 0v-7ZM2 5.5a.5.5 0 0 1 .5-.5H6a.5.5 0 0 1 0 1H2.5a.5.5 0 0 1-.5-.5Zm0 2a.5.5 0 0 1 .5-.5H6a.5.5 0 0 1 0 1H2.5a.5.5 0 0 1-.5-.5Zm0 2a.5.5 0 0 1 .5-.5H6a.5.5 0 0 1 0 1H2.5a.5.5 0 0 1-.5-.5ZM10.5 5a.5.5 0 0 0-.5.5v3a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-3a.5.5 0 0 0-.5-.5h-3ZM13 8h-2V6h2v2Z" />
                                </svg>
                            </button>
                            <button class="btn btn-danger" title="Delete" @click="remove(item.id)">
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
</template>