export class Person {
  constructor (id: number, givenName: string, familyName: string, mailAddress: string | null) {
    this.id = id
    this.givenName = givenName
    this.familyName = familyName
    this.mailAddress = mailAddress
  }

  id: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
}

export function getPeople (): Promise<Person[]> {
  return fetch('http://localhost:50805/api/people')
    .then(response => response.json())
    .then(response => response as Person[])
}
