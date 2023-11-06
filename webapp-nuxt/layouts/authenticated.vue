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
            <BNavItemDropdown text="Daniel">
              <BDropdownItem @click.prevent="logout">Abmelden</BDropdownItem>
            </BNavItemDropdown>
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
async function logout() {
  await $fetch('https://lerchen.net/korga/api/logout', { ignoreResponseError: true, credentials: 'include' })
}
</script>
