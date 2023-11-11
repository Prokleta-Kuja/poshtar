<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { type AddressLM, type AddressVM, AddressService, AddressType } from '@/api'
import Search from '@/components/form/SearchBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams
} from '@/components/table'
import EditAddress from '@/modals/EditAddress.vue'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'

interface IAddressParams extends ITableParams {
  searchTerm?: string
}

const props = defineProps<{ domainId: number; lastChange?: Date }>()
const data = reactive<{ params: IAddressParams; items: AddressLM[]; delete?: AddressLM }>({
  params: initParams(),
  items: []
})
const address = ref<AddressVM | undefined>(undefined)

const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  AddressService.getAddresses({ ...data.params, domainId: props.domainId }).then((r) => {
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

const edit = (model: AddressVM) => (address.value = model)
const update = (updatedAddress?: AddressVM) => {
  if (updatedAddress && address.value) {
    address.value.description = updatedAddress.description
    address.value.disabled = updatedAddress.disabled
    address.value.domainId = updatedAddress.domainId
    address.value.pattern = updatedAddress.pattern
    address.value.type = updatedAddress.type
  }
  address.value = undefined
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
  </div>
  <EditAddress :model="address" @updated="update" />
  <div class="table-responsive">
    <table class="table table-sm">
      <thead>
        <tr>
          <Header :params="data.params" :on-sort="refresh" column="pattern" />
          <Header :params="data.params" :on-sort="refresh" column="type" />
          <Header :params="data.params" :on-sort="refresh" column="description" />
          <Header :params="data.params" :on-sort="refresh" column="disabled" display="Disabled" />
          <Header
            :params="data.params"
            :on-sort="refresh"
            column="userCount"
            display="User count"
          />
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
          <td>{{ item.userCount }}</td>
          <td class="text-end p-1">
            <div class="btn-group" role="group">
              <button class="btn btn-sm btn-secondary" @click="edit(item)" title="Edit">
                <PencilSquareIcon />
              </button>
              <button class="btn btn-sm btn-danger" title="Delete" @click="showDelete(item)">
                <XLgIcon />
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <Pages class="mb-2" :params="data.params" :on-change="refresh" />
  <ConfirmationModal
    v-if="data.delete"
    title="Address deletion"
    :onClose="hideDelete"
    :onConfirm="deleteAddress"
    shown
  >
    Are you sure you want to remove
    <b>{{ patternText(data.delete.type, data.delete.pattern, data.delete.domainName) }}</b
    >?
  </ConfirmationModal>
</template>
