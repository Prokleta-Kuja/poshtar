<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
export interface IAreaBox {
  label?: string
  name?: string
  autoFocus?: boolean
  required?: boolean
  placeholder?: string
  rows?: number
  autoComplete?: 'off' | 'username' | 'password' | 'one-time-code'
  modelValue?: string
  help?: string
  error?: string
}

const el = ref<HTMLInputElement | null>(null)
const state = reactive<{ id: string }>({ id: crypto.randomUUID() })
const props = defineProps<IAreaBox>()
const emit = defineEmits<{ (e: 'update:modelValue', modelValue?: string): void }>()

const update = (e: Event) => {
  const input = e.target as HTMLInputElement
  emit('update:modelValue', input.value)
}
onMounted(() => {
  if (props.autoFocus) el.value?.focus()
})
</script>
<template>
  <div>
    <label v-if="label" :for="state.id" class="form-label"
      >{{ label }} <span v-if="required">*</span></label
    >
    <textarea
      ref="el"
      class="form-control"
      :id="state.id"
      :class="{ 'is-invalid': error }"
      :placeholder="placeholder"
      :value="modelValue"
      @input="update"
      :required="required"
      :autocomplete="autoComplete"
      :name="name"
      :rows="rows"
    ></textarea>
    <div v-if="error" class="invalid-feedback">{{ error }}</div>
    <div v-else-if="help" class="form-text">{{ help }}</div>
  </div>
</template>
