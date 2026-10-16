import { requireApiMode } from "./data-mode";

const BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5260/api";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  lastName: string;
  firstName: string;
  patronymic: string;
  phone: string;
  roleId?: string;
}

export interface Role {
  id: string;
  name: { value: string };
}

export const login = async (data: LoginRequest): Promise<LoginResponse> => {
  requireApiMode("Авторизация");
  const res = await fetch(`${BASE_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  const envelope = await res.json();
  if (!res.ok) throw new Error(envelope.error?.messages?.[0]?.message ?? envelope.error?.message ?? "Ошибка входа");
  return envelope.result;
};

export const register = async (data: RegisterRequest): Promise<string> => {
  requireApiMode("Регистрация");
  const res = await fetch(`${BASE_URL}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  const envelope = await res.json();
  if (!res.ok) throw new Error(envelope.error?.messages?.[0]?.message ?? envelope.error?.message ?? "Ошибка регистрации");
  return envelope.result;
};

export const getRoles = async (token?: string): Promise<Role[]> => {
  requireApiMode("Загрузка ролей");
  const headers: HeadersInit = token
    ? { Authorization: `Bearer ${token}` }
    : {};
  const res = await fetch(`${BASE_URL}/roles`, { headers });
  const envelope = await res.json();
  return envelope.result ?? [];
};

export interface UserProfile {
  id: string;
  email: string;
  lastName: string;
  firstName: string;
  patronymic: string;
  phone: string;
  roleId: string;
  createdAt: string;
}

export const getMe = async (token: string): Promise<UserProfile> => {
  requireApiMode("Загрузка профиля");
  const res = await fetch(`${BASE_URL}/auth/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  const envelope = await res.json();
  if (!res.ok) throw new Error(envelope.error?.messages?.[0]?.message ?? envelope.error?.message ?? "Ошибка профиля");
  return envelope.result;
};
