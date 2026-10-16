import { render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn(), refresh: vi.fn() }),
}));

import { AuthProvider, useAuth } from "./AuthContext";

function AuthProbe() {
  const { user, isAuthenticated, isAdmin, isDemo } = useAuth();

  return (
    <output>
      {JSON.stringify({
        userId: user?.id ?? null,
        isAuthenticated,
        isAdmin,
        isDemo,
      })}
    </output>
  );
}

describe("AuthProvider in mock mode", () => {
  const fetchSpy = vi.fn(() =>
    Promise.reject(new Error("fetch must not be called in mock mode")),
  );

  beforeEach(() => {
    vi.stubEnv("NEXT_PUBLIC_DATA_MODE", "mock");
    vi.stubGlobal("fetch", fetchSpy);
    localStorage.setItem("token", "a-real-token-must-be-ignored");
    fetchSpy.mockClear();
  });

  afterEach(() => {
    vi.unstubAllEnvs();
    localStorage.clear();
  });

  it("provides a fixed non-admin demo user without reading the API", async () => {
    render(
      <AuthProvider>
        <AuthProbe />
      </AuthProvider>,
    );

    await waitFor(() =>
      expect(screen.getByText(/"userId":"mock-user"/)).toBeInTheDocument(),
    );
    expect(screen.getByText(/"isAuthenticated":true/)).toBeInTheDocument();
    expect(screen.getByText(/"isAdmin":false/)).toBeInTheDocument();
    expect(screen.getByText(/"isDemo":true/)).toBeInTheDocument();
    expect(fetchSpy).not.toHaveBeenCalled();
  });
});
