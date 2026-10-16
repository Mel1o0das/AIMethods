import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { getObjectsPagedMock, getObjectTypesMock } = vi.hoisted(() => ({
  getObjectsPagedMock: vi.fn(),
  getObjectTypesMock: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useSearchParams: () => new URLSearchParams(),
}));

vi.mock("../../services/booking.objects", () => ({
  getObjectsPaged: getObjectsPagedMock,
}));

vi.mock("../../services/object-types", () => ({
  getObjectTypes: getObjectTypesMock,
}));

vi.mock("../../services/data-mode", () => ({
  isMockMode: () => true,
}));

vi.mock("../../components/BookingObjects", () => ({
  BookingObjects: ({ items }: { items: Array<{ name: { value: string } }> }) => (
    <div>{items.map((item) => <article key={item.name.value}>{item.name.value}</article>)}</div>
  ),
}));

import { BookingObjectsPageContent } from "../page";

describe("BookingObjectsPageContent", () => {
  beforeEach(() => {
    getObjectsPagedMock.mockReset();
    getObjectTypesMock.mockReset();
    getObjectTypesMock.mockResolvedValue([]);
  });

  it("shows a load error and renders cards after retry", async () => {
    getObjectsPagedMock
      .mockRejectedValueOnce(new Error("Mock network failure"))
      .mockResolvedValueOnce({
        items: [
          {
            id: "forest-sauna",
            name: { value: "Лесная баня" },
            description: { value: "Описание" },
            price: 4200,
            capacity: 6,
            imageUrl: null,
            isActive: true,
            createdAt: "2026-01-01T00:00:00.000Z",
            objectTypeId: "type-sauna",
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 10,
        totalPages: 1,
      });

    render(<BookingObjectsPageContent />);

    expect(await screen.findByText("Не удалось загрузить объекты")).toBeInTheDocument();
    expect(screen.getByText("Mock network failure")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Повторить" }));

    await waitFor(() => expect(screen.getByText("Лесная баня")).toBeInTheDocument());
    expect(getObjectsPagedMock).toHaveBeenCalledTimes(2);
  });
});
