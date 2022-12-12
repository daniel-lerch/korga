# Korga

![Ubuntu build and tests](https://github.com/daniel-lerch/korga/workflows/Ubuntu%20build%20and%20tests/badge.svg)
[![](https://img.shields.io/docker/pulls/daniellerch/korga.svg)](https://hub.docker.com/r/daniellerch/korga)
[![](https://img.shields.io/docker/image-size/daniellerch/korga/latest.svg)](https://hub.docker.com/r/daniellerch/korga)

## What is Korga planned to be?

Korga stands for the german term _**K**irchen **Orga**nisation_ (church organization).
It is not supposed to be an all-in-one solution for churches but rather to fill the gaps between commercial church management solutions like Church.Tools
and several open source applications, while it itself only provides church-specific functionality.

## What can Korga already do?

![Three screenshots of Korga's event registration](docs/assets/event_registration_overview.png)

An event registration with multiple programs for each event. The list of participants is public as well as the possibility to delete registrations.

## Installation

The only officially supported distribution are Docker containers. An official image is available at [daniellerch/korga](https://hub.docker.com/r/daniellerch/korga).
If you are using Docker Compose, take a look our example compose file in the `docs` folder.

When you start Korga for the first time you have create the database schema via CLI:

```
./Korga.Server database create -fp
```

Currently, there is neither an API endpoint nor a graphical user interface available to edit events and programs. Instead you have to write SQL queries.

## Contributing

Contributions are highly welcome. Please open an issue before implementing a feature to discuss your plans.

Korga's source code is split into the backend (located in `server`) and the frontend (located in `webapp`).
The following instructions are written for Windows but generally also apply to Linux development setups.

### Backend
- Visual Studio 2022
- .NET SDK 7.0
- MySQL or MariaDB _(e.g. from [PSModules](https://github.com/daniel-lerch/psmodules))_
- OpenLDAP server

### Frontend
- Visual Studio Code
- Vue Language Features (Volar) Extension
- NodeJS 18 LTS

During development the frontend running on the Vue CLI development server will use _http://localhost:50805_ as API endpoint.
That means the backend can be running in Visual Studio with Debugger attached.

If you just want to work on the frontend you can also use a public test server by creating a file `webapp/.env.development.local`
to override the defaults with `VUE_APP_API_URL=https://lerchen.net/korga`.
Then you don't have to setup a database server, LDAP server and the ASP.NET Core backend.
