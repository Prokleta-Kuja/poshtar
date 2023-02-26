<script setup lang="ts">
import { onMounted, ref } from 'vue';

const el = ref<HTMLInputElement | null>(null);
const props = defineProps<{ autoFocus?: boolean, modelValue?: string, onChange: () => void }>();
const emit = defineEmits<{ (e: 'update:modelValue', modelValue?: string): void }>()

const clear = () => { emit('update:modelValue', undefined); props.onChange(); }
const search = (e: Event) => {
    const input = e.target as HTMLInputElement;
    emit('update:modelValue', input.value);
    props.onChange();
}
onMounted(() => {
    if (props.autoFocus)
        el.value?.focus()
})
</script>
<template>
    <div>
        <label for="search" class="form-label">Search</label>
        <div class="input-group">
            <input ref="el" class="form-control" id="search" placeholder="Name, Host" :value="props.modelValue"
                @keyup.enter="search" type="search">
            <button class="btn btn-outline-danger" type="button" @click.prevent="clear">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-lg"
                    viewBox="0 0 16 16">
                    <path
                        d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z" />
                </svg>
            </button>
        </div>
    </div>
</template>