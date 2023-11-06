<template>
  <NuxtLayout :name="profile === null ? 'default' : 'authenticated'">
    <NuxtPage />
  </NuxtLayout>
</template>

<script setup lang="ts">

interface ProfileResponse {
  id: string;
  givenName: string;
  familyName: string;
  emailAddress: string;
}

const { data: profile } = useFetch<ProfileResponse | null>('https://lerchen.net/korga/api/profile', { credentials: 'include' })

</script>
