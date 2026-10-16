import type {
  BookingDataSource,
  BookingObject,
  BookingStatus,
  CreateBookingPayload,
  GetObjectsParams,
  MyBooking,
  ObjectType,
  OccupiedSlot,
  PagedResult,
} from "./data-contracts";

const BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5260/api";

const getToken = (): string | null =>
  typeof window === "undefined" ? null : window.localStorage.getItem("token");

const getEnvelope = async <T>(
  response: Response,
  fallbackMessage: string,
): Promise<T> => {
  const envelope = (await response.json().catch(() => ({}))) as {
    result?: T;
    error?: { message?: string; messages?: Array<{ message?: string }> };
  };
  if (!response.ok) {
    throw new Error(
      envelope.error?.messages?.[0]?.message ??
        envelope.error?.message ??
        fallbackMessage,
    );
  }
  if (!("result" in envelope)) throw new Error(fallbackMessage);
  return envelope.result as T;
};

const authorizedHeaders = (): HeadersInit => {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
};

export const apiDataSource: BookingDataSource = {
  async getObjectsPaged(
    params: GetObjectsParams = {},
  ): Promise<PagedResult<BookingObject>> {
    const query = new URLSearchParams();
    if (params.page != null) query.set("page", String(params.page));
    if (params.pageSize != null) query.set("pageSize", String(params.pageSize));
    if (params.search) query.set("search", params.search);
    if (params.sortBy) query.set("sortBy", params.sortBy);
    if (params.sortDir) query.set("sortDir", params.sortDir);
    if (params.typeId) query.set("typeId", params.typeId);
    if (params.maxPrice != null) query.set("maxPrice", String(params.maxPrice));
    if (params.minCapacity != null) {
      query.set("minCapacity", String(params.minCapacity));
    }

    const response = await fetch(
      `${BASE_URL}/booking-objects?${query.toString()}`,
      { headers: authorizedHeaders() },
    );
    return getEnvelope(response, "Не удалось загрузить объекты");
  },

  async getObjectById(id: string): Promise<BookingObject> {
    const response = await fetch(`${BASE_URL}/booking-objects/${id}`);
    return getEnvelope(response, "Объект не найден");
  },

  async getObjectTypes(): Promise<ObjectType[]> {
    const response = await fetch(`${BASE_URL}/object-types`);
    return getEnvelope(response, "Не удалось загрузить типы объектов");
  },

  async getBookingStatuses(): Promise<BookingStatus[]> {
    const response = await fetch(`${BASE_URL}/booking-statuses`);
    return getEnvelope(response, "Не удалось загрузить статусы бронирования");
  },

  async getMyBookings(userId: string): Promise<MyBooking[]> {
    const response = await fetch(`${BASE_URL}/bookings/my/${userId}`, {
      headers: authorizedHeaders(),
    });
    return getEnvelope(response, "Не удалось загрузить бронирования");
  },

  async getAllBookings(): Promise<MyBooking[]> {
    const response = await fetch(`${BASE_URL}/bookings`, {
      headers: authorizedHeaders(),
    });
    return getEnvelope(response, "Не удалось загрузить бронирования");
  },

  async confirmBooking(bookingId: string): Promise<void> {
    const response = await fetch(`${BASE_URL}/bookings/${bookingId}/confirm`, {
      method: "PUT",
      headers: authorizedHeaders(),
    });
    await getEnvelope(response, "Не удалось подтвердить бронирование");
  },

  async cancelBooking(bookingId: string): Promise<void> {
    const response = await fetch(`${BASE_URL}/bookings/${bookingId}/cancel`, {
      method: "PUT",
      headers: authorizedHeaders(),
    });
    await getEnvelope(response, "Не удалось отменить бронирование");
  },

  async getOccupiedSlotsForRange(
    objectId: string,
    from: string,
    to: string,
  ): Promise<OccupiedSlot[]> {
    const query = new URLSearchParams({ objectId, from, to });
    const response = await fetch(`${BASE_URL}/availability/range?${query}`);
    return getEnvelope(response, "Не удалось загрузить занятость");
  },

  async createBooking(data: CreateBookingPayload): Promise<string> {
    const response = await fetch(`${BASE_URL}/bookings`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...authorizedHeaders(),
      },
      body: JSON.stringify(data),
    });
    return getEnvelope(response, "Не удалось создать бронирование");
  },
};
