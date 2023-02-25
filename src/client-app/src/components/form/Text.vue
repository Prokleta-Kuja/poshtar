<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';

const el = ref<HTMLInputElement | null>(null);
const state = reactive<{ id: string }>({ id: crypto.randomUUID() });
const props = defineProps<{ label?: string, autoFocus?: boolean, modelValue?: string, placeholder?: string }>();
const emit = defineEmits<{ (e: 'update:modelValue', modelValue?: string): void }>()

const update = (e: Event) => {
    const input = e.target as HTMLInputElement;
    if (input)
        emit('update:modelValue', input.value);
}
onMounted(() => {
    if (props.autoFocus)
        el.value?.focus();
})
</script>
<template>
    <div>
        <label v-if="props.label" :for="state.id" class="form-label">{{ props.label }}</label>
        <input ref="el" class="form-control" :id="state.id" :placeholder="props.placeholder" :value="props.modelValue"
            @input="update">
        <div id="emailHelp" class="form-text">Napravi help i error</div>
    </div>
</template>