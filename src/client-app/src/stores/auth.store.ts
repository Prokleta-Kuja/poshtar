import { defineStore } from "pinia";
import { ref } from "vue";
import { AuthService, AuthStatusModel } from "../api";

export const useAuth = defineStore("auth", () => {
  const initialized = ref(false);
  const isAuthenticated = ref(false);
  const username = ref<string | undefined | null>(undefined);

  const setLoginInfo = (info: AuthStatusModel) => {
    isAuthenticated.value = info.authenticated;
    username.value = info.username;
  };

  const clearLoginInfo = () => {
    isAuthenticated.value = false;
    username.value = "";
  };

  const initialize = () =>
    AuthService.status()
      .then((r) => {
        isAuthenticated.value = r.authenticated;
        username.value = r.username;
      })
      .finally(() => (initialized.value = true));

  return {
    isAuthenticated,
    username,
    initialized,
    setLoginInfo,
    clearLoginInfo,
    initialize,
  };
});
