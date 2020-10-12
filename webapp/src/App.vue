<template>
  <header>
    <nav class="navbar navbar-expand-lg navbar-light bg-light">
      <router-link to="/" class="navbar-brand">Korga</router-link>
      <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarSupportedContent">
        <ul class="navbar-nav">
          <li class="nav-item">
            <router-link :to="{ name: 'Home' }" class="nav-link">Home</router-link>
          </li>
          <li class="nav-item">
            <router-link :to="{ name: 'About' }" class="nav-link">About</router-link>
          </li>
          <li class="nav-item">
            <router-link :to="{ name: 'People' }" class="nav-link">People</router-link>
          </li>
        </ul>
      </div>
    </nav>
  </header>
  <main class="content">
    <router-view v-bind="$attrs"/>
  </main>
  <footer class="bg-light">
    <div>Copyright &copy; 2020 Daniel Lerch and <a href="https://github.com/daniel-lerch/korga/graphs/contributors">others</a></div>
  </footer>
</template>

<style>
div#app {
  display: flex;
  flex-flow: column;
  height: 100vh;
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
.navbar-light .navbar-nav a.router-link-exact-active {
  color: rgba(0,0,0,.9)!important;
}

.page-load-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
}

.page-load-container .spinner-border {
  width: 3rem;
  height: 3rem;
}
</style>
