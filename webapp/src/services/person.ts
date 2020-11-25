import { get, send } from './client'

export interface PersonResponse {
  id: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
  deleted: boolean;
}

export interface PersonResponse2 {
  id: number;
  conflict: boolean;
  version: number;
  givenName: string;
  familyName: string;
  mailAddress: string | null;
  memberships: PersonMembership[];
  creationTime: Date;
  createdBy: PersonResponse | null;
  deletionTime: Date | null;
  deletedBy: PersonResponse | null;
  history: PersonSnapshot[];
}

export interface PersonMembership {
  id: number;
  roleId: number;
  roleName: string;
  groupId: number;
  groupName: string;
  creationTime: Date;
  createdBy: PersonResponse | null;
  deletionTime: Date | null;
  deletedBy: PersonResponse | null;
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

// TODO: createPerson does not have to handle 409 Conflict
export function createPerson (person: PersonRequest): Promise<PersonResponse2> {
  return send<PersonResponse2>('POST', '/api/person/new', person)
}

export function updatePerson (id: number, person: PersonRequest): Promise<PersonResponse2> {
  return send<PersonResponse2>('PUT', '/api/person/' + id, person)
}

export function deletePerson (id: number): Promise<PersonResponse2> {
  return send<PersonResponse2>('DELETE', '/api/person/' + id)
}
