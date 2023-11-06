<script setup lang="ts">
import { reactive } from 'vue'
import { type LogEntryLM, TransactionService } from '@/api'
import Search from '@/components/form/SearchBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams,
  getQuery
} from '@/components/table'
import JsonViewer from '@/components/JsonViewer.vue'
import { useRoute, useRouter } from 'vue-router'

interface ITransactionLogsParams extends ITableParams {
  searchTerm?: string
}

const route = useRoute()
const router = useRouter()
const props = defineProps<{ queryPrefix?: string; transactionId: number }>()
const data = reactive<{ params: ITransactionLogsParams; items: LogEntryLM[] }>({
  params: initParams(route.query, props.queryPrefix),
  items: []
})
data.params.sortBy = 'timestamp'
data.params.ascending = true

const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  const query = { ...route.query, ...getQuery(data.params, props.queryPrefix) }
  router.replace({ query })

  TransactionService.getLogs({ ...data.params, transactionId: props.transactionId }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
  })
}

const timestampText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleTimeString()
}

refresh()
</script>
<template>
  <div class="d-flex flex-wrap">
    <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refresh" />
    <Search
      class="me-3 mb-2"
      style="max-width: 16rem"
      placeholder="Message, data"
      v-model="data.params.searchTerm"
      :on-change="refresh"
    />
  </div>
  <div class="table-responsive">
    <table class="table table-sm">
      <thead>
        <tr>
          <Header :params="data.params" :on-sort="refresh" column="timestamp" />
          <Header :params="data.params" :on-sort="refresh" column="message" />
          <Header :params="data.params" :on-sort="refresh" column="properties" display="Data" />
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in data.items" :key="item.id" class="align-middle">
          <td>{{ timestampText(item.timestamp) }}</td>
          <td>{{ item.message }}</td>
          <td v-if="item.properties">
            <JsonViewer :json="item.properties" />
          </td>
          <td v-else></td>
        </tr>
      </tbody>
    </table>
  </div>
  <Pages class="mb-2" :params="data.params" :on-change="refresh" />
</template>
