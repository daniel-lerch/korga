export interface Person {
  id: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
}

export interface Person2 extends Person {
  version: number;
  creationTime: Date;
  creator: Person | null;
  deletionTime: Date | null;
  deletor: Person | null;
  history: Array<PersonSnapshot>;
}

export interface PersonSnapshot {
  version: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
  editTime: Date;
  editor: Person | null;
}

async function getResonse<T> (query: string): Promise<T> {
  const response = await fetch('http://localhost:50805' + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

export function getPeople (): Promise<Person[]> {
  return getResonse('/api/people')
}

export function getPerson (id: number): Promise<Person2> {
  return getResonse('/api/person/' + id)
}
