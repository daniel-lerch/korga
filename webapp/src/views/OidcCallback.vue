<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import client from "@/services/client";

const router = useRouter();

onMounted(async () => {
  const userManager = await client.userManager();
  await userManager.signinCallback();
  router.push("/distribution-lists");
});
</script>
