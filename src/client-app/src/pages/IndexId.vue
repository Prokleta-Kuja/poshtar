<script setup lang="ts">
// import { reactive } from "vue";
// import { ApiError, BadResponse, DomainByIdResponse, DomainService } from "../api/"

// const props = defineProps<{ id: number }>()
// const dat = reactive<{ loaded?: boolean, count: number, data?: DomainByIdResponse }>({ count: 0 });

// DomainService.getDomainById(props.id)
//     .then(r => dat.data = r)
//     .catch(r => {
//         dat.loaded = true;
//         if (r instanceof ApiError)
//             switch (r.status) {
//                 case 400:
//                     let e = r.body as BadResponse;
//                     break; // TODO: handle bad request
//                 case 404:
//                     break; // TODO: handle not found
//                 default: // TODO: handle server error
//                     console.log(r.status);
//                     break;
//             }
//     })
//     .finally(() => dat.loaded = true);

// const klik = () => { dat.count = dat.count + 1; }

</script>

<template>
    <!-- <h1 v-if="dat.data">{{ dat.data.name }}</h1>
    <h1 v-else>{{ $route.params.id }} - {{ props.id }}</h1>
    <h4 v-if="dat.loaded">Not loading</h4>
    <h4 v-else>Loading</h4>
    <button class="btn btn-primary" @click.prevent="klik">Klikni me {{ dat.count }}</button> -->
</template>