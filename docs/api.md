# Documentation of the Korga HTTP API
This API is intended for internal use and might change without notice at any time.

## Person
### `GET /api/people`
Returns a list of people. Query parameters:
- `offset` Start index of results
- `count` Limit number of results to this value
```json
[
  {
    "id": 1,
    "givenName": "Karl-Heinz",
    "familyName": "Günther",
    "mailAddress": "karl-heinz.guenther@example.com"               
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
```json
{
  "id": 2,
  "version": 2,
  "givenName": "Max",
  "familyName": "Mustermann",
  "mailAddress": "max.mustermann@example.com",
  "memberships": {
    "group": {
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
    },
    "roleId": 1,
    "creationTime": "",
    "creator": {
      "id": 1,
      "givenName": "Karl-Heinz",
      "familyName": "Günther",
      "mailAddress": "karl-heinz.guenther@example.com"               
    },
    "deletionTime": null,
    "deletor": null
  },
  "creationTime": "2020-09-25T16:34:15.6167655+02:00",
  "creator": {
    "id": 1,
    "givenName": "Karl-Heinz",
    "familyName": "Günther",
    "mailAddress": "karl-heinz.guenther@example.com"
  },
  "deletionTime": null,
  "deletor": null,
  "history": [
    {
      "version": 1,
      "givenName": "Max",
      "familyName": "Mustermann",
      "mailAddress": null,
      "editTime": "2020-09-25T18:35:34.6606819+02:00",
      "editor": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      }
    }
  ]
}
```

## Group
### `GET /api/groups`
Returns a list of groups
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
      "creator": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      },
      "creationTime": "2020-09-30T10:15:31.0829675+02:00",
      "deletionTime": null,
      "deletor": null,
      "history": []
    },
    {
      "id": 2,
      "version": 2,
      "name": "Leiter",
      "creator": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      },
      "creationTime": "2020-09-30T10:15:31.0829675+02:00",
      "deletionTime": null,
      "deletor": null,
      "history": [
        {
          "version": 1,
          "name": "Leitung",
          "editTime": "2020-09-30T10:52:45.6063725+02:00",
          "editor": {
            "id": 1,
            "givenName": "Karl-Heinz",
            "familyName": "Günther",
            "mailAddress": "karl-heinz.guenther@example.com"               
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
        "mailAddress": "max.mustermann@example.com"
      },
      "roleId": 1,
      "creationTime": "",
      "creator": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      },
      "deletionTime": null,
      "deletor": null
    },
    {
      "person": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      },
      "roleId": 2,
      "creationTime": "",
      "creator": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      },
      "deletionTime": null,
      "deletor": null
    }
  ],
  "creationTime": "2020-09-30T10:15:31.0829675+02:00",
  "creator": {
    "id": 1,
    "givenName": "Karl-Heinz",
    "familyName": "Günther",
    "mailAddress": "karl-heinz.guenther@example.com"               
  },
  "deletionTime": null,
  "deletor": null,
  "history": [
    {
      "version": 1,
      "name": "Jugend",
      "description": null,
      "editTime": "2020-09-20T10:25:45.5893077+02:00",
      "editor": {
        "id": 1,
        "givenName": "Karl-Heinz",
        "familyName": "Günther",
        "mailAddress": "karl-heinz.guenther@example.com"               
      }
    }
  ]
}
```
