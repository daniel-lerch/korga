<template>
  <div>
    <header>
      <nav class="navbar navbar-expand-lg navbar-light bg-light">
        <router-link to="/" class="navbar-brand">Korga</router-link>
        <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navbarSupportedContent">
          <ul class="navbar-nav">
            <li class="nav-item">
              <router-link to="/" class="nav-link">Home</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/about" class="nav-link">About</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/people" class="nav-link">People</router-link>
            </li>
          </ul>
        </div>
      </nav>
    </header>
    <router-view />
  </div>
</template>

<style>
/* Apply Bootstrap active link design to active router-link */
.navbar-light .navbar-nav a.router-link-exact-active {
  color: rgba(0,0,0,.9)!important;
}
</style>
