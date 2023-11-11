<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ServiceName, ServiceRequestType, ServicesService, type ServiceResultModel } from '@/api'
import GeneralModal from './GeneralModal.vue'
import CircleFillIcon from './icons/CircleFillIcon.vue'

const shown = ref(false)
const color = reactive({
  'text-warning': true,
  'text-danger': false,
  'text-success': false
})
const statusResult = reactive<ServiceResultModel>({ success: false })
const commandResult = reactive<ServiceResultModel>({ success: false })
const toggle = () => (shown.value = !shown.value)
const exec = (command: ServiceRequestType) =>
  ServicesService.request({
    requestBody: {
      name: ServiceName.Dovecot,
      type: command
    }
  }).then((r) => {
    if (command === ServiceRequestType.Status) {
      statusResult.success = r.success
      statusResult.output = r.output
      statusResult.error = r.error
      color['text-warning'] = false
      color['text-danger'] = !r.success
      color['text-success'] = r.success
    } else {
      exec(ServiceRequestType.Status)
      commandResult.success = r.success
      commandResult.output = r.output
      commandResult.error = r.error
    }
  })

exec(ServiceRequestType.Status)
</script>
<template>
  <div class="nav-link py-2 px-0 px-lg-2 pointer" title="Dovecot status" @click="toggle">
    <CircleFillIcon :class="color" />
    <small class="d-lg-none ms-2">Dovecot status</small>
  </div>
  <GeneralModal title="Dovecot" width="sm" :onClose="toggle" :shown="shown">
    <template #body>
      <div class="d-flex justify-content-center mb-4">
        <div class="btn-group" role="group">
          <button type="button" class="btn btn-success" @click="exec(ServiceRequestType.Start)">
            Start
          </button>
          <button type="button" class="btn btn-warning" @click="exec(ServiceRequestType.Restart)">
            Restart
          </button>
          <button type="button" class="btn btn-danger" @click="exec(ServiceRequestType.Stop)">
            Stop
          </button>
        </div>
      </div>
      <p v-if="commandResult.output" class="text-success">{{ commandResult.output }}</p>
      <p v-if="commandResult.error" class="text-danger">{{ commandResult.error }}</p>
    </template>
    <template #footer>
      <div class="me-auto">
        <CircleFillIcon :class="color" />
        <small v-if="statusResult.success" class="ms-2">Running</small>
        <small v-else class="ms-2">Not Running</small>
      </div>
      <button class="btn btn-outline-danger" @click="toggle">Close</button>
    </template>
  </GeneralModal>
</template>
