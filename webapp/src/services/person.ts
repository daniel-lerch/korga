import { get, post } from './client'

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
  history: PersonSnapshot[];
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

export function getPeople (): Promise<PersonResponse[]> {
  return get('/api/people')
}

export function getPerson (id: number): Promise<PersonResponse2> {
  return get('/api/person/' + id)
}

export function createPerson (person: CreatePersonRequest): Promise<PersonResponse2> {
  return post<PersonResponse2>('/api/person/new', person)
}
