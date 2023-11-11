<script setup lang="ts">
import { reactive, ref } from 'vue'
import {
  type DomainLM,
  DomainService,
  type RelayLM,
  RelayService,
  type DomainVM,
  type RelayVM
} from '@/api'
import Search from '@/components/form/SearchBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams
} from '@/components/table'
import AddDomain from '@/modals/AddDomain.vue'
import AddRelay from '@/modals/AddRelay.vue'
import EditDomain from '@/modals/EditDomain.vue'
import EditRelay from '@/modals/EditRelay.vue'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import PencilSquareIcon from '@/components/icons/PencilSquareIcon.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'

interface IDomainParams extends ITableParams {
  searchTerm?: string
}

const domainData = reactive<{ params: IDomainParams; items: DomainLM[]; delete?: DomainLM }>({
  params: initParams(),
  items: []
})
const relayData = reactive<{ params: IDomainParams; items: RelayLM[]; delete?: RelayLM }>({
  params: initParams(),
  items: []
})
const relayKv = ref<{ value: number; label: string }[]>([])
const update = reactive<{ domain?: DomainVM; relay?: RelayVM }>({})
const refreshDomains = (params?: ITableParams) => {
  if (params) domainData.params = params

  DomainService.getDomains({ ...domainData.params }).then((r) => {
    domainData.items = r.items
    updateParams(domainData.params, r)
  })
}
const refreshRelays = (params?: ITableParams) => {
  if (params) relayData.params = params

  RelayService.getRelays({ ...relayData.params }).then((r) => {
    relayData.items = r.items
    updateParams(relayData.params, r)
    relayKv.value = []
    r.items.forEach((e) => relayKv.value.push({ value: e.id, label: e.name }))
  })
}
const refreshAll = () => {
  refreshDomains()
  refreshRelays()
}

const showUpdateDomain = (d: DomainLM) =>
  (update.domain = {
    id: d.id,
    name: d.name,
    relayId: d.relayId,
    disabled: d.disabled
  })

const showUpdateRelay = (r: RelayLM) =>
  (update.relay = {
    id: r.id,
    host: r.host,
    name: r.name,
    port: r.port,
    username: r.username,
    disabled: r.disabled
  })

const hideUpdateDomain = (domain?: DomainVM) => {
  update.domain = undefined
  if (domain) refreshAll()
}
const hideUpdateRelay = (relay?: RelayVM) => {
  update.relay = undefined
  if (relay) refreshAll()
}

const showDeleteDomain = (domain: DomainLM) => (domainData.delete = domain)
const showDeleteRelay = (relay: RelayLM) => (relayData.delete = relay)
const hideDeleteDomain = () => {
  domainData.delete = undefined
  refreshAll()
}
const hideDeleteRelay = () => {
  relayData.delete = undefined
  refreshAll()
}
const deleteDomain = () => {
  if (!domainData.delete) return

  DomainService.deleteDomain({ domainId: domainData.delete.id })
    .then(() => {
      refreshDomains()
      hideDeleteDomain()
    })
    .catch(() => {
      /* TODO: show error */
    })
}
const deleteRelay = () => {
  if (!relayData.delete) return

  RelayService.deleteRelay({ relayId: relayData.delete.id })
    .then(() => {
      refreshAll()
      hideDeleteRelay()
    })
    .catch(() => {
      /* TODO: show error */
    })
}

const disabledText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleString()
}

