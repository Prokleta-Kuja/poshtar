<script setup lang="ts">
import { reactive, watch } from 'vue'
import { type AddressLM, AddressService, AddressType, DomainService } from '@/api'
import Search from '@/components/form/SearchBox.vue'
import TypeaheadBox, { type KV } from '@/components/form/TypeaheadBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams
} from '@/components/table'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'

interface IAddressParams extends ITableParams {
  searchTerm?: string
  domainId?: number
}

const props = defineProps<{ notUserId?: number; userId?: number; lastChange?: Date }>()
const data = reactive<{ params: IAddressParams; items: AddressLM[]; delete?: AddressLM }>({
  params: initParams(),
  items: []
})
const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  AddressService.getAddresses({
    ...data.params,
    notUserId: props.notUserId,
    userId: props.userId
  }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
  })
}
const showDelete = (domain: AddressLM) => (data.delete = domain)
const hideDelete = () => (data.delete = undefined)
const deleteAddress = () => {
  if (!data.delete) return

  AddressService.deleteAddress({ addressId: data.delete.id })
    .then(() => {
      refresh()
      hideDelete()
    })
    .catch(() => {
      /* TODO: show error */
    })
}

watch(
  () => props.lastChange,
  () => refresh()
)

const add = (addressId: number) =>
  AddressService.addAddressUser({ addressId: addressId, userId: props.notUserId! }).then(() =>
    refresh()
  )
const remove = (addressId: number) =>
  AddressService.removeAddressUser({ addressId: addressId, userId: props.userId! }).then(() =>
    refresh()
  )

const searchDomain = async (term: string): Promise<KV[]> => {
  const response = await DomainService.getDomains({
    size: 5,
    sortBy: 'name',
    ascending: true,
    searchTerm: term
  })
  return response.items.map((i) => ({ value: i.id, label: i.name }))
}

const patternText = (type: AddressType, pattern: string, domain: string) => {
  switch (type) {
    case AddressType.CatchAll:
      return `***@${domain}`
    case AddressType.Exact:
      return `${pattern}@${domain}`
    case AddressType.Prefix:
      return `${pattern}***@${domain}`
    case AddressType.Suffix:
      return `***${pattern}@${domain}`
  }
}

const typeText = (type: AddressType) => {
  switch (type) {
    case AddressType.CatchAll:
      return 'Catch All'
    case AddressType.Exact:
      return 'Exact'
    case AddressType.Prefix:
      return 'Prefix'
    case AddressType.Suffix:
      return 'Suffix'
  }
}

const disabledText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleString()
}

refresh()
</script>
<template>
  <div class="d-flex flex-wrap">
    <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refresh" />
    <Search
      autoFocus
      class="me-3 mb-2"
      style="max-width: 16rem"
      placeholder="Pattern"
      v-model="data.params.searchTerm"
      :on-change="refresh"
    />
    <TypeaheadBox
      class="me-3 mb-2"
      label="Domain"
      style="max-width: 16rem"
      v-model="data.params.domainId"
      :onSearch="searchDomain"
      :onChange="refresh"
    />
  </div>
  <div class="table-responsive">
    <table class="table table-sm">
      <thead>
        <tr>
          <Header :params="data.params" :on-sort="refresh" column="pattern" />
          <Header :params="data.params" :on-sort="refresh" column="type" />
          <Header :params="data.params" :on-sort="refresh" column="description" />
          <Header :params="data.params" :on-sort="refresh" column="disabled" display="Disabled" />
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in data.items" :key="item.id" class="align-middle">
          <td>
            {{ patternText(item.type, item.pattern, item.domainName) }}
          </td>
          <td>{{ typeText(item.type) }}</td>
          <td>{{ item.description }}</td>
          <td>{{ disabledText(item.disabled) }}</td>
          <td class="text-end p-1">
            <template v-if="props.userId">
              <div class="btn-group" role="group">
                <button
                  class="btn btn-sm btn-outline-danger"
                  title="Unlink from user"
                  @click="remove(item.id)"
                >
                  <LinkIcon />
                </button>
                <button class="btn btn-sm btn-danger" title="Delete" @click="showDelete(item)">
                  <XLgIcon />
                </button>
              </div>
            </template>
            <template v-else-if="props.notUserId">
              <button class="btn btn-sm btn-success" @click="add(item.id)">
                <PlusLgIcon />
              </button>
            </template>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <Pages class="mb-2" :params="data.params" :on-change="refresh" />
  <ConfirmationModal
    v-if="data.delete"
    title="Domain deletion"
    :onClose="hideDelete"
    :onConfirm="deleteAddress"
    shown
  >
    Are you sure you want to remove
    <b>{{ patternText(data.delete.type, data.delete.pattern, data.delete.domainName) }}</b
    >?
  </ConfirmationModal>
</template>
