<template>
  <div>
    <header>
      <BNavbar toggleable="md" class="navbar-dark bg-primary">
        <BNavbarBrand to="/">Korga</BNavbarBrand>
        <BNavbarToggle target="nav-collapse" />
        <BCollapse id="nav-collapse" is-nav>
          <BNavbarNav class="me-auto">
            <BNavItem to="/events">Events</BNavItem>
          </BNavbarNav>
          <BNavbarNav>
            <BButton variant="outline-light" @click.prevent="login">
              Login
            </BButton>
          </BNavbarNav>
        </BCollapse>
      </BNavbar>
    </header>
    <main>
      <slot />
    </main>
    <footer>
      <div>
        <small>
          Copyright &copy; 2022-2023 Daniel Lerch and Benjamin Stieler
        </small>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">

interface UnauthorizedResponse {
  openIdConnectRedirectUrl: string;
}

async function login() {
  const response = await $fetch<UnauthorizedResponse>('https://lerchen.net/korga/api/challenge', { ignoreResponseError: true, credentials: 'include' })
  window.location.href = response.openIdConnectRedirectUrl
}
</script>
