import { fireEvent, render, screen } from "@testing-library/react";
import { useMemo, useState } from "react";
import { describe, expect, it, vi } from "vitest";
import type { BookingObject } from "../../services/booking.objects";
import { BookingObjects } from "../BookingObjects";

vi.mock("../BookingFormModal", () => ({
  BookingFormModal: () => null,
}));

const objects: BookingObject[] = [
  {
    id: "sauna",
    name: { value: "Лесная баня" },
    description: { value: "Баня среди сосен" },
    price: 4200,
    capacity: 6,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-01T00:00:00.000Z",
    objectTypeId: "type-sauna",
  },
  {
    id: "house",
    name: { value: "Дом у озера" },
    description: { value: "Тихий глэмпинг у воды" },
    price: 9800,
    capacity: 4,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-02T00:00:00.000Z",
    objectTypeId: "type-glamping",
  },
];

function CatalogHarness() {
  const [search, setSearch] = useState("");
  const visibleObjects = useMemo(() => {
    const query = search.toLowerCase();
    return objects.filter(
      (object) =>
        object.name.value.toLowerCase().includes(query) ||
        object.description.value.toLowerCase().includes(query),
    );
  }, [search]);

  return (
    <BookingObjects
      items={visibleObjects}
      totalCount={visibleObjects.length}
      page={1}
      pageSize={10}
      totalPages={1}
      loading={false}
      search={search}
      sortBy="createdAt"
      sortDir="asc"
      typeId={null}
      maxPrice={null}
      minCapacity={null}
      objectTypes={[
        { id: "type-sauna", name: { value: "Баня" } },
        { id: "type-glamping", name: { value: "Глэмпинг" } },
      ]}
      typesLoading={false}
      typesError={null}
      isDemo
      onRetryTypes={vi.fn()}
      onSearchChange={setSearch}
      onSortChange={vi.fn()}
      onPageSizeChange={vi.fn()}
      onPageChange={vi.fn()}
      onTypeChange={vi.fn()}
      onMaxPriceChange={vi.fn()}
      onMinCapacityChange={vi.fn()}
      onRefresh={vi.fn()}
    />
  );
}

describe("BookingObjects", () => {
  it("changes visible mock results when the user searches", () => {
    render(<CatalogHarness />);

    expect(screen.getByText("Лесная баня")).toBeInTheDocument();
    expect(screen.getByText("Дом у озера")).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText("Поиск по названию или описанию"), {
      target: { value: "озера" },
    });

    expect(screen.queryByText("Лесная баня")).not.toBeInTheDocument();
    expect(screen.getByText("Дом у озера")).toBeInTheDocument();
  });

  it("shows an empty state and resets active filters", () => {
    render(<CatalogHarness />);

    fireEvent.change(screen.getByLabelText("Поиск по названию или описанию"), {
      target: { value: "неизвестное место" },
    });

    expect(screen.getByText("По выбранным условиям ничего не найдено.")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Сбросить фильтры" }));

    expect(screen.getByText("Лесная баня")).toBeInTheDocument();
    expect(screen.getByText("Дом у озера")).toBeInTheDocument();
  });
});
