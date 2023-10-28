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
    updateParams(domainData.params, r)
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
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      class="bi bi-pencil-square"
                      viewBox="0 0 16 16"
                    >
                      <path
                        d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"
                      />
                      <path
                        fill-rule="evenodd"
                        d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z"
                      />
                    </svg>
                  </button>
                  <button
                    class="btn btn-sm btn-danger"
                    title="Delete"
                    @click="showDeleteDomain(item)"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      class="bi bi-x-lg"
                      viewBox="0 0 16 16"
                    >
                      <path
                        d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"
                      />
                    </svg>
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
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      class="bi bi-pencil-square"
                      viewBox="0 0 16 16"
                    >
                      <path
                        d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"
                      />
                      <path
                        fill-rule="evenodd"
                        d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z"
                      />
                    </svg>
                  </button>
                  <button
                    class="btn btn-sm btn-danger"
                    title="Delete"
                    @click="showDeleteRelay(item)"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      class="bi bi-x-lg"
                      viewBox="0 0 16 16"
                    >
                      <path
                        d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"
                      />
                    </svg>
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
</template>
