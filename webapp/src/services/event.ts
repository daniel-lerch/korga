import { get, send } from "./client";

export interface EventResponse {
  id: number;
  name: string;
  programs: {
    id: number;
    name: string;
    limit: number;
    count: number;
  }[];
  memberCount: number;
}

export interface EventResponse2 {
  id: number;
  name: string;
  programs: {
    id: number;
    name: string;
    limit: number;
    participants: {
      id: string;
      givenName: string;
      familyName: string;
    }[];
  }[];
}

export interface EventResponse3 {
  programId: number;
  givenName: string;
  familyName: string;
  conflict: boolean;
}

export interface EventResponse4 {
  conflict: boolean;
}

export function getEvents(): Promise<EventResponse[]> {
  return get("/api/events");
}

export function getEvent(id: string): Promise<EventResponse2> {
  return get("/api/event/" + id);
}

export function registerForEvent(
  data: EventResponse3
): Promise<EventResponse3> {
  return send<EventResponse3>("POST", "/api/events/register", data);
}

export function deletePerson(id: string): Promise<EventResponse4> {
  return send("DELETE", "/api/events/participant/" + id);
}
