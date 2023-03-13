<script setup lang="ts">
import { computed } from 'vue';
export interface IModal {
    title: string;
    width?: "sm" | "lg" | "xl";
    scrollable?: boolean;
    centered?: boolean;
    shown?: boolean;
}
const props = defineProps<IModal>()
const classModal = computed(() => ({ show: props.shown, 'd-none': !props.shown, 'd-block': props.shown }))
const classDialog = computed(() => ({
    'modal-sm': props.width === "sm",
    'modal-lg': props.width === "lg",
    'modal-xl': props.width === "xl",
    'modal-dialog-scrollable': props.scrollable,
    'modal-dialog-centered': props.centered,
}))

</script>
<template>
    <div class="modal fade" :class="classModal" tabindex="-1" role="dialog">
        <div class="modal-dialog" :class="classDialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">{{ props.title }}</h5>
                    <button type="button" class="btn-close"></button>
                </div>
                <div class="modal-body">
                    <slot name="body" />
                </div>
                <div class="modal-footer">
                    <slot name="footer" />
                </div>
            </div>
        </div>
    </div>
    <div class="modal-backdrop fade" :class="classModal"></div>
</template>