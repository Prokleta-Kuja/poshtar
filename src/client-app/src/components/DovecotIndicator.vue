<script setup lang="ts">
import { reactive } from 'vue';
import { ServiceName, ServiceRequestType, ServicesService, type ServiceResultModel } from '@/api';

const color = reactive({
    'text-warning': true,
    'text-danger': false,
    'text-success': false,
})
const result = reactive<ServiceResultModel>({ success: false });

const getStatus = () => ServicesService.request({
    requestBody: {
        name: ServiceName.Dovecot,
        type: ServiceRequestType.Status,
    }
}).then(r => {
    result.success = r.success;
    result.output = r.output;
    result.error = r.error;
    color['text-warning'] = false;
    color['text-danger'] = !r.success;
    color['text-success'] = r.success;
});

getStatus();
</script>
<template>
    <div class="nav-link py-2 px-0 px-lg-2 pointer" title="Dovecot status">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-circle-fill"
            :class="color" viewBox="0 0 16 16">
            <circle cx="8" cy="8" r="8" />
        </svg>
        <small class="d-lg-none ms-2">Dovecot status</small>
    </div>
</template>