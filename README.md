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

## Roadmap

Person API
- [x] List
- [x] Details
- [ ] Create
- [ ] Modify
- [ ] Delete
- [ ] LDAP person integration

Group API
- [x] List
- [x] Details
- [ ] Create
- [ ] Modify
- [ ] Delete
- [ ] LDAP group integration

Documentation
- [ ] Setup guide
- [ ] User guide 
- ...

Server
- [x] Project creation
- [x] Basic database model
- [x] Changelog for important entities
- [ ] LDAP integration
- [ ] Permission model
- [ ] Identity provider
- [ ] Mailing lists
- ...

Client
- [x] Project creation
- [ ] People view
- [ ] Group view
- [ ] Login
- [ ] PWA support
- ...
