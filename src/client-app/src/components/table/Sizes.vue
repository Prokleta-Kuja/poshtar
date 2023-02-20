<script setup lang="ts">
import { ref } from 'vue';
import IListResponse from '../../interfaces/IListResponse';

const props = defineProps<{ all?: boolean, params: IListResponse, onChange: (params: IListResponse) => void }>();
const current = ref(props.params.size);
const sizes = [10, 25, 50, 100];
const change = () => {
    props.params.size = current.value;
    props.onChange(props.params);
}
</script>
<template>
    <div>
        <select class="form-select" v-model="current" @change="change">
            <option v-for="size in sizes" :key="size" :value="size">{{ size }}</option>
            <option v-if="props.all" value="0">All</option>
        </select>
        <span>&nbsp;per page</span>
    </div>
</template>