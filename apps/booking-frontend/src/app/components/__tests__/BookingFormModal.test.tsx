import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import dayjs from "dayjs";
import type { ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  createBooking: vi.fn(),
  getBookingStatuses: vi.fn(),
  getOccupiedSlotsForRange: vi.fn(),
}));

vi.mock("antd", async () => {
  const actual = await vi.importActual<typeof import("antd")>("antd");
  const { default: dayjs } = await import("dayjs");

  return {
    ...actual,
    Modal: ({ children, title }: { children: ReactNode; title: ReactNode }) => (
      <section role="dialog" aria-label={String(title)}>
        {children}
      </section>
    ),
    DatePicker: ({
      value,
      onChange,
      placeholder,
    }: {
      value?: { toISOString(): string };
      onChange?: (value: ReturnType<typeof dayjs>) => void;
      placeholder?: string;
    }) => (
      <input
        placeholder={placeholder}
        value={value?.toISOString() ?? ""}
        onChange={(event) => onChange?.(dayjs(event.target.value))}
      />
    ),
  };
});

vi.mock("../../services/bookings", () => ({
  createBooking: mocks.createBooking,
  getBookingStatuses: mocks.getBookingStatuses,
  getOccupiedSlotsForRange: mocks.getOccupiedSlotsForRange,
}));

vi.mock("../../services/data-mode", () => ({ isMockMode: () => true }));

import { BookingFormModal } from "../BookingFormModal";

const booking = {
  id: "object-forest-sauna",
  name: { value: "Лесная баня" },
  description: { value: "Демонстрационный объект" },
  price: 4200,
  capacity: 6,
  imageUrl: null,
  isActive: true,
  createdAt: "2026-01-10T09:00:00.000Z",
  objectTypeId: "type-sauna",
};

describe("BookingFormModal", () => {
  beforeEach(() => {
    mocks.createBooking.mockReset();
    mocks.getBookingStatuses.mockReset();
    mocks.getOccupiedSlotsForRange.mockReset();
    mocks.getBookingStatuses.mockResolvedValue([
      {
        id: "status-pending",
        name: { value: "Ожидает подтверждения" },
      },
    ]);
    mocks.getOccupiedSlotsForRange.mockResolvedValue([]);
  });

  it("requests availability with local calendar dates", async () => {
    const expectedFrom = dayjs().format("YYYY-MM-DD");
    const expectedTo = dayjs().add(365, "day").format("YYYY-MM-DD");

    render(
      <BookingFormModal
        booking={booking}
        objectTypes={[
          { id: "type-sauna", name: { value: "Баня" } },
        ]}
        user={{ id: "mock-user" }}
        onClose={vi.fn()}
        onSuccess={vi.fn()}
      />,
    );

    await waitFor(() =>
      expect(mocks.getOccupiedSlotsForRange).toHaveBeenCalledWith(
        booking.id,
        expectedFrom,
        expectedTo,
      ),
    );
    const [, from, to] = mocks.getOccupiedSlotsForRange.mock.calls[0] as [
      string,
      string,
      string,
    ];
    expect(from).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    expect(to).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  it("keeps entered values and stays open when creation reports a conflict", async () => {
    mocks.createBooking.mockRejectedValue(
      new Error("Выбранное демонстрационное время уже занято"),
    );
    const onClose = vi.fn();

    render(
      <BookingFormModal
        booking={booking}
        objectTypes={[
          { id: "type-sauna", name: { value: "Баня" } },
        ]}
        user={{ id: "mock-user" }}
        onClose={onClose}
        onSuccess={vi.fn()}
      />,
    );

    const startInput = await screen.findByPlaceholderText(
      "Выберите дату и время начала",
    );
    const endInput = screen.getByPlaceholderText(
      "Выберите дату и время окончания",
    );
    const guestsInput = screen.getByRole("spinbutton");

    fireEvent.change(startInput, { target: { value: "2099-07-01T10:00:00.000Z" } });
    fireEvent.change(endInput, { target: { value: "2099-07-01T12:00:00.000Z" } });
    fireEvent.change(guestsInput, { target: { value: "2" } });
    fireEvent.click(
      screen.getByRole("button", { name: "Создать демо-заявку" }),
    );

    expect(
      await screen.findByText("Выбранное демонстрационное время уже занято"),
    ).toBeInTheDocument();
    expect(mocks.createBooking).toHaveBeenCalledOnce();
    expect(startInput).toHaveValue("2099-07-01T10:00:00.000Z");
    expect(endInput).toHaveValue("2099-07-01T12:00:00.000Z");
    expect(guestsInput).toHaveValue("2");
    expect(onClose).not.toHaveBeenCalled();
    await waitFor(() =>
      expect(
        screen.getByRole("button", { name: "Создать демо-заявку" }),
      ).toBeEnabled(),
    );
  });
});
