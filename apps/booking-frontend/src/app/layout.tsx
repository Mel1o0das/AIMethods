import type { Metadata, Viewport } from "next";
import "./globals.css";
import { Layout } from "antd";
import { Content, Footer } from "antd/es/layout/layout";
import { AppProviders } from "./AppProviders";
import { AppHeader } from "./components/AppHeader";
import { isMockMode } from "./services/data-mode";

export const metadata: Metadata = {
  title: "BookingService — бани и глэмпинги",
  description: "Каталог мест для спокойного отдыха за городом",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html>
      <body>
        <AppProviders>
          <Layout className="root-layout">
            <AppHeader />
            {isMockMode() && (
              <div className="demo-mode-banner" role="status">
                Демонстрационный режим · реальные заявки не создаются · данные сбрасываются после перезагрузки
              </div>
            )}
            <Content className="root-content">{children}</Content>
            <Footer className="root-footer">
              <span>BookingService</span>
              <span>Бани и глэмпинги для спокойного отдыха</span>
              <span>Учебная демонстрация · 2026</span>
            </Footer>
          </Layout>
        </AppProviders>
      </body>
    </html>
  );
}
