import client, { get, send } from "./client";

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

export function getEvents(): Promise<EventResponse[]> {
  return get("/api/events");
}

export function getEvent(id: string): Promise<EventResponse2> {
  return get("/api/event/" + id);
}

export async function registerForEvent(data: EventResponse3): Promise<boolean> {
  const response = await client.post("/api/events/register", data);
  return response.status === 204;
}

export async function deletePerson(id: string): Promise<boolean> {
  const response = await client.delete("api/events/participant/" + id);
  return response.status === 204;
}
