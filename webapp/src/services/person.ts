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

export async function getPeople (): Promise<Person[]> {
  const response = await fetch('http://localhost:50805/api/people')
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json()
}
