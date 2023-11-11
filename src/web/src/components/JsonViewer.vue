<script setup lang="ts">
import { ref } from 'vue'
import GeneralModal from '@/components/GeneralModal.vue'
import FileTypeJsonIconVue from './icons/FileTypeJsonIcon.vue'

const props = defineProps<{ json: string }>()
const formated = ref<string>()

const show = () => {
  const parsed = JSON.parse(props.json)
  formated.value = JSON.stringify(parsed, null, 2)
}
const hide = () => {
  formated.value = undefined
}
</script>
<template>
  <button class="btn btn-sm btn-primary" @click="show" title="View">
    <FileTypeJsonIconVue />
  </button>
  <GeneralModal v-if="formated" title="Data" shown :onClose="hide">
    <template #body>
      <pre>{{ formated }}</pre>
    </template>
    <template #footer>
      <button class="btn btn-outline-danger" @click="hide">Close</button>
    </template>
  </GeneralModal>
</template>
