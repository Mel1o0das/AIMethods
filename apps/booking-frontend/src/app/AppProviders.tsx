"use client";

import { ConfigProvider } from "antd";
import ruRU from "antd/locale/ru_RU";
import type { ReactNode } from "react";
import { AuthProvider } from "./context/AuthContext";

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <ConfigProvider
      locale={ruRU}
      theme={{
        token: {
          colorPrimary: "#a9783d",
          colorInfo: "#557667",
          colorSuccess: "#4f765b",
          colorWarning: "#a9783d",
          colorError: "#a84242",
          colorLink: "#926632",
          colorLinkHover: "#b68548",
          borderRadius: 4,
          controlHeight: 40,
          fontFamily: "Arial, Helvetica, sans-serif",
        },
        components: {
          Button: {
            primaryShadow: "none",
            dangerShadow: "none",
          },
          Modal: {
            borderRadiusLG: 6,
          },
          Pagination: {
            itemActiveBg: "#d8ad71",
          },
        },
      }}
    >
      <AuthProvider>{children}</AuthProvider>
    </ConfigProvider>
  );
}
