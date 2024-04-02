import { UserManager } from "oidc-client-ts";
import { defineStore } from "pinia";

export const useUserManagerStore = defineStore("usermanager", {
  state() {
    return {
      userManager: new UserManager({
        authority: "https://example.org/keycloak/realms/churchtools",
        client_id: "korga-non-confidential-dev",
        redirect_uri: "http://localhost:8080/oidc-callback",
      }),
    };
  },
});
