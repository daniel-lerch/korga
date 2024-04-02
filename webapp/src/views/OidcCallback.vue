<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { useUserManagerStore } from "@/services/usermanager";

const store = useUserManagerStore();
const router = useRouter();

onMounted(() => {
  store.userManager.signinCallback();
  router.push("/distribution-lists");
});
</script>
