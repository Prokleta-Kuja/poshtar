<script setup lang="ts">
import { reactive } from 'vue'
import { type UserLM, UserService } from '@/api'
import Search from '@/components/form/SearchBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams
} from '@/components/table'
import AddUser from '@/modals/AddUser.vue'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'
import InfinityIcon from '@/components/icons/InfinityIcon.vue'
import CheckLgIcon from '@/components/icons/CheckLgIcon.vue'

interface IUserParams extends ITableParams {
  searchTerm?: string
}

const data = reactive<{ params: IUserParams; items: UserLM[]; delete?: UserLM }>({
  params: initParams(),
  items: []
})
const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  UserService.getUsers({ ...data.params }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
  })
}
const showDelete = (user: UserLM) => (data.delete = user)
const hideDelete = () => (data.delete = undefined)
const deleteUser = () => {
  if (!data.delete) return

  UserService.deleteUser({ userId: data.delete.id })
    .then(() => {
      refresh()
      hideDelete()
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

refresh()
</script>
<template>
  <main>
    <div class="d-flex align-items-center flex-wrap">
      <h1 class="display-6 me-3">Users</h1>
      <AddUser />
    </div>
    <div class="d-flex flex-wrap">
      <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refresh" />
      <Search
        autoFocus
        class="me-3 mb-2"
        style="max-width: 16rem"
        placeholder="Name, Description"
        v-model="data.params.searchTerm"
        :on-change="refresh"
      />
    </div>
    <div class="table-responsive">
      <table class="table">
        <thead>
          <tr>
            <Header :params="data.params" :on-sort="refresh" column="name" />
            <Header :params="data.params" :on-sort="refresh" column="description" />
            <Header :params="data.params" :on-sort="refresh" column="isMaster" display="Master" />
            <Header
              :params="data.params"
              :on-sort="refresh"
              column="quotaMegaBytes"
              display="Quota"
            />
            <Header
              :params="data.params"
              :on-sort="refresh"
              column="addressCount"
              display="Address count"
            />
            <Header :params="data.params" :on-sort="refresh" column="disabled" display="Disabled" />
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in data.items" :key="item.id" class="align-middle">
            <td>
              <RouterLink :to="{ name: 'route.userDetails', params: { id: item.id } }"
                >{{ item.name }}
              </RouterLink>
            </td>
            <td>{{ item.description }}</td>
            <td>
              <CheckLgIcon v-if="item.isMaster" />
              <XLgIcon v-else class="text-danger" />
            </td>
            <td>
              <span v-if="item.quotaMegaBytes">{{ item.quotaMegaBytes }} MB</span>
              <InfinityIcon v-else />
            </td>
            <td>{{ item.addressCount }}</td>
            <td>{{ disabledText(item.disabled) }}</td>
            <td class="text-end p-1">
              <div class="btn-group" role="group">
                <button class="btn btn-sm btn-danger" title="Delete" @click="showDelete(item)">
                  <XLgIcon />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <Pages :params="data.params" :on-change="refresh" />
    <ConfirmationModal
      v-if="data.delete"
      title="User deletion"
      :onClose="hideDelete"
      :onConfirm="deleteUser"
      shown
    >
      Are you sure you want to remove user <b>{{ data.delete.name }}</b> and all their messages?
    </ConfirmationModal>
  </main>
</template>
