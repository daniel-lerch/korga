<template>
  <div>
    <header>
      <BNavbar toggleable="md" class="navbar-dark bg-primary">
        <BNavbarBrand to="/">Korga</BNavbarBrand>
        <BNavbarToggle target="nav-collapse" />
        <BCollapse id="nav-collapse" is-nav>
          <BNavbarNav>
            <BNavItem to="/events">Events</BNavItem>
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
