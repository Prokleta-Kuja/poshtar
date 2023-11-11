<script setup lang="ts">
import { reactive } from 'vue'
import {
  type CalendarLM,
  CalendarService,
  type CalendarVM,
  type CalendarUserVM,
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
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import PencilSquareIcon from '@/components/icons/PencilSquareIcon.vue'
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'
import CheckLgIcon from '@/components/icons/CheckLgIcon.vue'

interface ICalendarParams extends ITableParams {
  searchTerm?: string
}

const data = reactive<{ params: ICalendarParams; items: CalendarLM[]; createName?: string, update?: CalendarVM, delete?: CalendarLM }>({
  params: initParams(),
  items: []
})
const refreshCalendars = (params?: ITableParams) => {
  if (params) data.params = params

  CalendarService.getCalendars({ ...data.params }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
  })
}
const showCreate = () => {
  data.createName = ''
  data.update = undefined
}
const showUpdate = (calendarId: number) => {
  CalendarService.getCalendar({ calendarId })
    .then(r => data.update = r)
    .catch(() => {
      /* TODO: show error */
    })
}
const showDelete = (c: CalendarLM) => data.delete = c
const hideDelete = () => {
  data.delete = undefined
  refreshCalendars()
}
const deleteCalendar = () => {
  if (!data.delete) return

  CalendarService.deleteCalendar({ calendarId: data.delete.id })
    .then(() => {
      refreshCalendars()
      hideDelete()
    })
    .catch(() => {
      /* TODO: show error */
    })
}
const deleteCalendarUser = (cu: CalendarUserVM) => {
  if (data.update) {
    const calendarId = data.update.id
    CalendarService.removeCalendarUser({ calendarId, userId: cu.userId })
      .then(() => {
        showUpdate(calendarId)
        refreshCalendars()
      })
      .catch(() => {
        /* TODO: show error */
      })
  }
}

refreshCalendars()
</script>
<template>
  <main>
    <div class="d-flex align-items-center flex-wrap">
      <h1 class="display-6 me-3">Calendars</h1>
      <button class="btn btn-success me-3" @click="showCreate">
        <PlusLgIcon />
        Calendar
      </button>
    </div>
    <div class="d-flex flex-wrap">
      <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refreshCalendars" />
      <Search autoFocus class="me-3 mb-2" style="max-width: 16rem" placeholder="Name, Host"
        v-model="data.params.searchTerm" :on-change="refreshCalendars" />
    </div>
    <div class="row">
      <div class="col-6">
        <div class="table-responsive">
          <table class="table">
            <thead>
              <tr>
                <Header :params="data.params" :on-sort="refreshCalendars" column="displayName" display="Name" />
                <Header :params="data.params" :on-sort="refreshCalendars" column="userCount" display="Readers" />
                <Header :params="data.params" :on-sort="refreshCalendars" column="writeUserCount" display="Writers" />
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in data.items" :key="item.id" class="align-middle">
                <td>
                  <a href="" @click.prevent="showUpdate(item.id)">{{ item.displayName }}</a>
                </td>
                <td>{{ item.userCount }}</td>
                <td>{{ item.writeUserCount }}</td>
                <td class="text-end p-1">
                  <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-secondary" @click="showUpdate(item.id)" title="Edit">
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
        <Pages :params="data.params" :on-change="refreshCalendars" />
        <ConfirmationModal v-if="data.delete" title="Calendar deletion" :onClose="hideDelete" :onConfirm="deleteCalendar"
          shown>
          Are you sure you want to remove calendar <b>{{ data.delete.displayName }}</b>?
        </ConfirmationModal>
      </div>
      <div class="col-6">
        <div v-if="data.update" class="table-responsive">
          <table class="table">
            <thead>
              <tr>
                <th>User</th>
                <th>Write</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in data.update.users" :key="item.userId" class="align-middle">
                <td>{{ item.userName }}</td>
                <td>
                  <CheckLgIcon v-if="item.canWrite" />
                  <XLgIcon v-else />
                </td>
                <td class="text-end p-1">
                  <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-danger" title="Delete" @click="deleteCalendarUser(item)">
                      <XLgIcon />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <form v-else>
          <h1>New</h1>
        </form>
      </div>
    </div>
  </main>
</template>
