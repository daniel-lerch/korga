import { encode } from "base64-arraybuffer";
import client from "./client";

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

export function postHash(data: HashPostRequest) {
  return client.post("/api/password/reset", data);
}

export interface TokenData {
  uid: string;
  givenName: string;
  familyName: string;
}

export function checkToken(token: string): Promise<TokenData> {
  return client.get(`/api/password/reset?token=${token}`);
}
