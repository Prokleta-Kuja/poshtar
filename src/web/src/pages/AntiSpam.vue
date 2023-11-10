<script setup lang="ts">
import { AntiSpamService, type AntiSpamSettings } from '@/api'
import CheckBox from '@/components/form/CheckBox.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import type IModelState from '@/components/form/modelState'
import { reactive } from 'vue'

const settings = reactive<IModelState<AntiSpamSettings>>({ model: {} })
const lists = reactive<{ asn: string; clients: string; dns: string }>({
  asn: '',
  clients: '',
  dns: ''
})
const load = () => {
  AntiSpamService.getAntiSpam()
    .then((r) => {
      settings.model = r
      lists.asn = r.asnBlocklist.join('\n')
      lists.clients = r.clientBlocklist.join('\n')
      // TODO: lists.dns =
    })
    .catch(() => {
      /* TODO: show error */
    })
}

const submit = () => {
  settings.submitting = true
  settings.error = undefined
  settings.model.asnBlocklist = lists.asn.split('\n')
  settings.model.clientBlocklist = lists.clients.split('\n')
  // TODO:settings.model.dnsBlocklist = lists.dns.split('\n')
  AntiSpamService.updateAntiSpam({ requestBody: settings.model })
    .catch((r) => (settings.error = r.body))
    .finally(() => (settings.submitting = false))
}

load()
</script>
<template>
  <main>
    <h1>Antispam Settings</h1>
    <form @submit.prevent="submit">
      <div class="row">
        <div class="col-4">
          <fieldset>
            <legend>General</legend>
            <IntegerBox
              class="mb-3"
              label="Tarpit time in seconds"
              v-model="settings.model.tarpitSeconds"
              required
              :error="settings.error?.errors?.tarpitSeconds"
            />
            <IntegerBox
              class="mb-3"
              label="Ban time in minutes"
              v-model="settings.model.banMinutes"
              required
              :error="settings.error?.errors?.banMinutes"
            />
            <CheckBox
              class="mb-3"
              label="Forward-confirmed reverse DNS Workarounds"
              v-model="settings.model.enableFCrDNSWorkarounds"
              :error="settings.error?.errors?.enableFCrDNSWorkarounds"
            />
          </fieldset>
          <fieldset>
            <legend c>Command failure</legend>
            <IntegerBox
              class="mb-3"
              label="Consecutive fail count"
              v-model="settings.model.consecutiveCmdFail"
              required
              :error="settings.error?.errors?.consecutiveCmdFail"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitConsecutiveCmdFail"
              :error="settings.error?.errors?.tarpitConsecutiveCmdFail"
              :disabled="(settings.model.consecutiveCmdFail ?? 0) <= 0"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banConsecutiveCmdFail"
              :error="settings.error?.errors?.banConsecutiveCmdFail"
              :disabled="(settings.model.consecutiveCmdFail ?? 0) <= 0"
            />
          </fieldset>
          <fieldset>
            <legend>Recipient failure</legend>
            <IntegerBox
              class="mb-3"
              label="Consecutive fail count"
              v-model="settings.model.consecutiveRcptFail"
              required
              :error="settings.error?.errors?.consecutiveCmdFail"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitConsecutiveRcptFail"
              :error="settings.error?.errors?.tarpitConsecutiveRcptFail"
              :disabled="(settings.model.consecutiveRcptFail ?? 0) <= 0"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banConsecutiveRcptFail"
              :error="settings.error?.errors?.banConsecutiveRcptFail"
              :disabled="(settings.model.consecutiveRcptFail ?? 0) <= 0"
            />
          </fieldset>
        </div>
        <div class="col-4"></div>
        <div class="col-4">
          <fieldset>
            <legend>Forward DNS check</legend>
            <CheckBox
              class="mb-3"
              inline
              label="Enforce"
              v-model="settings.model.enforceForwardDns"
              :error="settings.error?.errors?.enforceForwardDns"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitForwardDns"
              :error="settings.error?.errors?.tarpitForwardDns"
              :disabled="!settings.model.enforceForwardDns"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banForwardDns"
              :error="settings.error?.errors?.banForwardDns"
              :disabled="!settings.model.enforceForwardDns"
            />
          </fieldset>
          <fieldset>
            <legend>Reverse DNS check</legend>
            <CheckBox
              class="mb-3"
              inline
              label="Enforce"
              v-model="settings.model.enforceReverseDns"
              :error="settings.error?.errors?.enforceReverseDns"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitReverseDns"
              :error="settings.error?.errors?.tarpitReverseDns"
              :disabled="!settings.model.enforceReverseDns"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banReverseDns"
              :error="settings.error?.errors?.banReverseDns"
              :disabled="!settings.model.enforceReverseDns"
            />
          </fieldset>
          <fieldset>
            <legend>ASN block list check</legend>
            TODO: textarea
            <CheckBox
              class="mb-3"
              inline
              label="Enforce"
              v-model="settings.model.enforceAsnBlocklist"
              :error="settings.error?.errors?.enforceAsnBlocklist"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitAsnBlocklist"
              :error="settings.error?.errors?.tarpitAsnBlocklist"
              :disabled="!settings.model.enforceAsnBlocklist"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banAsnBlocklist"
              :error="settings.error?.errors?.banAsnBlocklist"
              :disabled="!settings.model.enforceAsnBlocklist"
            />
          </fieldset>
          <fieldset>
            <legend>Client block list check</legend>
            TODO: textarea
            <CheckBox
              class="mb-3"
              inline
              label="Enforce"
              v-model="settings.model.enforceClientBlocklist"
              :error="settings.error?.errors?.enforceClientBlocklist"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitClientBlocklist"
              :error="settings.error?.errors?.tarpitClientBlocklist"
              :disabled="!settings.model.enforceClientBlocklist"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banClientBlocklist"
              :error="settings.error?.errors?.banClientBlocklist"
              :disabled="!settings.model.enforceClientBlocklist"
            />
          </fieldset>
          <fieldset>
            <legend>TODO: DNS block list check</legend>
          </fieldset>
          <fieldset>
            <legend>SPF check</legend>
            <CheckBox
              class="mb-3"
              inline
              label="Enforce"
              v-model="settings.model.enforceSpf"
              :error="settings.error?.errors?.enforceSpf"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Tarpit"
              v-model="settings.model.tarpitSpf"
              :error="settings.error?.errors?.tarpitSpf"
              :disabled="!settings.model.enforceSpf"
            />
            <CheckBox
              class="mb-3"
              inline
              label="Ban"
              v-model="settings.model.banSpf"
              :error="settings.error?.errors?.banSpf"
              :disabled="!settings.model.enforceSpf"
            />
          </fieldset>
          <p v-if="settings.error" class="text-danger">{{ settings.error.message }}</p>
          <SpinButton
            class="btn-primary"
            :loading="settings.submitting"
            text="Save"
            loadingText="Saving"
            @click="submit"
          />
        </div>
      </div>
    </form>
  </main>
</template>
