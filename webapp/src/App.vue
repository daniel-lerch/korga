<template>
  <header>
    <nav class="navbar navbar-expand-md navbar-dark bg-primary">
      <div class="container-fluid">
        <router-link to="/events" class="navbar-brand">Korga</router-link>
        <button
          class="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarSupportedContent"
        >
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navbarSupportedContent">
          <ul class="navbar-nav">
            <li class="nav-item">
              <router-link :to="{ name: 'Events' }" class="nav-link"
                >Events</router-link
              >
            </li>
            <!-- <li class="nav-item">
            <router-link :to="{ name: 'Admin' }" class="nav-link"
              >Admin</router-link
            >
          </li> -->
          </ul>
        </div>
      </div>
    </nav>
  </header>
  <main class="content">
    <router-view v-bind="$attrs" />
  </main>
  <footer>
    <div>
      <small
        >Copyright &copy; 2022-2023 Daniel Lerch and Benjamin Stieler
      </small>
    </div>
  </footer>
</template>

<style>
div#app {
  display: flex;
  flex-flow: column;
  min-height: 100vh;
}

main.content {
  flex: 1;
  overflow: auto;
}

footer {
  min-height: 40px;
  padding: 8px 16px;
}

/* Apply Bootstrap active link design to active router-link */
.navbar-light .navbar-nav a.router-link-active {
  color: rgba(0, 0, 0, 0.9) !important;
}

.navbar-dark .navbar-nav a.router-link-active {
  color: #fff !important;
}

a.subdued {
  color: inherit;
  text-decoration-line: none;
}

a.subdued:hover {
  text-decoration-line: underline;
}

.page-load-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
  animation: fadein 1.4s;
}

.page-loaded-container {
  animation: fadein 0.7s;
}

.page-load-container .spinner-border {
  width: 3rem;
  height: 3rem;
}

@keyframes fadein {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
