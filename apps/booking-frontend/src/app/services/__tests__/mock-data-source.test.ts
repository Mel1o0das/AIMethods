import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  getObjectById,
  getObjectsPaged,
} from "../booking.objects";
import {
  cancelBooking,
  createBooking,
  getBookingStatuses,
  getMyBookings,
  getOccupiedSlotsForRange,
} from "../bookings";
import { getObjectTypes } from "../object-types";
import { resetMockData } from "../mock-data-source";
import { getMe } from "../auth";
import { getAllUsers } from "../users";

describe("mock data source", () => {
  const fetchSpy = vi.fn(() =>
    Promise.reject(new Error("fetch must not be called in mock mode")),
  );

  beforeEach(() => {
    vi.stubEnv("NEXT_PUBLIC_DATA_MODE", "mock");
    vi.stubGlobal("fetch", fetchSpy);
    fetchSpy.mockClear();
    resetMockData();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.unstubAllEnvs();
  });

  it("filters and paginates the deterministic catalog without HTTP", async () => {
    const result = await getObjectsPaged({
      search: "баня",
      typeId: "type-sauna",
      page: 1,
      pageSize: 1,
      sortBy: "price",
      sortDir: "asc",
    });
    const types = await getObjectTypes();

    expect(result.totalCount).toBe(2);
    expect(result.items).toHaveLength(1);
    expect(result.items[0].id).toBe("object-pine-sauna");
    expect(types.map((type) => type.id)).toEqual([
      "type-sauna",
      "type-glamping",
    ]);
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("creates and cancels a mock booking without HTTP", async () => {
    const statuses = await getBookingStatuses();
    const pendingStatus = statuses.find(
      (status) => status.id === "status-pending",
    );
    expect(pendingStatus).toBeDefined();

    const bookingId = await createBooking({
      userId: "mock-user",
      bookingObjectId: "object-forest-sauna",
      startDateTime: "2026-12-01T10:00:00.000Z",
      endDateTime: "2026-12-01T12:00:00.000Z",
      guestsCount: 4,
      comment: "Тестовая заявка",
      statusId: pendingStatus!.id,
    });

    expect(await getMyBookings("mock-user")).toEqual(
      expect.arrayContaining([expect.objectContaining({ id: bookingId })]),
    );

    await cancelBooking(bookingId);
    expect(await getMyBookings("mock-user")).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          id: bookingId,
          statusId: "status-cancelled",
        }),
      ]),
    );
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("returns occupied slots and rejects a conflicting request without HTTP", async () => {
    const slots = await getOccupiedSlotsForRange(
      "object-lake-glamping",
      "2026-11-01T00:00:00.000Z",
      "2026-11-30T23:59:59.999Z",
    );
    expect(slots).toHaveLength(1);

    await expect(
      createBooking({
        userId: "mock-user",
        bookingObjectId: "object-lake-glamping",
        startDateTime: "2026-11-15T12:00:00.000Z",
        endDateTime: "2026-11-17T09:00:00.000Z",
        guestsCount: 2,
        statusId: "status-pending",
      }),
    ).rejects.toThrow("демонстрационное время уже занято");
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("reports a missing mock object as an error", async () => {
    await expect(getObjectById("missing-object")).rejects.toThrow(
      "Демонстрационный объект не найден",
    );
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("blocks unrelated real API services in mock mode", async () => {
    await expect(getMe("persisted-real-token")).rejects.toThrow(
      "HTTP-запрос заблокирован",
    );
    await expect(getAllUsers()).rejects.toThrow("HTTP-запрос заблокирован");
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("keeps a new booking private to its owner and rejects an overlap", async () => {
    const payload = {
      userId: "mock-user",
      bookingObjectId: "object-forest-sauna",
      startDateTime: "2099-04-12T10:00:00.000Z",
      endDateTime: "2099-04-12T12:00:00.000Z",
      guestsCount: 3,
      statusId: "status-pending",
    };

    const bookingId = await createBooking(payload);

    expect((await getMyBookings("mock-user")).map((item) => item.id)).toContain(
      bookingId,
    );
    expect(await getMyBookings("another-user")).toHaveLength(0);
    await expect(
      createBooking({
        ...payload,
        startDateTime: "2099-04-12T11:30:00.000Z",
        endDateTime: "2099-04-12T13:00:00.000Z",
      }),
    ).rejects.toThrow();
    expect(
      (await getMyBookings("mock-user")).filter(
        (item) => item.bookingObjectId === payload.bookingObjectId,
      ),
    ).toHaveLength(1);
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("releases a half-open interval after its owner cancels it", async () => {
    const payload = {
      userId: "mock-user",
      bookingObjectId: "object-forest-sauna",
      startDateTime: "2099-05-01T10:00:00.000Z",
      endDateTime: "2099-05-01T12:00:00.000Z",
      guestsCount: 2,
      statusId: "status-pending",
    };
    const bookingId = await createBooking(payload);

    await cancelBooking(bookingId, "mock-user");

    expect(
      await getOccupiedSlotsForRange(
        payload.bookingObjectId,
        payload.startDateTime,
        payload.endDateTime,
      ),
    ).toHaveLength(0);
    await expect(createBooking(payload)).resolves.toMatch(/^booking-demo-/);
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("validates the interval and guest capacity before changing mock state", async () => {
    const before = await getMyBookings("mock-user");
    const basePayload = {
      userId: "mock-user",
      bookingObjectId: "object-forest-sauna",
      startDateTime: "2099-06-01T12:00:00.000Z",
      endDateTime: "2099-06-01T10:00:00.000Z",
      guestsCount: 2,
      statusId: "status-pending",
    };

    await expect(createBooking(basePayload)).rejects.toThrow();
    await expect(
      createBooking({
        ...basePayload,
        startDateTime: "2099-06-01T10:00:00.000Z",
        endDateTime: "2099-06-01T12:00:00.000Z",
        guestsCount: 7,
      }),
    ).rejects.toThrow();

    expect(await getMyBookings("mock-user")).toHaveLength(before.length);
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("does not let a demo user cancel somebody else's booking", async () => {
    await expect(cancelBooking("booking-demo-1", "another-user")).rejects.toThrow();
    expect(await getMyBookings("mock-user")).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          id: "booking-demo-1",
          statusId: "status-confirmed",
        }),
      ]),
    );
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("allows adjacent half-open intervals without treating them as overlaps", async () => {
    const base = {
      userId: "mock-user",
      bookingObjectId: "object-forest-sauna",
      guestsCount: 2,
      statusId: "status-pending",
    };

    await createBooking({
      ...base,
      startDateTime: "2099-07-01T10:00:00.000Z",
      endDateTime: "2099-07-01T12:00:00.000Z",
    });
    await expect(
      createBooking({
        ...base,
        startDateTime: "2099-07-01T12:00:00.000Z",
        endDateTime: "2099-07-01T14:00:00.000Z",
      }),
    ).resolves.toMatch(/^booking-demo-/);
    await expect(
      createBooking({
        ...base,
        startDateTime: "2099-07-01T08:00:00.000Z",
        endDateTime: "2099-07-01T10:00:00.000Z",
      }),
    ).resolves.toMatch(/^booking-demo-/);

    expect(
      (await getMyBookings("mock-user")).filter(
        (item) => item.bookingObjectId === base.bookingObjectId,
      ),
    ).toHaveLength(3);
  });

  it("atomically keeps only one booking when the same interval is submitted twice", async () => {
    const payload = {
      userId: "mock-user",
      bookingObjectId: "object-forest-sauna",
      startDateTime: "2099-08-01T10:00:00.000Z",
      endDateTime: "2099-08-01T12:00:00.000Z",
      guestsCount: 2,
      statusId: "status-pending",
    };

    const results = await Promise.allSettled([
      createBooking(payload),
      createBooking(payload),
    ]);

    expect(results.filter((result) => result.status === "fulfilled")).toHaveLength(1);
    expect(results.filter((result) => result.status === "rejected")).toHaveLength(1);
    expect(
      (await getMyBookings("mock-user")).filter(
        (item) => item.bookingObjectId === payload.bookingObjectId,
      ),
    ).toHaveLength(1);
  });

  it("does not allow cancellation after the demo interval has ended", async () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2027-01-01T00:00:00.000Z"));

    await expect(cancelBooking("booking-demo-1", "mock-user")).rejects.toThrow(
      "Завершённую демонстрационную заявку отменить нельзя",
    );
    expect(await getMyBookings("mock-user")).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          id: "booking-demo-1",
          statusId: "status-confirmed",
        }),
      ]),
    );
  });
});
