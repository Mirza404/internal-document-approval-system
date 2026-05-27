import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  loadAuthSession,
  loadAuthToken,
  loadAuthUser,
  saveAuthSession,
  type AuthUser,
} from "./authStorage";

const user: AuthUser = {
  userId: "user-1",
  email: "employee@internaldocs.local",
  fullName: "Demo Employee",
  role: "Employee",
};

const createToken = (exp: number) => {
  const encode = (value: object) =>
    btoa(JSON.stringify(value))
      .replaceAll("+", "-")
      .replaceAll("/", "_")
      .replaceAll("=", "");

  return `${encode({ alg: "none", typ: "JWT" })}.${encode({ exp })}.signature`;
};

describe("authStorage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.useRealTimers();
  });

  it("saves and loads a current authentication session", () => {
    const token = createToken(Math.floor(Date.now() / 1000) + 60);

    saveAuthSession(token, user);

    expect(loadAuthSession()).toEqual({ token, user });
  });

  it("clears the stored session when the token has expired", () => {
    const token = createToken(Math.floor(Date.now() / 1000) - 60);
    saveAuthSession(token, user);

    expect(loadAuthToken()).toBeNull();
    expect(localStorage.getItem("authToken")).toBeNull();
    expect(localStorage.getItem("authUser")).toBeNull();
  });

  it("removes malformed stored user data", () => {
    localStorage.setItem("authUser", "{invalid");

    expect(loadAuthUser()).toBeNull();
    expect(localStorage.getItem("authUser")).toBeNull();
  });
});
