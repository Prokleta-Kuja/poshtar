import { defineStore } from 'pinia'
import { ref } from 'vue'
import { AuthService, type AuthStatusModel } from '@/api'
import { useRouter } from 'vue-router'

export const useAuth = defineStore('auth', () => {
  const router = useRouter()
  const initialized = ref(false)
  const isAuthenticated = ref(false)
  const username = ref<string | undefined | null>(undefined)

  const setLoginInfo = (info: AuthStatusModel) => {
    isAuthenticated.value = info.authenticated
    username.value = info.username
    setExpire(info.expires)
  }

  const clearLoginInfo = () => {
    isAuthenticated.value = false
    username.value = ''
  }

  const initialize = () =>
    AuthService.status()
      .then((r) => {
        isAuthenticated.value = r.authenticated
        username.value = r.username
        setExpire(r.expires)
      })
      .finally(() => (initialized.value = true))

  const setExpire = (dateTime: string | null | undefined) => {
    if (!dateTime) return

    const dt = new Date(dateTime)
    const time = dt.getTime() - Date.now()
    if (time > 0) setTimeout(onExpire, time)
    else onExpire()
  }

  const onExpire = () => router.push({ name: 'route.logout' })

  return {
    isAuthenticated,
    username,
    initialized,
    setLoginInfo,
    clearLoginInfo,
    initialize
  }
})
