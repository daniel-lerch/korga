# Korga

![Ubuntu build and tests](https://github.com/daniel-lerch/korga/workflows/Ubuntu%20build%20and%20tests/badge.svg)

> ⚠ This project is work in progress and far away from being usable. For more information see _roadmap_ below.

## What is Korga planned to be?

Korga stands for the german term _**K**irchen **Orga**nisation_ (church organization).
Unlike all other church management systems known to the author, Korga is not designed to be an all-in-one solution for churches.
It is rather the link between several open source applications, while it itself only provides church-specific functionality.

| Feature | Applications |
|---|---|
| Account management | OpenLDAP,<br>Korga (OpenID Connect) |
| People, Groups, Mailing lists | Korga |
| File sharing | Nextcloud,<br>ownCloud |
| Chat | Synapse (Matrix),<br>Rocket.Chat,<br>Nextcloud Talk |
| Website | WordPress |

## Installation

The only officially supported distribution are Docker containers. As there have not been releases so far you have to build the container locally:

```
$ docker build -t daniel-lerch/korga https://github.com/daniel-lerch/korga.git
```

If you are using Docker Compose, take a look our example compose file in the `docs` folder.

## Contributing

Contributions are highly welcome. Please open an issue before implementing a feature to discuss your plans.

Korga's source code is split into the backend (located in `server`) and the frontend (located in `webapp`).
The following instructions are written for Windows but generally also apply to Linux development setups.

### Backend
- Visual Studio 2022
- .NET SDK 6.0
- MySQL or MariaDB _(e.g. from [PSModules](https://github.com/daniel-lerch/psmodules))_
- OpenLDAP server

### Frontend
- Visual Studio Code
- Vetur Extension
- NodeJS 16 LTS

During development the frontend running on the Vue CLI development server will use _http://localhost:50805_ as API endpoint.
That means the backend can be running in Visual Studio with Debugger attached.

If you just want to work on the frontend you can also use a public test server by creating a file `webapp/.env.development.local`
to override the defaults with `VUE_APP_API_URL=https://lerchen.net/korga`.
Then you don't have to setup a database server, LDAP server and the ASP.NET Core backend.