refreshAll()
</script>
<template>
  <main>
    <div class="d-flex align-items-center flex-wrap">
      <h1 class="display-6 me-3">Domains</h1>
      <AddDomain :onAdded="refreshAll" :relays="relayKv" />
      <AddRelay :onAdded="refreshAll" />
      <EditDomain
        v-if="update.domain"
        :relays="relayKv"
        :model="update.domain"
        :onUpdated="hideUpdateDomain"
      />
      <EditRelay v-if="update.relay" :model="update.relay" :onUpdated="hideUpdateRelay" />
    </div>
    <div class="d-flex flex-wrap">
      <Sizes
        class="me-3 mb-2"
        style="max-width: 8rem"
        :params="domainData.params"
        :on-change="refreshDomains"
      />
      <Search
        autoFocus
        class="me-3 mb-2"
        style="max-width: 16rem"
        placeholder="Name, Host"
        v-model="domainData.params.searchTerm"
        :on-change="refreshDomains"
      />
    </div>
    <div class="row">
      <div class="col-6">
        <div class="table-responsive">
          <table class="table">
            <thead>
              <tr>
                <Header :params="domainData.params" :on-sort="refreshDomains" column="name" />
                <Header
                  :params="domainData.params"
                  :on-sort="refreshDomains"
                  column="relayName"
                  display="Relay"
                />
                <Header
                  :params="domainData.params"
                  :on-sort="refreshDomains"
                  column="disabled"
                  display="Disabled"
                />
                <Header
                  :params="domainData.params"
                  :on-sort="refreshDomains"
                  column="addressCount"
                  display="Addresses"
                />
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in domainData.items" :key="item.id" class="align-middle">
                <td>
                  <RouterLink :to="{ name: 'route.domainDetails', params: { id: item.id } }"
                    >{{ item.name }}
                  </RouterLink>
                </td>
                <td>{{ item.relayName }}</td>
                <td>{{ disabledText(item.disabled) }}</td>
                <td>{{ item.addressCount }}</td>
                <td class="text-end p-1">
                  <div class="btn-group" role="group">
                    <button
                      class="btn btn-sm btn-secondary"
                      @click="showUpdateDomain(item)"
                      title="Edit"
                    >
                      <PencilSquareIcon />
                    </button>
                    <button
                      class="btn btn-sm btn-danger"
                      title="Delete"
                      @click="showDeleteDomain(item)"
                    >
                      <XLgIcon />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <Pages :params="domainData.params" :on-change="refreshDomains" />
        <ConfirmationModal
          v-if="domainData.delete"
          title="Domain deletion"
          :onClose="hideDeleteDomain"
          :onConfirm="deleteDomain"
          shown
        >
          Are you sure you want to remove domain <b>{{ domainData.delete.name }}</b
          >?
        </ConfirmationModal>
      </div>
      <div class="col-6">
        <div class="table-responsive">
          <table class="table">
            <thead>
              <tr>
                <Header :params="relayData.params" :on-sort="refreshRelays" column="name" />
                <Header :params="relayData.params" :on-sort="refreshRelays" column="host" />
                <Header :params="relayData.params" :on-sort="refreshRelays" column="username" />
                <Header :params="relayData.params" :on-sort="refreshRelays" column="port" />
                <Header :params="relayData.params" :on-sort="refreshRelays" column="disabled" />
                <Header
                  :params="relayData.params"
                  :on-sort="refreshRelays"
                  column="domainCount"
                  display="Domains"
                />
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in relayData.items" :key="item.id" class="align-middle">
                <td>{{ item.name }}</td>
                <td>{{ item.host }}</td>
                <td>{{ item.username }}</td>
                <td>{{ item.port }}</td>
                <td>{{ disabledText(item.disabled) }}</td>
                <td>{{ item.domainCount }}</td>
                <td class="text-end p-1">
                  <div class="btn-group" role="group">
                    <button
                      class="btn btn-sm btn-secondary"
                      @click="showUpdateRelay(item)"
                      title="Edit"
                    >
                      <PencilSquareIcon />
                    </button>
                    <button
                      class="btn btn-sm btn-danger"
                      title="Delete"
                      @click="showDeleteRelay(item)"
                    >
                      <XLgIcon />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <Pages :params="relayData.params" :on-change="refreshRelays" />
        <ConfirmationModal
          v-if="relayData.delete"
          title="Relay deletion"
          :onClose="hideDeleteRelay"
          :onConfirm="deleteRelay"
          shown
        >
          Are you sure you want to remove relay <b>{{ relayData.delete.name }}</b
          >?
        </ConfirmationModal>
      </div>
    </div>
  </main>
</template>
