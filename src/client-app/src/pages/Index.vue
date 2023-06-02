<script setup lang="ts">
import { reactive } from 'vue';
import Modal from '@/components/Modal.vue';
import TransactionLogs from '@/lists/TransactionLogs.vue'
import TransactionRecipients from '@/lists/TransactionRecipients.vue';
import Search from '@/components/form/SearchBox.vue'
import { Header, Pages, Sizes, type ITableParams, initParams, updateParams } from "@/components/table"
import { type TransactionLM, TransactionService } from '@/api';
import CheckBox from '@/components/form/CheckBox.vue';

interface ITransactionParams extends ITableParams {
    searchTerm?: string;
    connectionId?: string;
    includeMonitor?: boolean;
    includePrivate?: boolean;
}

const show = reactive<{ logsId?: number, recipientsId?: number }>({})
const data = reactive<{ params: ITransactionParams, items: TransactionLM[] }>({ params: { ...initParams(), includePrivate: true, sortBy: "start" }, items: [] });
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
    return dt.toLocaleTimeString();
}
const dateText = (dateTime: string | null | undefined) => {
    if (!dateTime)
        return '-';
    var dt = new Date(dateTime);
    return dt.toLocaleDateString();
}

refresh();
</script>
<template>
    <h1 class="display-6 me-3">Transactions</h1>
    <Modal v-if="show.logsId" title="Logs" width="lg" shown :onClose="hideLogs">
        <template #body>
            <TransactionLogs v-if="show.logsId" :transactionId="show.logsId" />
        </template>
        <template #footer>
            <button class="btn btn-outline-danger" @click="hideLogs">Close</button>
        </template>
    </Modal>
    <Modal v-if="show.recipientsId" title="Recipients" width="lg" :onClose="hideRecipients">
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
        <!-- <Search class="me-3 mb-2" style="max-width:16rem" label="Connection Id" v-model="data.params.connectionId" :on-change="refresh" /> -->
        <div class="me-3 mb-2">
            <label class="form-label">Include</label>
            <div class="pt-1">
                <CheckBox v-model="data.params.includeMonitor" label="Monitor" inline :onChange="refresh" />
                <CheckBox v-model="data.params.includePrivate" label="Private" inline :onChange="refresh" />
            </div>
        </div>
    </div>
    <div class="table-responsive">
        <table class="table table-sm">
            <thead>
                <Header :params="data.params" :on-sort="refresh" column="start" />
                <Header :params="data.params" :on-sort="refresh" column="countryCode" display="Code" />
                <Header :params="data.params" :on-sort="refresh" column="asn" display="ASN" />
                <Header :params="data.params" :on-sort="refresh" column="client" />
                <!-- <Header :params="data.params" :on-sort="refresh" column="from" /> -->
                <th></th>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id" class="align-middle">
                    <td class="text-nowrap">
                        <span v-if="item.secure" class="text-success me-1" title="Secure">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-lock" viewBox="0 0 16 16">
                                <path
                                    d="M8 1a2 2 0 0 1 2 2v4H6V3a2 2 0 0 1 2-2zm3 6V3a3 3 0 0 0-6 0v4a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2zM5 8h6a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1z" />
                            </svg>
                        </span>
                        <span v-else class="text-danger me-1" title="Insecure">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-unlock" viewBox="0 0 16 16">
                                <path
                                    d="M11 1a2 2 0 0 0-2 2v4a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h5V3a3 3 0 0 1 6 0v4a.5.5 0 0 1-1 0V3a2 2 0 0 0-2-2zM3 8a1 1 0 0 0-1 1v5a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V9a1 1 0 0 0-1-1H3z" />
                            </svg>
                        </span>
                        <span v-if="item.submission" class="text-primary" title="Submission">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-telephone-outbound-fill" viewBox="0 0 16 16">
                                <path fill-rule="evenodd"
                                    d="M1.885.511a1.745 1.745 0 0 1 2.61.163L6.29 2.98c.329.423.445.974.315 1.494l-.547 2.19a.678.678 0 0 0 .178.643l2.457 2.457a.678.678 0 0 0 .644.178l2.189-.547a1.745 1.745 0 0 1 1.494.315l2.306 1.794c.829.645.905 1.87.163 2.611l-1.034 1.034c-.74.74-1.846 1.065-2.877.702a18.634 18.634 0 0 1-7.01-4.42 18.634 18.634 0 0 1-4.42-7.009c-.362-1.03-.037-2.137.703-2.877L1.885.511zM11 .5a.5.5 0 0 1 .5-.5h4a.5.5 0 0 1 .5.5v4a.5.5 0 0 1-1 0V1.707l-4.146 4.147a.5.5 0 0 1-.708-.708L14.293 1H11.5a.5.5 0 0 1-.5-.5z" />
                            </svg>
                        </span>
                        <span v-else title="Relay" class="text-secondary">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-telephone-inbound-fill" viewBox="0 0 16 16">
                                <path fill-rule="evenodd"
                                    d="M1.885.511a1.745 1.745 0 0 1 2.61.163L6.29 2.98c.329.423.445.974.315 1.494l-.547 2.19a.678.678 0 0 0 .178.643l2.457 2.457a.678.678 0 0 0 .644.178l2.189-.547a1.745 1.745 0 0 1 1.494.315l2.306 1.794c.829.645.905 1.87.163 2.611l-1.034 1.034c-.74.74-1.846 1.065-2.877.702a18.634 18.634 0 0 1-7.01-4.42 18.634 18.634 0 0 1-4.42-7.009c-.362-1.03-.037-2.137.703-2.877L1.885.511zM15.854.146a.5.5 0 0 1 0 .708L11.707 5H14.5a.5.5 0 0 1 0 1h-4a.5.5 0 0 1-.5-.5v-4a.5.5 0 0 1 1 0v2.793L15.146.146a.5.5 0 0 1 .708 0z" />
                            </svg>
                        </span>
                        <span class="ms-2" :title="dateText(item.start)">{{ timeText(item.start) }}</span>
                    </td>
                    <td class="text-end">
                        <span v-if="item.countryCode === 'XX'" :title="item.countryName ?? ''">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-router" viewBox="0 0 16 16">
                                <path
                                    d="M5.525 3.025a3.5 3.5 0 0 1 4.95 0 .5.5 0 1 0 .707-.707 4.5 4.5 0 0 0-6.364 0 .5.5 0 0 0 .707.707Z" />
                                <path
                                    d="M6.94 4.44a1.5 1.5 0 0 1 2.12 0 .5.5 0 0 0 .708-.708 2.5 2.5 0 0 0-3.536 0 .5.5 0 0 0 .707.707ZM2.5 11a.5.5 0 1 1 0-1 .5.5 0 0 1 0 1Zm4.5-.5a.5.5 0 1 0 1 0 .5.5 0 0 0-1 0Zm2.5.5a.5.5 0 1 1 0-1 .5.5 0 0 1 0 1Zm1.5-.5a.5.5 0 1 0 1 0 .5.5 0 0 0-1 0Zm2 0a.5.5 0 1 0 1 0 .5.5 0 0 0-1 0Z" />
                                <path
                                    d="M2.974 2.342a.5.5 0 1 0-.948.316L3.806 8H1.5A1.5 1.5 0 0 0 0 9.5v2A1.5 1.5 0 0 0 1.5 13H2a.5.5 0 0 0 .5.5h2A.5.5 0 0 0 5 13h6a.5.5 0 0 0 .5.5h2a.5.5 0 0 0 .5-.5h.5a1.5 1.5 0 0 0 1.5-1.5v-2A1.5 1.5 0 0 0 14.5 8h-2.306l1.78-5.342a.5.5 0 1 0-.948-.316L11.14 8H4.86L2.974 2.342ZM14.5 9a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-.5.5h-13a.5.5 0 0 1-.5-.5v-2a.5.5 0 0 1 .5-.5h13Z" />
                                <path d="M8.5 5.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0Z" />
                            </svg>
                        </span>
                        <span v-else-if="item.countryCode === 'ZZ'" :title="item.countryName ?? ''">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                class="bi bi-display" viewBox="0 0 16 16">
                                <path
                                    d="M0 4s0-2 2-2h12s2 0 2 2v6s0 2-2 2h-4c0 .667.083 1.167.25 1.5H11a.5.5 0 0 1 0 1H5a.5.5 0 0 1 0-1h.75c.167-.333.25-.833.25-1.5H2s-2 0-2-2V4zm1.398-.855a.758.758 0 0 0-.254.302A1.46 1.46 0 0 0 1 4.01V10c0 .325.078.502.145.602.07.105.17.188.302.254a1.464 1.464 0 0 0 .538.143L2.01 11H14c.325 0 .502-.078.602-.145a.758.758 0 0 0 .254-.302 1.464 1.464 0 0 0 .143-.538L15 9.99V4c0-.325-.078-.502-.145-.602a.757.757 0 0 0-.302-.254A1.46 1.46 0 0 0 13.99 3H2c-.325 0-.502.078-.602.145z" />
                            </svg>
                        </span>
                        <span v-else :title="item.countryName ?? ''">
                            {{ item.countryCode }}
                        </span>
                    </td>
                    <td class="user-select-all text-nowrap" :title="item.ipAddress ?? ''">{{ item.asn }}</td>
                    <td class="user-select-all text-nowrap" :title="item.from ?? ''">{{ item.client }}</td>
                    <td class="text-end p-1">
                        <div class="btn-group" role="group">
                            <button class="btn btn-sm btn-primary" @click="showLogs(item.id)" title="Logs">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    class="bi bi-stack" viewBox="0 0 16 16">
                                    <path
                                        d="m14.12 10.163 1.715.858c.22.11.22.424 0 .534L8.267 15.34a.598.598 0 0 1-.534 0L.165 11.555a.299.299 0 0 1 0-.534l1.716-.858 5.317 2.659c.505.252 1.1.252 1.604 0l5.317-2.66zM7.733.063a.598.598 0 0 1 .534 0l7.568 3.784a.3.3 0 0 1 0 .535L8.267 8.165a.598.598 0 0 1-.534 0L.165 4.382a.299.299 0 0 1 0-.535L7.733.063z" />
                                    <path
                                        d="m14.12 6.576 1.715.858c.22.11.22.424 0 .534l-7.568 3.784a.598.598 0 0 1-.534 0L.165 7.968a.299.299 0 0 1 0-.534l1.716-.858 5.317 2.659c.505.252 1.1.252 1.604 0l5.317-2.659z" />
                                </svg>
                            </button>
                            <button v-if="item.queued" class="btn btn-sm btn-success" @click="showRecipients(item.id)"
                                title="Recipients">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    class="bi bi-postcard" viewBox="0 0 16 16">
                                    <path fill-rule="evenodd"
                                        d="M2 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H2ZM1 4a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1V4Zm7.5.5a.5.5 0 0 0-1 0v7a.5.5 0 0 0 1 0v-7ZM2 5.5a.5.5 0 0 1 .5-.5H6a.5.5 0 0 1 0 1H2.5a.5.5 0 0 1-.5-.5Zm0 2a.5.5 0 0 1 .5-.5H6a.5.5 0 0 1 0 1H2.5a.5.5 0 0 1-.5-.5Zm0 2a.5.5 0 0 1 .5-.5H6a.5.5 0 0 1 0 1H2.5a.5.5 0 0 1-.5-.5ZM10.5 5a.5.5 0 0 0-.5.5v3a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-3a.5.5 0 0 0-.5-.5h-3ZM13 8h-2V6h2v2Z" />
                                </svg>
                            </button>
                            <button class="btn btn-sm btn-danger" title="Delete" @click="remove(item.id)">
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
    <Pages class="mb-3" :params="data.params" :on-change="refresh" />
</template>