<script setup lang="ts">
import { ref } from 'vue';
import Modal from '../components/Modal.vue';

const shown = ref(false)
const toggle = () => shown.value = !shown.value
</script>
<template>
    <h1>Home</h1>
    <button class="btn btn-primary" @click="toggle">kita</button>
    <Modal title="kita" :shown="shown">
        <template #body>Moje tijelo</template>
        <template #footer>
            <button @click="toggle">Kita</button>
        </template>
    </Modal>
</template>