# Documentation of the Korga HTTP API

## Person
### `GET /api/people`
Returns a list of people. Query parameters:
- `offset` Start index of results
- `count` Limit number of results to this value
```json
[
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
