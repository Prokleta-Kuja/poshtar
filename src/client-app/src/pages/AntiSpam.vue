<script setup lang="ts">
import { AntiSpamService, type AntiSpamSettings } from '@/api'
import CheckBox from '@/components/form/CheckBox.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
import type IModelState from '@/components/form/modelState'
import { reactive } from 'vue'

const settings = reactive<IModelState<AntiSpamSettings>>({ model: {} })
const load = () => {
  AntiSpamService.getAntiSpam()
    .then((r) => (settings.model = r))
    .catch(() => {
      /* TODO: show error */
    })
}

const submit = () => {
  settings.submitting = true
  settings.error = undefined
  AntiSpamService.updateAntiSpam({ requestBody: settings.model })
    .then((r) => {})
    .catch((r) => (settings.error = r.body))
    .finally(() => (settings.submitting = false))
}

load()
</script>
<template>
  <main>
    <h1>Antispam Settings</h1>
    <form @submit.prevent="submit">
      <fieldset class="row mb-3">
        <legend class="col-form-label">General</legend>
        <IntegerBox
          label="Tarpit time in seconds"
          v-model="settings.model.tarpitSeconds"
          required
          :error="settings.error?.errors?.tarpitSeconds"
        />
        <IntegerBox
          label="Ban time in minutes"
          v-model="settings.model.banMinutes"
          required
          :error="settings.error?.errors?.banMinutes"
        />
        <CheckBox
          label="Workarounds"
          v-model="settings.model.enableFCrDNSWorkarounds"
          :error="settings.error?.errors?.enableFCrDNSWorkarounds"
        />
      </fieldset>
      <fieldset class="row mb-3">
        <legend class="col-form-label">Command failure</legend>
        <IntegerBox
          label="Consecutive fail count"
          v-model="settings.model.consecutiveCmdFail"
          required
          :error="settings.error?.errors?.consecutiveCmdFail"
        />
        <CheckBox
          label="Tarpit"
          v-model="settings.model.tarpitConsecutiveCmdFail"
          :error="settings.error?.errors?.tarpitConsecutiveCmdFail"
          :disabled="(settings.model.consecutiveCmdFail ?? 0) <= 0"
        />
        <CheckBox
          label="Ban"
          v-model="settings.model.banConsecutiveCmdFail"
          :error="settings.error?.errors?.banConsecutiveCmdFail"
          :disabled="(settings.model.consecutiveCmdFail ?? 0) <= 0"
        />
      </fieldset>
      <fieldset class="row mb-3">
        <legend class="col-form-label">Recipient failure</legend>
        <IntegerBox
          label="Consecutive fail count"
          v-model="settings.model.consecutiveRcptFail"
          required
          :error="settings.error?.errors?.consecutiveCmdFail"
        />
        <CheckBox
          label="Tarpit"
          v-model="settings.model.tarpitConsecutiveRcptFail"
          :error="settings.error?.errors?.tarpitConsecutiveRcptFail"
          :disabled="(settings.model.consecutiveRcptFail ?? 0) <= 0"
        />
        <CheckBox
          label="Ban"
          v-model="settings.model.banConsecutiveRcptFail"
          :error="settings.error?.errors?.banConsecutiveRcptFail"
          :disabled="(settings.model.consecutiveRcptFail ?? 0) <= 0"
        />
      </fieldset>
    </form>
  </main>
</template>
