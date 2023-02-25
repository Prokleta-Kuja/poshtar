<script setup lang="ts">
import { reactive } from 'vue';
import Text from '../../components/form/Text.vue';

const props = defineProps<{ id: number }>()
const state = reactive<{ text?: string }>({});
</script>
<template>
    <h1 class="display-6">Edit domain</h1>
    <Text :label="'Kurac'" :placeholder="'Tu upiši kurac'" autoFocus v-model="state.text" />
    <p>{{ state.text }}</p>
</template>
