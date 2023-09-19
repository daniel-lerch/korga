import client from "./client";

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

export interface EventRegistrationRequest {
  programId: number;
  givenName: string;
  familyName: string;
}

export function getEvents(): Promise<EventResponse[]> {
  return client.get("/api/events");
}

export function getEvent(id: string): Promise<EventResponse2> {
  return client.get("/api/event/" + id);
}

export function registerForEvent(data: EventRegistrationRequest) {
  return client.post("/api/events/register", data);
}

export function deletePerson(id: string) {
  return client.delete("/api/events/participant/" + id);
}
