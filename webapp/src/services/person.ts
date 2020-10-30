export interface PersonResponse {
  id: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
}

export interface PersonResponse2 extends PersonResponse {
  version: number;
  creationTime: Date;
  creator: PersonResponse | null;
  deletionTime: Date | null;
  deletor: PersonResponse | null;
  history: Array<PersonSnapshot>;
}

export interface PersonSnapshot {
  version: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
  editTime: Date;
  editor: PersonResponse | null;
}

export interface CreatePersonRequest {
  givenName: string;
  familyName: string;
  mailAddress: string | null;
}

async function get<T> (query: string): Promise<T> {
  const response = await fetch('http://localhost:50805' + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

async function post<T> (query: string, body: object): Promise<T> {
  const response = await fetch('http://localhost:50805' + query, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  })
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

export function getPeople (): Promise<PersonResponse[]> {
  return get('/api/people')
}

export function getPerson (id: number): Promise<PersonResponse2> {
  return get('/api/person/' + id)
}

export function createPerson (person: CreatePersonRequest): Promise<PersonResponse2> {
  return post<PersonResponse2>('/api/person/new', person)
}
