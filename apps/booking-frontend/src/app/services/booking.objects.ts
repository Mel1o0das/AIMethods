import { getBookingDataSource } from "./data-source";
import { isMockMode } from "./data-mode";
import type {
  BookingObject,
  GetObjectsParams,
  PagedResult,
} from "./data-contracts";

export type { BookingObject, GetObjectsParams, PagedResult } from "./data-contracts";

export interface BookingObjectRequest {
  name: string;
  description: string;
  price: number;
  capacity: number;
  imageUrl: string;
  isActive: boolean;
}

export interface CreateObjectPayload extends BookingObjectRequest {
  objectTypeId: string | null;
}

export interface UpdateObjectPayload {
  name: string;
  description: string;
  price: number;
  capacity: number;
  imageUrl: string | null;
  isActive: boolean;
  objectTypeId: string | null;
}

const BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5260/api";

const getToken = (): string | null =>
  typeof window === "undefined" ? null : window.localStorage.getItem("token");

const requireApiObjectMutation = (): void => {
  if (isMockMode()) {
    throw new Error("Управление объектами недоступно в демонстрационном режиме");
  }
};

export const getImageUrl = (imageUrl: string | null): string | null => {
  if (!imageUrl) return null;
  if (imageUrl.startsWith("http")) return imageUrl;
  const filename = imageUrl.split(/[\\/]/).pop();
  return `${BASE_URL.replace(/\/api$/, "")}/images/${filename}`;
};

export const getObjectsPaged = (
  params: GetObjectsParams = {},
): Promise<PagedResult<BookingObject>> =>
  getBookingDataSource().getObjectsPaged(params);

export const getAllObjects = async (): Promise<BookingObject[]> => {
  const result = await getObjectsPaged({ pageSize: 100 });
  return result.items;
};

export const getObjectById = (id: string): Promise<BookingObject> =>
  getBookingDataSource().getObjectById(id);

export const createObject = async (
  data: CreateObjectPayload,
): Promise<string> => {
  requireApiObjectMutation();
  const token = getToken();
  const response = await fetch(`${BASE_URL}/booking-objects`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(data),
  });
  const envelope = await response.json();
  if (!response.ok) {
    throw new Error(envelope.error?.message ?? "Не удалось создать объект");
  }
  return envelope.result;
};

export const updateObject = async (
  id: string,
  data: UpdateObjectPayload,
): Promise<string> => {
  requireApiObjectMutation();
  const token = getToken();
  const response = await fetch(`${BASE_URL}/booking-objects/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(data),
  });
  const envelope = await response.json();
  if (!response.ok) {
    throw new Error(envelope.error?.message ?? "Не удалось обновить объект");
  }
  return envelope.result;
};

export const deleteObject = async (id: string): Promise<void> => {
  requireApiObjectMutation();
  const token = getToken();
  const response = await fetch(`${BASE_URL}/booking-objects/${id}`, {
    method: "DELETE",
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!response.ok) {
    const envelope = await response.json();
    throw new Error(envelope.error?.message ?? "Не удалось удалить объект");
  }
};
