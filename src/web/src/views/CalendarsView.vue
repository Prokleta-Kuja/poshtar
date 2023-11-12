<script setup lang="ts">
import { reactive } from 'vue'
import {
  type CalendarLM,
  CalendarService,
  type CalendarVM,
  type CalendarUserVM,
  type CalendarUserSM
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
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'
import CheckLgIcon from '@/components/icons/CheckLgIcon.vue'
import Text from '@/components/form/TextBox.vue'
import type IModelState from '@/components/form/modelState'
import SpinButton from '@/components/form/SpinButton.vue'
import GeneralModal from '@/components/GeneralModal.vue'
import PencilIcon from '@/components/icons/PencilIcon.vue'
import BookIcon from '@/components/icons/BookIcon.vue'

interface ICalendarParams extends ITableParams {
  searchTerm?: string
}
interface ICalendarUserParams extends ITableParams {
  searchTerm?: string
}
const model = reactive<IModelState<{ displayName: string }>>({ model: { displayName: '' } })
const calendars = reactive<{
  params: ICalendarParams
  items: CalendarLM[]
  update?: CalendarVM
  delete?: CalendarLM
  showUsers?: boolean
}>({
  params: initParams(),
  items: []
})
const users = reactive<{ params: ICalendarUserParams; items: CalendarUserSM[] }>({
  params: initParams(),
  items: []
})
const refreshCalendars = (params?: ITableParams) => {
  if (params) calendars.params = params

  CalendarService.getCalendars({ ...calendars.params }).then((r) => {
    calendars.items = r.items
    updateParams(calendars.params, r)
  })
}
const refreshUsers = (params?: ITableParams) => {
  if (!calendars.update) return
  if (params) users.params = params

  CalendarService.getCalendarAddableUsers({
    calendarId: calendars.update.id,
    ...users.params
  }).then((r) => {
    users.items = r.items
    updateParams(users.params, r)
  })
}
const showCreate = () => {
  model.model.displayName = ''
  calendars.update = undefined
}
const showUpdate = (calendarId: number) => {
  CalendarService.getCalendar({ calendarId })
    .then((r) => {
      calendars.update = r
      model.model.displayName = r.displayName
    })
    .catch(() => {
      /* TODO: show error */
    })
}
const showDelete = (c: CalendarLM) => (calendars.delete = c)
const hideDelete = () => {
  calendars.delete = undefined
  refreshCalendars()
}
const deleteCalendar = () => {
  if (!calendars.delete) return

  CalendarService.deleteCalendar({ calendarId: calendars.delete.id })
    .then(() => {
      refreshCalendars()
      hideDelete()
    })
    .catch(() => {
      /* TODO: show error */
    })
}
const addCalendarUser = (cu: CalendarUserSM, canWrite: boolean) => {
  if (calendars.update) {
    const calendarId = calendars.update.id
    CalendarService.addCalendarUser({ calendarId, userId: cu.userId, canWrite })
      .then(() => {
        showUpdate(calendarId)
        refreshCalendars()
        refreshUsers()
      })
      .catch(() => {
        /* TODO: show error */
      })
  }
}
const deleteCalendarUser = (cu: CalendarUserVM) => {
  if (calendars.update) {
    const calendarId = calendars.update.id
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
const submit = () => {
  const req = { requestBody: { displayName: model.model.displayName } }
  if (calendars.update)
    CalendarService.updateCalendar({ calendarId: calendars.update.id, ...req }).then(() => {
      calendars.update = undefined
      model.model.displayName = ''
      refreshCalendars()
    })
  else
    CalendarService.createCalendar(req).then(() => {
      model.model.displayName = ''
      refreshCalendars()
    })
}
const addUsers = () => {
  refreshUsers()
  calendars.showUsers = true
}
const hideUsers = () => {
  calendars.showUsers = false
  users.items = []
  users.params = initParams()
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
      <Sizes
        class="me-3 mb-2"
        style="max-width: 8rem"
        :params="calendars.params"
        :on-change="refreshCalendars"
      />
      <Search
        autoFocus
        class="me-3 mb-2"
        style="max-width: 16rem"
        placeholder="Display name"
        v-model="calendars.params.searchTerm"
        :on-change="refreshCalendars"
      />
    </div>
    <div class="row">
      <div class="col-6">
        <div class="table-responsive">
          <table class="table">
            <thead>
              <tr>
                <Header
                  :params="calendars.params"
                  :on-sort="refreshCalendars"
                  column="displayName"
                  display="Name"
                />
                <Header
                  :params="calendars.params"
                  :on-sort="refreshCalendars"
                  column="userCount"
                  display="Readers"
                />
                <Header
                  :params="calendars.params"
                  :on-sort="refreshCalendars"
                  column="writeUserCount"
                  display="Writers"
                />
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in calendars.items" :key="item.id" class="align-middle">
                <td>
                  <a href="" @click.prevent="showUpdate(item.id)">{{ item.displayName }}</a>
                </td>
                <td>{{ item.userCount }}</td>
                <td>{{ item.writeUserCount }}</td>
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
        <Pages :params="calendars.params" :on-change="refreshCalendars" />
        <ConfirmationModal
          v-if="calendars.delete"
          title="Calendar deletion"
          :onClose="hideDelete"
          :onConfirm="deleteCalendar"
          shown
        >
          Are you sure you want to remove calendar <b>{{ calendars.delete.displayName }}</b
          >?
        </ConfirmationModal>
      </div>
      <div class="col-6">
        <form @submit.prevent="submit">
          <h1 v-if="calendars.update">Edit</h1>
          <h1 v-else>New</h1>
          <Text
            class="mb-3"
            label="Display name"
            autoFocus
            v-model="model.model.displayName"
            required
            :error="model.error?.errors?.displayName"
          />
          <SpinButton
            class="btn-primary"
            :loading="model.submitting"
            text="Save"
            loadingText="Saving"
            @click="submit"
          />
        </form>
        <div v-if="calendars.update" class="table-responsive">
          <table class="table">
            <thead>
              <tr>
                <th>User</th>
                <th>Is Master</th>
                <th>Write</th>
                <th class="text-end p-1">
                  <button class="btn btn-sm btn-success" @click="addUsers">
                    <PlusLgIcon />
                  </button>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in calendars.update.users" :key="item.userId" class="align-middle">
                <td>{{ item.userName }}</td>
                <td>
                  <CheckLgIcon v-if="item.isMaster" />
                  <XLgIcon v-else class="text-danger" />
                </td>
                <td>
                  <CheckLgIcon v-if="item.canWrite" />
                  <XLgIcon v-else class="text-danger" />
                </td>
                <td class="text-end p-1">
                  <button
                    class="btn btn-sm btn-danger"
                    title="Delete"
                    @click="deleteCalendarUser(item)"
                  >
                    <XLgIcon />
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <GeneralModal
            v-if="calendars.showUsers"
            title="Allow user"
            :onClose="hideUsers"
            shown
            autoFocus
          >
            <template #body>
              <div class="d-flex flex-wrap">
                <Sizes
                  class="me-3 mb-2"
                  style="max-width: 8rem"
                  :params="users.params"
                  :on-change="refreshUsers"
                />
                <Search
                  autoFocus
                  class="me-3 mb-2"
                  style="max-width: 16rem"
                  placeholder="User name"
                  v-model="users.params.searchTerm"
                  :on-change="refreshUsers"
                />
              </div>
              <table class="table">
                <thead>
                  <tr>
                    <Header
                      :params="users.params"
                      :on-sort="refreshUsers"
                      column="name"
                      display="Name"
                    />
                    <Header
                      :params="users.params"
                      :on-sort="refreshUsers"
                      column="isMaster"
                      display="Is master"
                    />
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in users.items" :key="item.userId" class="align-middle">
                    <td>{{ item.userName }}</td>
                    <td>
                      <CheckLgIcon v-if="item.isMaster" class="text-success" />
                      <XLgIcon v-else class="text-danger" />
                    </td>
                    <td class="text-end p-1">
                      <div class="btn-group" role="group">
                        <button
                          class="btn btn-sm btn-outline-primary"
                          title="Delete"
                          @click="addCalendarUser(item, true)"
                        >
                          <PencilIcon />
                        </button>
                        <button
                          class="btn btn-sm btn-outline-success"
                          title="Delete"
                          @click="addCalendarUser(item, false)"
                        >
                          <BookIcon />
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
              <Pages :params="users.params" :on-change="refreshUsers" />
            </template>
            <template #footer>
              <button class="btn btn-outline-danger" @click="hideUsers">Close</button>
            </template>
          </GeneralModal>
        </div>
      </div>
    </div>
  </main>
</template>
