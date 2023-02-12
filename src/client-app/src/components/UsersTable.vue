<script setup lang="ts">
import { UserLM } from "../api";
defineProps<{ users: UserLM[], onSelect: () => void, text: string, colorClass: string }>();
</script>
<template>
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Description</th>
                <th>Master</th>
                <th>Quota</th>
                <th>Addresses</th>
                <th>Domains</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="user in users" :key="user.id">
                <td>{{ user.name }}</td>
                <td>{{ user.host }}</td>
                <td>{{ user.userCount }}</td>
                <td>{{ user.addressCount }}</td>
                <td><button class="btn btn-sm" :class="colorClass" @click="onSelect">{{ text }}</button></td>
            </tr>
        </tbody>
    </table>
</template>