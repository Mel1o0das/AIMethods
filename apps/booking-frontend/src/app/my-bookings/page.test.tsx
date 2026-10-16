import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  cancelBooking: vi.fn(),
  getMyBookings: vi.fn(),
  retryMaps: vi.fn(),
  demoUser: { id: "mock-user" },
}));

vi.mock("../context/AuthContext", () => ({
  useAuth: () => ({
    user: mocks.demoUser,
    isAuthenticated: true,
    isAdmin: false,
    isDemo: true,
    logout: vi.fn(),
  }),
}));

vi.mock("../hooks/useBookingMaps", () => ({
  useBookingMaps: () => ({
    objectsMap: {
      "object-forest-sauna": {
        id: "object-forest-sauna",
        name: { value: "Лесная баня" },
      },
    },
    statusesMap: {
      "status-pending": {
        id: "status-pending",
        name: { value: "Ожидает подтверждения" },
      },
      "status-cancelled": {
        id: "status-cancelled",
        name: { value: "Отменено" },
      },
    },
    statuses: [
      { id: "status-pending", name: { value: "Ожидает подтверждения" } },
      { id: "status-cancelled", name: { value: "Отменено" } },
    ],
    loading: false,
    error: null,
    retry: mocks.retryMaps,
  }),
}));

vi.mock("../services/bookings", () => ({
  cancelBooking: mocks.cancelBooking,
  getMyBookings: mocks.getMyBookings,
  getStatusColor: () => "default",
  isCancelledStatus: (name: string) => name === "Отменено",
}));

import MyBookingsPage from "./page";

const activeBooking = {
  id: "booking-demo-42",
  userId: "mock-user",
  bookingObjectId: "object-forest-sauna",
  startDateTime: "2099-05-01T10:00:00.000Z",
  endDateTime: "2099-05-01T12:00:00.000Z",
  guestsCount: 2,
  comment: null,
  statusId: "status-pending",
  createdAt: "2026-10-16T09:00:00.000Z",
};

describe("MyBookingsPage mock interactions", () => {
  beforeEach(() => {
    mocks.cancelBooking.mockReset();
    mocks.getMyBookings.mockReset();
    mocks.retryMaps.mockReset();
  });

  it("shows a load error and recovers to the empty state on retry", async () => {
    mocks.getMyBookings
      .mockRejectedValueOnce(new Error("temporary demo failure"))
      .mockResolvedValueOnce([]);

    render(<MyBookingsPage />);

    expect(
      await screen.findByText("Не удалось загрузить мои заявки"),
    ).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Повторить" }));

    expect(
      await screen.findByText("У вас пока нет демонстрационных заявок"),
    ).toBeInTheDocument();
    expect(mocks.retryMaps).toHaveBeenCalledOnce();
    expect(mocks.getMyBookings).toHaveBeenCalledTimes(2);
  });

  it("confirms cancellation and refreshes the visible status", async () => {
    mocks.cancelBooking.mockResolvedValue(undefined);
    mocks.getMyBookings
      .mockResolvedValueOnce([activeBooking])
      .mockResolvedValueOnce([
        { ...activeBooking, statusId: "status-cancelled" },
      ]);

    render(<MyBookingsPage />);

    fireEvent.click(
      await screen.findByRole("button", { name: "Отменить заявку" }),
    );
    fireEvent.click(
      await screen.findByRole("button", { name: "Да, отменить" }),
    );

    await waitFor(() =>
      expect(mocks.cancelBooking).toHaveBeenCalledWith(
        "booking-demo-42",
        "mock-user",
      ),
    );
    expect(await screen.findByText("Отменено")).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Отменить заявку" }),
    ).not.toBeInTheDocument();
  });

  it("shows a completed booking as read-only history", async () => {
    mocks.getMyBookings.mockResolvedValue([
      {
        ...activeBooking,
        id: "booking-demo-past",
        startDateTime: "2020-05-01T10:00:00.000Z",
        endDateTime: "2020-05-01T12:00:00.000Z",
      },
    ]);

    render(<MyBookingsPage />);

    expect(
      await screen.findByText(
        "Завершённая заявка доступна только для просмотра",
      ),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Отменить заявку" }),
    ).not.toBeInTheDocument();
    expect(mocks.cancelBooking).not.toHaveBeenCalled();
  });
});
