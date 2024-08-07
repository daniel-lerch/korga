# Korga

[![Build and tests](https://github.com/daniel-lerch/korga/actions/workflows/main.yml/badge.svg)](https://github.com/daniel-lerch/korga/actions/workflows/main.yml)
[![](https://img.shields.io/docker/pulls/daniellerch/korga.svg)](https://hub.docker.com/r/daniellerch/korga)
[![](https://img.shields.io/docker/image-size/daniellerch/korga/latest.svg)](https://hub.docker.com/r/daniellerch/korga)

Korga stands for the german term _**K**irchen **Orga**nisation_ (church organization).
It synchronizes people and groups with [ChurchTools](https://church.tools) and provides specialized features like email distribution lists.

## What can Korga already do?

### Email distribution lists

Once configured, Korga automatically synchronizes people and groups from ChurchTools. This data can be used to create distribution lists.
There is no Web UI available yet to manage distribution lists so you must stick to the CLI inside the Docker container:

```
./Korga dist create kids
./Korga dist add-recipient kids -g 137
```

This command creates a distribution list _kids@example.org_ which forwards emails to every member of group #137.

## Installation

The only officially supported distribution are Docker containers. An official image is available at [daniellerch/korga](https://hub.docker.com/r/daniellerch/korga).
If you are using Docker Compose, take a look our [example compose file](docs/compose.yaml) in the `docs` folder.

Korga has multiple modules that must be enabled via configuration to use them:
- ChurchTools sync
- Email delivery
- Email relay

Configuration can set as enviroment variables or by creating a custom config file.
I recommend to use environment variables and will explain them in the following sections.
However, if you prefer a config file, copy the default [appsettings.json](server/Korga/appsettings.json), edit it as required, and mount it at `/app/appsettings.json`.

### OpenID Connect authentication

Korga supports single sign-on using OpenID Connect.
It is the only authentication mechanism Korga supports.
If you do not configure OpenID Connect, you will not be able to login in Korga's Web UI.

First you must register a new client in your OpenID Connect Provider.

These instructions assume you use [Keycloak](https://www.keycloak.org/) either in standalone mode,
with storage provider like [canchanchara/keycloak-churchtools-storage-provider](https://github.com/canchanchara/keycloak-churchtools-storage-provider),
or with LDAP user federation with a wrapper like [milux/ctldap](https://github.com/milux/ctldap).

1. Login to Keycloak's admin console, select the correct realm and select _Clients_ in the left-hand navbar

2. Click on _Create client_, select _OpenID Connect_ as type and choose a client ID you like and click _Next_

3. Enable _Client authentication_ and disable _Direct access grants_ so that _Standard flow_ is the only enabled authentication flow and click _Next_

4. As _Root URL_ and _Home URL_ enter the URL to your Korga instance, e.g. https://example.org/korga. As _Valid redirect URIs_ and _Valid post logout redirect URIs_ enter a wildcard path, e.g. https://example.org/korga/*. _Web origins_ should be left empty. Then click _Save_

5. Finally go to the _Credentials_ tab of your newly created client and copy the _Client secret_

After creating a client for Korga in Keycloak or your OpenID Connect provider of choice, you can configure it via environment variables in `docker-compose.yml`.

- `OpenIdConnect__Authority`  
Set this to the root URL of your Keycloak realm, e.g. `https://keycloak.example.org/realms/churchtools`
- `OpenIdConnect__ClientId`  
Set this to the client ID you chose when creating a client for Korga in Keycloak, e.g. `korga`
- `OpenIdConnect__ClientSecret`  
Set this to the client secret copied in step 5.

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
- Personen & Gruppen > Gruppenmitgliedschaften aller sichtbaren Personen bearbeiten `churchdb:edit group memberships`
- Events > "Events" sehen `churchservice:view`
- Events > Dienste einzelner Dienstgruppen einsehen (Alle) `churchservice:view servicegroup(-1)`
- Events > Events von einzelnen Kalendern sehen (Alle) `churchservice:view events(-1)`

After creating and configuring a ChurchTools user for Korga you can finally configure it via environment variables in `docker-compose.yml`.

- `ChurchTools__EnableSync`  
Set this to `true` to enable a periodic sync with ChurchTools
- `ChurchTools__Host`  
Configure your ChurchTools domain, e.g. `example.church.tools`
- `ChurchTools__LoginToken`  
Fill in the login token of your service account for Korga.
This should be a 256 chars long alphanumeric text without special chars.

Do not forget to recreate your container to take these changes into effect.

### Email delivery

Email delivery via SMTP can be configured with environment variables in `docker-compose.yml`:

- `EmailDelivery__Enable`  
Set this to `true` to enable Korga to send emails via SMTP
- `EmailDelivery__SenderName`  
The display name for system messages. Defaults to `Korga`
- `EmailDelivery__SenderAddress`  
The address Korga will send emails from. Usually this should be the email address for the credentials below.
- `EmailDelivery__SmtpHost`  
Defaults to `smtp.strato.de`
- `EmailDelivery__SmtpPort`  
Defaults to `465`
- `EmailDelivery__SmtpUseSsl`  
Defaults to `true`
- `EmailDelivery__SmtpUsername`
- `EmailDelivery__SmtpPassword`

### Email relay

Korga's email relay requires a catchall IMAP inbox.
Such an inbox receives all emails sent to a domain where no matching inbox was found, i.e. `*@example.org`.
It can be configured with environment variables in `docker-compose.yml`:

- `EmailRelay__Enable`  
Set this to `true` to let Korga periodically check for emails to forward them. If `EmailDelivery__Enable` is set to `false`, this option will have no effect. Email relay depends on email delivery.
- `EmailRelay__ImapHost`  
Defaults to `imap.strato.de`
- `EmailRelay__ImapPort`  
Defaults to `993`
- `EmailRelay__ImapUseSsl`  
Defaults to `true`
- `EmailRelay__ImapUsername`
- `EmailRelay__ImapPassword`
- `EmailRelay__RetrievalIntervalInMinutes`  
Defaults to `2.0` (2 minutes)
- `EmailRelay__ImapRetentionIntervalInDays`  
Defaults to `1.0` (24 hours)
- `EmailRelay__MaxHeaderSizeInKilobytes`  
Defaults to `64` (64 KiB)
- `EmailRelay__MaxBodySizeInKilobytes`  
Defaults to `12288` (12 MiB). You must adjust MariaDB's `max_packet_size` if you want to increase this limit.

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
- NodeJS 20 LTS

During development the frontend running on the Vue CLI development server will use _http://localhost:10501_ as API endpoint.
That means the backend can be running in Visual Studio with Debugger attached.

If you just want to work on the frontend you can also use a public test server by creating a file `webapp/.env.development.local`
```env
VITE_API_ORIGIN=https://lerchen.net
VITE_API_BASE_PATH=/korga/
```
Then you don't have to setup a database server and the ASP.NET Core backend.
