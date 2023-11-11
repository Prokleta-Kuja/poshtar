<script setup lang="ts">
import { reactive } from 'vue'
import GeneralModal from '@/components/GeneralModal.vue'
import TransactionLogs from '@/lists/TransactionLogs.vue'
import TransactionRecipients from '@/lists/TransactionRecipients.vue'
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
import { type TransactionLM, TransactionService } from '@/api'
import CheckBox from '@/components/form/CheckBox.vue'
import { useRoute, useRouter } from 'vue-router'
import XLgIcon from '@/components/icons/XLgIcon.vue'
import PostCardIcon from '@/components/icons/PostCardIcon.vue'
import StackIcon from '@/components/icons/StackIcon.vue'
import DisplayIcon from '@/components/icons/DisplayIcon.vue'
import RouterIcon from '@/components/icons/RouterIcon.vue'
import TelephoneInboundFillIcon from '@/components/icons/TelephoneInboundFillIcon.vue'
import UnlockIcon from '@/components/icons/UnlockIcon.vue'
import LockIcon from '@/components/icons/LockIcon.vue'
import TelephoneOutboundFillIcon from '@/components/icons/TelephoneOutboundFillIcon.vue'

interface ITransactionParams extends ITableParams {
  searchTerm?: string
  connectionId?: string
  includeMonitor?: boolean
  includePrivate?: boolean
}

const route = useRoute()
const router = useRouter()
const show = reactive<{ logsId?: number; recipientsId?: number }>({})
const data = reactive<{ params: ITransactionParams; items: TransactionLM[] }>({
  params: { ...initParams(route.query), includePrivate: true, sortBy: 'start' },
  items: []
})
const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  const query = { ...route.query, ...getQuery(data.params) }
  router.replace({ query })

  TransactionService.getTransactions({ ...data.params })
    .then((r) => {
      data.items = r.items
      updateParams(data.params, r)
    })
    .catch(() => {
      /* TODO: show error  */
    })
}
const remove = (transactionId: number) => {
  TransactionService.deleteTransaction({ transactionId })
    .then(() => refresh())
    .catch(() => {
      /* TODO: show error */
    })
}
const showLogs = (transactionId: number) => (show.logsId = transactionId)
const hideLogs = () => (show.logsId = undefined)
const showRecipients = (transactionId: number) => (show.recipientsId = transactionId)
const hideRecipients = () => (show.recipientsId = undefined)
const timeText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleTimeString()
}
const dateText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleDateString()
}

refresh()
</script>
<template>
  <main>
    <h1 class="display-6 me-3">Transactions</h1>
    <GeneralModal v-if="show.logsId" title="Logs" width="lg" shown :onClose="hideLogs">
      <template #body>
        <TransactionLogs v-if="show.logsId" :transactionId="show.logsId" query-prefix="l" />
      </template>
      <template #footer>
        <button class="btn btn-outline-danger" @click="hideLogs">Close</button>
      </template>
    </GeneralModal>
    <GeneralModal
      v-if="show.recipientsId"
      title="Recipients"
      width="lg"
      shown
      :onClose="hideRecipients"
    >
      <template #body>
        <TransactionRecipients v-if="show.recipientsId" :transactionId="show.recipientsId" />
      </template>
      <template #footer>
        <button class="btn btn-outline-danger" @click="hideRecipients">Close</button>
      </template>
    </GeneralModal>

    <div class="d-flex flex-wrap">
      <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refresh" />
      <Search
        autoFocus
        class="me-3 mb-2"
        style="max-width: 16rem"
        placeholder="Client, from"
        v-model="data.params.searchTerm"
        :on-change="refresh"
      />
      <!-- <Search class="me-3 mb-2" style="max-width:16rem" label="Connection Id" v-model="data.params.connectionId" :on-change="refresh" /> -->
      <div class="me-3 mb-2">
        <label class="form-label">Include</label>
        <div class="pt-1">
          <CheckBox
            v-model="data.params.includeMonitor"
            label="Monitor"
            inline
            :onChange="refresh"
          />
          <CheckBox
            v-model="data.params.includePrivate"
            label="Private"
            inline
            :onChange="refresh"
          />
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
                <LockIcon />
              </span>
              <span v-else class="text-danger me-1" title="Insecure">
                <UnlockIcon />
              </span>
              <span v-if="item.submission" class="text-primary" title="Submission">
                <TelephoneOutboundFillIcon />
              </span>
              <span v-else title="Relay" class="text-secondary">
                <TelephoneInboundFillIcon />
              </span>
              <span class="ms-2" :title="dateText(item.start)">{{ timeText(item.start) }}</span>
            </td>
            <td class="text-end">
              <span v-if="item.countryCode === 'XX'" :title="item.countryName ?? ''">
                <RouterIcon />
              </span>
              <span v-else-if="item.countryCode === 'ZZ'" :title="item.countryName ?? ''">
                <DisplayIcon />
              </span>
              <span v-else :title="item.countryName ?? ''">
                {{ item.countryCode }}
              </span>
            </td>
            <td class="user-select-all text-nowrap" :title="item.ipAddress ?? ''">
              {{ item.asn }}
            </td>
            <td class="user-select-all text-nowrap" :title="item.from ?? ''">{{ item.client }}</td>
            <td class="text-end p-1">
              <div class="btn-group" role="group">
                <button class="btn btn-sm btn-primary" @click="showLogs(item.id)" title="Logs">
                  <StackIcon />
                </button>
                <button
                  v-if="item.queued"
                  class="btn btn-sm btn-success"
                  @click="showRecipients(item.id)"
                  title="Recipients"
                >
                  <PostCardIcon />
                </button>
                <button class="btn btn-sm btn-danger" title="Delete" @click="remove(item.id)">
                  <XLgIcon />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <Pages class="mb-3" :params="data.params" :on-change="refresh" />
  </main>
</template>
