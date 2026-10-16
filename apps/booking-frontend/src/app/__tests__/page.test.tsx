import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const { pushMock } = vi.hoisted(() => ({ pushMock: vi.fn() }));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: pushMock }),
}));

import Home from "../page";

describe("Home", () => {
  it("opens the catalog with the entered search query", () => {
    render(<Home />);

    fireEvent.change(
      screen.getByLabelText("Найти место по названию или описанию"),
      { target: { value: "дом у озера" } },
    );
    fireEvent.click(screen.getByRole("button", { name: "Найти" }));

    expect(pushMock).toHaveBeenCalledWith(
      "/objects?search=%D0%B4%D0%BE%D0%BC%20%D1%83%20%D0%BE%D0%B7%D0%B5%D1%80%D0%B0",
    );
  });
});
