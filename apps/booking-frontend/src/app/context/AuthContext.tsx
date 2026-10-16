"use client";

import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { getMe, getRoles, UserProfile } from "../services/auth";
import { isMockMode } from "../services/data-mode";

export const DEMO_USER: UserProfile = {
  id: "mock-user",
  email: "demo@example.invalid",
  lastName: "Пользователь",
  firstName: "Демо",
  patronymic: "",
  phone: "+7 000 000-00-00",
  roleId: "mock-user-role",
  createdAt: "2026-01-01T00:00:00.000Z",
};

interface AuthContextValue {
  user: UserProfile | null;
  isAdmin: boolean;
  isAuthenticated: boolean;
  isDemo: boolean;
  authenticate: (token: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  isAdmin: false,
  isAuthenticated: false,
  isDemo: false,
  authenticate: async () => {},
  logout: () => {},
});

const getSession = async (token: string) => {
  const profile = await getMe(token);
  try {
    const roles = await getRoles(token);
    const myRole = roles.find((role) => role.id === profile.roleId);
    const roleName = myRole?.name.value.toLowerCase() ?? "";
    return {
      profile,
      isAdmin: roleName.includes("admin") || roleName.includes("админ"),
    };
  } catch {
    return { profile, isAdmin: false };
  }
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const demoMode = isMockMode();
  const [user, setUser] = useState<UserProfile | null>(() =>
    demoMode ? DEMO_USER : null,
  );
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    if (demoMode) return;
    const token = localStorage.getItem("token");
    if (!token) return;
    let cancelled = false;

    getSession(token)
      .then((session) => {
        if (!cancelled) {
          setUser(session.profile);
          setIsAdmin(session.isAdmin);
        }
      })
      .catch(() => {
        localStorage.removeItem("token");
      });

    return () => {
      cancelled = true;
    };
  }, [demoMode]);

  const authenticate = async (token: string) => {
    if (demoMode) return;
    localStorage.setItem("token", token);
    try {
      const session = await getSession(token);
      setUser(session.profile);
      setIsAdmin(session.isAdmin);
    } catch (error) {
      localStorage.removeItem("token");
      setUser(null);
      setIsAdmin(false);
      throw error;
    }
  };

  const logout = () => {
    if (demoMode) return;
    localStorage.removeItem("token");
    setUser(null);
    setIsAdmin(false);
    router.push("/");
    router.refresh();
  };

  return (
    <AuthContext.Provider
      value={{ user, isAdmin, isAuthenticated: !!user, isDemo: demoMode, authenticate, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
