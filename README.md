# Korga

> [!IMPORTANT]
> This README reflect the current development state on `master` for Korga 4. See [Korga 3.1.3](https://github.com/daniel-lerch/korga/blob/v3.1.3/README.md) for the latest released version.

[![Build and tests](https://github.com/daniel-lerch/korga/actions/workflows/main.yml/badge.svg)](https://github.com/daniel-lerch/korga/actions/workflows/main.yml)
[![](https://img.shields.io/docker/pulls/daniellerch/korga.svg)](https://hub.docker.com/r/daniellerch/korga)
[![](https://img.shields.io/docker/image-size/daniellerch/korga/latest.svg)](https://hub.docker.com/r/daniellerch/korga)

Korga stands for the german term _**K**irchen **Orga**nisation_ (church organization).
It synchronizes people and groups with [ChurchTools](https://church.tools) and provides specialized features like email distribution lists.

## What can Korga already do?

### Email distribution lists

Korga makes it possible to send emails to ChurchTools groups via email.
For example, anyone can send an email to _youth@example.oorg_ and Korga will forward it to all members of your ChurchTools group _Youth_ with role _Leader_. 

There is no Web UI available yet to manage distribution lists so you must stick to the CLI inside the Docker container:

```
./Korga dist create youth
./Korga dist add-recipient youth -g 137
```

This command creates a distribution list _youth@example.org_ which forwards emails to every member of group #137.

## Installation

The only officially supported distribution are Docker containers. An official image is available at [daniellerch/korga](https://hub.docker.com/r/daniellerch/korga).
If you are using Docker Compose, take a look our [example compose file](docs/compose.yaml) in the `docs` folder.

Korga has multiple modules that must be enabled via configuration to use them:
- ChurchTools sync
- Email delivery
- Email relay

See [Korga server configuration](docs/configuration.md) for a full list of configuration options.

### OAuth authentication

Korga uses single sign-on via OAuth 2.0. It is the only authentication mechanism Korga supports and required to start the application.

These instructions assume you use ChurchTools as OAuth provider (supported since December 2024).
Other OAuth 2.0 and OpenID Connect identity providers should also work.

1. Login to ChurchTools as an administrator and open _System settings_ from the navbar

2. Go to _Integrations_ > _Login to third-party system_ and click on _Add OAuth-Client_

3. Choose a name (will be displayed to users when they log in) and enter the URL of your Korga instance as _Redirect-URI_ e.g. `https://korga.example.org/api/signin-oauth`

After creating an OAuth client for Korga in ChurchTools, you can configure it via [environment variables](docs/configuration.md#oauth) in `docker-compose.yml`.

### ChurchTools sync

First you must create a service account which Korga can use for API access to ChurchTools:

1. Create a person with email address in your ChurchTools instance  
   You might want to define a special status for API users
2. Invite that person to ChurchTools
3. Click on the invitation link and set a password
4. Open https://example.church.tools/api/whoami to look up the ID of the newly created user
5. Open https://example.church.tools/api/person/${ID}/logintoken to look up the long-term access token which will be used like an API key

Steps 4. and 5. can also be performed in the ChurchTools web interface: [Official Documentation](https://hilfe.church.tools/wiki/0/API%20Authentifizierung#logintoken)

> [!WARNING]
> For security reasons it is not recommended to let Korga use your ChurchTools admin account.

Grant the following permissions to Korga's user:

- Administration > Berechtigungen anpassen `churchcore:administer persons`
- Personen & Gruppen > "Personen & Gruppen" sehen `churchdb:view`
- Personen & Gruppen > Sicherheitslevel Personendaten (Stufe 1-3) `churchdb:security level person(1,2,3)`
- Personen & Gruppen > Alle Personen des jeweiligen Bereiches sichtbar machen (Alle) `churchdb:view alldata(-1)`
- Personen & Gruppen > Einzelne Gruppen inkl. der enthaltenen Personen sehen (gilt auch für versteckte Gruppen) (Alle) `churchdb:view group(-1)`
- Personen & Gruppen > Gruppenmitgliedschaften aller sichtbaren Personen bearbeiten `churchdb:edit group memberships`
- Events > "Events" sehen `churchservice:view`
- Events > Dienste einzelner Dienstgruppen einsehen (Alle) `churchservice:view servicegroup(-1)`
- Events > Events von einzelnen Kalendern sehen (Alle) `churchservice:view events(-1)`

After creating and configuring a ChurchTools user for Korga you can finally configure it via [environment variables](docs/configuration.md#churchtools) in `docker-compose.yml`.

Do not forget to recreate your container to take these changes into effect.

### Email delivery

Email delivery requires an SMTP server and credentials.
This is given for almost any email inbox.
Depending on your email hosting provider you might need an app password.
Email delivery can be configured via [environment variables](docs/configuration.md#email-delivery) in `docker-compose.yml`.

### Email relay

Korga's email relay requires a catchall IMAP inbox.
Such an inbox receives all emails sent to a domain where no matching inbox was found, i.e. `*@example.org`.
It can be configured via [environment variables](docs/configuration.md#email-relay) in `docker-compose.yml`.

## Contributing

Contributions are highly welcome. Please open an issue before implementing a feature to discuss your plans.

Korga's source code is split into the backend (located in `server`) and the frontend (located in `webapp`).
The following instructions are written for Windows but generally also apply to Linux development setups.

### Backend
- Visual Studio 2022
- .NET SDK 8.0
- EF Core CLI Tools _(e.g. `dotnet tool install -g dotnet-ef`)_
- MySQL or MariaDB _(e.g. from [PSModules](https://github.com/daniel-lerch/psmodules))_

### Frontend
- Visual Studio Code
- Vue Language Features (Volar) Extension
- NodeJS 22 LTS

During development the frontend running on the Vue CLI development server will use _http://localhost:10501_ as API endpoint.
That means the backend can be running in Visual Studio with Debugger attached.

If you just want to work on the frontend you can also use a public test server by creating a file `webapp/.env.development.local`
```env
VITE_API_ORIGIN=https://lerchen.net
VITE_API_BASE_PATH=/korga/
```
Then you don't have to setup a database server and the ASP.NET Core backend.
