import { get, post, put } from './client'

export interface PersonResponse {
  id: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
}

export interface PersonResponse2 extends PersonResponse {
  version: number;
  creationTime: Date;
  createdBy: PersonResponse | null;
  deletionTime: Date | null;
  deletedBy: PersonResponse | null;
  history: PersonSnapshot[];
}

export interface PersonSnapshot {
  version: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
  overrideTime: Date;
  overriddenBy: PersonResponse | null;
}

export interface PersonRequest {
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

export function createPerson (person: PersonRequest): Promise<PersonResponse2> {
  return post<PersonResponse2>('/api/person/new', person)
}

export function updatePerson (id: number, person: PersonRequest): Promise<PersonResponse2> {
  return put<PersonResponse2>('/api/person/' + id, person)
}
