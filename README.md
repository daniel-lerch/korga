# Korga

![Ubuntu build and tests](https://github.com/daniel-lerch/korga/workflows/Ubuntu%20build%20and%20tests/badge.svg)
[![](https://img.shields.io/docker/pulls/daniellerch/korga.svg)](https://hub.docker.com/r/daniellerch/korga)
[![](https://img.shields.io/docker/image-size/daniellerch/korga/latest.svg)](https://hub.docker.com/r/daniellerch/korga)

Korga stands for the german term _**K**irchen **Orga**nisation_ (church organization).
It synchronizes people and groups with [ChurchTools](https://church.tools) and provides specialized features like email distribution lists.

## What can Korga already do?

### Email distribution lists

Once configured, Korga automatically synchronizes people and groups from ChurchTools. This data can be used to create distribution lists.
There is no Web UI available yet to manage distribution lists so you must stick to the CLI inside the Docker container:

```
./Korga.Server distribution-list create --group 137 kids
```

This command creates a distribution list _kids@example.org_ which forwards emails to every member of group #137.

### Event registration

![Three screenshots of Korga's event registration](docs/assets/event_registration_overview.png)

An event registration with multiple programs for each event. The list of participants is public as well as the possibility to delete registrations.

Currently, there is neither an API endpoint nor a graphical user interface available to edit events and programs. Instead you have to write SQL queries.

## Installation

The only officially supported distribution are Docker containers. An official image is available at [daniellerch/korga](https://hub.docker.com/r/daniellerch/korga).
If you are using Docker Compose, take a look our [example compose file](docs/docker-compose.yml) in the `docs` folder.

Korga has multiple modules that must be enabled via configuration to use them:
- ChurchTools sync
- Email delivery
- Email relay

Configuration can set as enviroment variables or by creating a custom config file.
I recommend to use environment variables and will explain them in the following sections.
However, if you prefer a config file, copy the default [appsettings.json](server/src/Korga.Server/appsettings.json), edit it as required, and mount it at `/app/appsettings.json`.

### ChurchTools sync

First you must create a service account which Korga can use for API access to ChurchTools:

1. Create a person with email address in your ChurchTools instance  
   You might want to define a special status for API users
2. Invite that person to ChurchTools
3. Click on the invitation link and set a password
4. Open https://example.church.tools/api/whoami to look up the ID of the newly created user
5. Open https://example.church.tools/api/person/${ID}/logintoken to look up the long-term access token which will be used like an API key

Steps 4. and 5. can also be performed in the ChurchTools web interface: [Official Documentation](https://hilfe.church.tools/wiki/0/API%20Authentifizierung#logintoken)

> ⚠️ For security reasons it is not recommended to let Korga use your ChurchTools admin account.

Grant the following permissions to Korga's user:

- Administration > Berechtigungen anpassen `churchcore:administer persons`
- Personen & Gruppen > "Personen & Gruppen" sehen `churchdb:view`
- Personen & Gruppen > Sicherheitslevel Personendaten (Stufe 1-3) `churchdb:security level person(1,2,3)`
- Personen & Gruppen > Alle Personen des jeweiligen Bereiches sichtbar machen (Alle) `churchdb:view alldata(-1)`
- Personen & Gruppen > Einzelne Gruppen inkl. der enthaltenen Personen sehen (gilt auch für versteckte Gruppen) (Alle) `churchdb:view group(-1)`

After creating and configuring a ChurchTools user for Korga you can finally configure it via environment variables in `docker-compose.yml`.

- `ChurchTools__EnableSync`  
Set this to `true` to enable a periodic sync with ChurchTools
- `ChurchTools__Host`  
Configure your ChurchTools domain, e.g. `example.church.tools`
- `ChurchTools__LoginToken`  
Fill in the login token of your service account for Korga.
This should be a 256 chars long alphanumeric text without special chars.

Do not forget to recreate your container to take these changes into effect.

## Contributing

Contributions are highly welcome. Please open an issue before implementing a feature to discuss your plans.

Korga's source code is split into the backend (located in `server`) and the frontend (located in `webapp`).
The following instructions are written for Windows but generally also apply to Linux development setups.

### Backend
- Visual Studio 2022
- .NET SDK 7.0
- EF Core CLI Tools _(e.g. `dotnet tool install -g dotnet-ef`)_
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
