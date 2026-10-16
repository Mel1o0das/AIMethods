"use client";

import { Button, Drawer, Menu } from "antd";
import { Layout } from "antd";
import Link from "next/link";
import { useState } from "react";
import { useAuth } from "../context/AuthContext";

const { Header } = Layout;

export function AppHeader() {
  const { user, isAdmin, isAuthenticated, isDemo, logout } = useAuth();
  const [drawerOpen, setDrawerOpen] = useState(false);

  const closeDrawer = () => setDrawerOpen(false);

  const navItems = [
    { key: "home", label: <Link href="/" onClick={closeDrawer}>Главная</Link> },
    { key: "objects", label: <Link href="/objects" onClick={closeDrawer}>Коллекция мест</Link> },
    ...(isAuthenticated && !isAdmin
      ? [{ key: "my-bookings", label: <Link href="/my-bookings" onClick={closeDrawer}>Мои бронирования</Link> }]
      : []),
    ...(isAdmin
      ? [
          { key: "admin", label: <Link href="/admin" onClick={closeDrawer}>Панель админа</Link> },
          { key: "bookings", label: <Link href="/admin/bookings" onClick={closeDrawer}>Бронирования</Link> },
        ]
      : []),
  ];

  return (
    <Header className="app-header">
      <Link href="/" className="app-header__brand">
        <span>Booking</span><em>Service</em>
      </Link>

      <Menu
        theme="dark"
        mode="horizontal"
        items={navItems}
        className="app-header__nav"
      />

      <div className="app-header__auth">
        {user ? (
          <>
            <span className="app-header__greeting">
              {isDemo ? "Демо-пользователь" : `Привет, ${user.firstName}`}
            </span>
            {!isDemo && <Button size="small" onClick={logout}>Выйти</Button>}
          </>
        ) : (
          <Link href="/login">
            <Button type="primary" size="small">Войти</Button>
          </Link>
        )}
      </div>

      <Button
        type="text"
        className="app-header__hamburger"
        onClick={() => setDrawerOpen(true)}
        aria-label="Открыть меню"
        aria-expanded={drawerOpen}
      >
        <span aria-hidden="true">☰</span>
      </Button>

      <Drawer
        title="Меню"
        placement="right"
        open={drawerOpen}
        onClose={closeDrawer}
        size="default"
        className="app-header__drawer"
      >
        <Menu mode="inline" items={navItems} />
        <div className="app-header__drawer-auth">
          {user ? (
            <>
              <p className="app-header__drawer-greeting">
                {isDemo ? "Демо-пользователь" : `Привет, ${user.firstName}`}
              </p>
              {!isDemo && (
                <Button block onClick={() => { logout(); closeDrawer(); }}>
                  Выйти
                </Button>
              )}
            </>
          ) : (
            <Link href="/login" onClick={closeDrawer}>
              <Button type="primary" block>Войти</Button>
            </Link>
          )}
        </div>
      </Drawer>
    </Header>
  );
}
