import { encode } from "base64-arraybuffer";
import client, { get, send } from "./client";

// OpenLDAP compatible SSHA (salted SHA-1) hash function
// https://stackoverflow.com/a/42490862/7075733
export async function ssha(password: string) {
  const encodedPassword = new TextEncoder().encode(password);
  const salt = crypto.getRandomValues(new Uint8Array(4));
  const combined = new Uint8Array(encodedPassword.length + salt.length);
  combined.set(encodedPassword);
  combined.set(salt, encodedPassword.length);
  const hash = await crypto.subtle.digest("SHA-1", combined);
  const hashAndSalt = new Uint8Array(hash.byteLength + salt.length);
  hashAndSalt.set(new Uint8Array(hash));
  hashAndSalt.set(salt, hash.byteLength);
  const base64 = encode(hashAndSalt.buffer);
  return "{SSHA}" + base64;
}

export interface HashPostRequest {
  token: string;
  passwordHash: string;
}

export async function postHash(data: HashPostRequest): Promise<boolean> {
  const response = await client.post("/api/password/reset", data);
  return response.status === 204;
}

export interface TokenData {
  uid: string;
  givenName: string;
  familyName: string;
}

export async function checkToken(token: string): Promise<TokenData> {
  // return get(`/korga/api/password/reset?token=${token}`);
  const response = await client.get(`/api/password/reset?token=${token}`);
  return response.data ?? null;
}
