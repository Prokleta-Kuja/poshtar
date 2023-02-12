<script setup lang="ts">
import { reactive } from "vue";
import { DomainLM, DomainService } from "../api";
import DomainTable from "../components/DomainTable.vue"

const data = reactive<{ loaded?: boolean, domains: DomainLM[] }>({ domains: [] });
const refresh = () => {
    DomainService.getDomains().then(r => data.domains = r.items);
};

refresh();
</script>
<template>
    <DomainTable :domains="data.domains" text="Edit" color-class="btn-primary" :on-select="refresh" />
</template>