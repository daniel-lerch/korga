<template>
  <NuxtLayout :name="store.profile === null ? 'default' : 'authenticated'">
    <NuxtPage />
  </NuxtLayout>
</template>

<script setup lang="ts">

const store = useProfileStore()
await store.init()

</script>
