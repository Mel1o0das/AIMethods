import { requireApiMode } from "./data-mode";

const BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5260/api";

export interface AdminUser {
  id: string;
  email: { value: string };
  fullName: { lastName: string; firstName: string; patronymic: string };
  phone: { value: string };
  roleId: string;
  createdAt: string;
}

export const getFullName = (user: AdminUser): string =>
  `${user.fullName.lastName} ${user.fullName.firstName} ${user.fullName.patronymic}`.trim();

export const getAllUsers = async (): Promise<AdminUser[]> => {
  requireApiMode("Загрузка пользователей");
  const token = localStorage.getItem("token");
  const res = await fetch(`${BASE_URL}/users`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  const envelope = await res.json();
  return envelope.result ?? [];
};
