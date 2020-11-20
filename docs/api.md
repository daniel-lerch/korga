# Documentation of the Korga HTTP API
This API is intended for internal use and might change without notice at any time.

## Person
### `GET /api/people`
Returns a list of people. Query parameters:
- `offset` Start index of results
- `count` Limit number of results to this value

Response body: `PersonResponse`
```json
[
  {
    "id": 1,
    "givenName": "Karl-Heinz",
    "familyName": "Günther",
    "mailAddress": "gunther@example.com"               
  },
  {
    "id": 2,
    "givenName": "Max",
    "familyName": "Mustermann",
    "mailAddress": "max.mustermann@example.com"
  }
]
```

### `GET /api/person/{id}`
Returns detailed information about a single person.

Response body: `PersonResponse2`
```json
{
  "id": 2,
  "version": 2,
  "givenName": "Max",
  "familyName": "Mustermann",
  "mailAddress": "mustermann@example.com",
  "memberships": [
    {
      "id": 3,
      "roleId": 1,
      "roleName": "Leiter",
      "groupId": 1,
      "groupName": "Jugend",
      "creationTime": "2020-11-16T17:20:16.8506212+01:00",
      "createdBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      },
      "deletionTime": null,
      "deletedBy": null
    }
  ],
  "creationTime": "2020-09-25T16:34:15.6167655+02:00",
  "createdBy": {
    "id": 1,
    "givenName": "Karl-Heinz",
    "familyName": "Günther",
    "mailAddress": "gunther@example.com"
  },
  "deletionTime": null,
  "deletedBy": null,
  "history": [
    {
      "version": 1,
      "givenName": "Max",
      "familyName": "Mustermann",
      "mailAddress": null,
      "overrideTime": "2020-09-25T18:35:34.6606819+02:00",
      "overriddenBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      }
    }
  ]
}
```

### `POST /api/person/new`
Creates a new person.
```json
{
  "givenName": "Max",
  "familyName": "Mustermann",
  "mailAddress": "max.mustermann@example.com"
}
```

Response body: `PersonResponse2`

### `PUT /api/person/{id}`
Updates an existing person if a value is different.
```json
{
  "givenName": "Max",
  "familyName": "Mustermann",
  "mailAddress": "mustermann@example.com"
}
```

Response body: `PersonResponse2`

### `DELETE /api/person/{id}`
Marks an exisiting person as deleted. The actual data will be deleted according to retention policies.

Response body: `PersonRequest2`

---

## Group
### `GET /api/groups`
Returns a list of groups

Response body: `GroupResponse`
```json
[
  {
    "id": 1,
    "name": "Jugend",
    "description": "Gruppe für Jugendliche ab 14 Jahren",
    "roles": [
      {
        "id": 1,
        "name": "Teilnehmer"
      },
      {
        "id": 2,
        "name": "Leiter"
      }
    ],
    "memberCount": 2
  }
]
```

### `GET /api/group/{id}`
Returns detailed information about a single group

Response body: `GroupResponse2`
```json
{
  "id": 1,
  "version": 2,
  "name": "Jugend",
  "description": "Gruppe für Jugendliche ab 14 Jahren",
  "roles": [
    {
      "id": 1,
      "version": 1,
      "name": "Teilnehmer",
      "createdBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      },
      "creationTime": "2020-09-30T10:15:31.0829675+02:00",
      "deletionTime": null,
      "deletedBy": null,
      "history": []
    },
    {
      "id": 2,
      "version": 2,
      "name": "Leiter",
      "createdBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      },
      "creationTime": "2020-09-30T10:15:31.0829675+02:00",
      "deletionTime": null,
      "deletedBy": null,
      "history": [
        {
          "version": 1,
          "name": "Leitung",
          "overrideTime": "2020-09-30T10:52:45.6063725+02:00",
          "overriddenBy": {
            "id": 1,
            "givenName": "Karl-Heinz",
            "familyName": "Günther",
            "mailAddress": "gunther@example.com"               
          }
        }
      ]
    }
  ],
  "members": [
    {
      "person": {
        "id": 2,
        "givenName": "Max",
        "familyName": "Mustermann",
        "mailAddress": "mustermann@example.com"
      },
      "roleId": 1,
      "creationTime": "",
      "createdBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      },
      "deletionTime": null,
      "deletedBy": null
    },
    {
      "person": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      },
      "roleId": 2,
      "creationTime": "",
      "createdBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      },
      "deletionTime": null,
      "deletedBy": null
    }
  ],
  "creationTime": "2020-09-30T10:15:31.0829675+02:00",
  "createdBy": {
    "id": 1,
    "givenName": "Karl-Heinz",
    "familyName": "Günther",
    "mailAddress": "gunther@example.com"               
  },
  "deletionTime": null,
  "deletedBy": null,
  "history": [
    {
      "version": 1,
      "name": "Jugend",
      "description": null,
      "overrideTime": "2020-09-20T10:25:45.5893077+02:00",
      "overriddenBy": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "gunther@example.com"               
      }
    }
  ]
}
```
