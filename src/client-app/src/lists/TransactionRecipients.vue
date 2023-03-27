<script setup lang="ts">
import { reactive } from 'vue';
import { RecipientLM, TransactionService } from '../api';
import Search from '../components/form/Search.vue'
import { Header, Pages, Sizes, ITableParams, initParams, updateParams } from '../components/table';

interface ITransactionRecipientsParams extends ITableParams {
    searchTerm?: string;
}

const props = defineProps<{ transactionId: number }>()
const data = reactive<{ params: ITransactionRecipientsParams, items: RecipientLM[] }>({ params: initParams(), items: [] });

const refresh = (params?: ITableParams) => {
    if (params)
        data.params = params;

    TransactionService.getRecipients({ ...data.params, transactionId: props.transactionId })
        .then(r => { data.items = r.items; updateParams(data.params, r) });
};

const getAddresses = (item: RecipientLM) => {
    const addresses = JSON.parse(item.data) as string[];
    if (addresses)
        return addresses;
    else
        return [];
}

refresh();
</script>
<template>
    <div class="d-flex flex-wrap">
        <Sizes class="me-3 mb-2" style="max-width:8rem" :params="data.params" :on-change="refresh" />
        <Search autoFocus class="me-3 mb-2" style="max-width:16rem" placeholder="User, address"
            v-model="data.params.searchTerm" :on-change="refresh" />
    </div>
    <div class="table-responsive">
        <table class="table">
            <thead>
                <tr>
                    <Header :params="data.params" :on-sort="refresh" column="type" unsortable />
                    <Header :params="data.params" :on-sort="refresh" column="address" unsortable />
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in data.items" :key="item.id">
                    <template v-if="item.userId">
                        <td>Internal user</td>
                        <td>{{ item.data }}</td>
                    </template>
                    <template v-else>
                        <td>External addresse(s)</td>
                        <td>
                            <ul>
                                <li v-for="address in getAddresses(item)">{{ address }}</li>
                            </ul>
                        </td>
                    </template>
                </tr>
            </tbody>
        </table>
    </div>
    <Pages class="mb-2" :params="data.params" :on-change="refresh" />
</template>