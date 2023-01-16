<script setup lang="ts">
import { reactive } from "vue";
import { DomainByIdResult, DomainService } from "../api/"

const dat = reactive<{ loading?: boolean, data?: DomainByIdResult }>({});

const load = async () => {
    // const kurac = await DomainService.domainById(1)
    // dat.data = kurac;
    DomainService.domainById(1)
        .then(r => dat.data = r)
        .catch(r => console.log(r));
}

load();
</script>

<template>
    <h1 v-if="dat.data">{{ dat.data.name }}</h1>
    <h1 v-else>{{ $route.params.id }}</h1>
</template>