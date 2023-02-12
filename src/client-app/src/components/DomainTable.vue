<script setup lang="ts">
import { DomainLM } from "../api";
defineProps<{ domains: DomainLM[], onSelect: () => void, text: string, colorClass: string }>();
</script>
<template>
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Host</th>
                <th>Users</th>
                <th>Addresses</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="domain in domains" :key="domain.id">
                <td>{{ domain.name }}</td>
                <td>{{ domain.host }}</td>
                <td>{{ domain.userCount }}</td>
                <td>{{ domain.addressCount }}</td>
                <td><button class="btn btn-sm" :class="colorClass" @click="onSelect">{{ text }}</button></td>
            </tr>
        </tbody>
    </table>
</template>