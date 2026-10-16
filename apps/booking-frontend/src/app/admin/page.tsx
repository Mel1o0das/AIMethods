"use client";

import { useEffect, useState } from "react";
import { Button, Card, Col, Row, Spin, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "../context/AuthContext";
import {
  BookingStatus,
  MyBooking,
  getAllBookings,
  getBookingStatuses,
  getStatusColor,
  isPendingStatus,
  isConfirmedStatus,
} from "../services/bookings";
import { BookingObject, getAllObjects } from "../services/booking.objects";
import { AdminUser, getAllUsers, getFullName } from "../services/users";

const { Title, Text } = Typography;

const formatDt = (iso: string) =>
  new Date(iso).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

export default function AdminPage() {
  const { isAdmin, isAuthenticated } = useAuth();
  const router = useRouter();

  const [loading, setLoading] = useState(true);
  const [bookings, setBookings] = useState<MyBooking[]>([]);
  const [objects, setObjects] = useState<BookingObject[]>([]);
  const [statusesMap, setStatusesMap] = useState<Record<string, BookingStatus>>({});
  const [objectsMap, setObjectsMap] = useState<Record<string, BookingObject>>({});
  const [usersMap, setUsersMap] = useState<Record<string, AdminUser>>({});

  useEffect(() => {
    if (!isAuthenticated) return;
    if (!isAdmin) {
      router.replace("/");
      return;
    }
    Promise.all([getAllBookings(), getAllObjects(), getBookingStatuses(), getAllUsers()])
      .then(([allBookings, allObjects, statusList, users]) => {
        setBookings(allBookings);
        setObjects(allObjects);
        setStatusesMap(Object.fromEntries(statusList.map((s) => [s.id, s])));
        setObjectsMap(Object.fromEntries(allObjects.map((o) => [o.id, o])));
        setUsersMap(Object.fromEntries(users.map((u) => [u.id, u])));
      })
      .finally(() => setLoading(false));
  }, [isAuthenticated, isAdmin, router]);

  const totalBookings = bookings.length;
  const pendingBookings = bookings.filter((b) =>
    isPendingStatus(statusesMap[b.statusId]?.name.value ?? ""),
  );
  const confirmedCount = bookings.filter((b) =>
    isConfirmedStatus(statusesMap[b.statusId]?.name.value ?? ""),
  ).length;
  const activeObjectsCount = objects.filter((o) => o.isActive).length;

  const pendingColumns: ColumnsType<MyBooking> = [
    {
      title: "Клиент",
      key: "client",
      render: (_, b) => {
        const user = usersMap[b.userId];
        return user ? (
          <Text strong>{getFullName(user)}</Text>
        ) : (
          <Text type="secondary">—</Text>
        );
      },
    },
    {
      title: "Объект",
      key: "object",
      ellipsis: true,
      render: (_, b) => objectsMap[b.bookingObjectId]?.name.value ?? "—",
    },
    {
      title: "Период",
      key: "period",
      render: (_, b) => (
        <span className="admin-bookings__period">
          {formatDt(b.startDateTime)}
          <br />
          {formatDt(b.endDateTime)}
        </span>
      ),
    },
    {
      title: "Статус",
      key: "status",
      width: 190,
      render: (_, b) => {
        const name = statusesMap[b.statusId]?.name.value ?? "—";
        return <Tag color={getStatusColor(name)}>{name}</Tag>;
      },
    },
  ];

  const stats = [
    {
      label: "Всего бронирований",
      value: totalBookings,
      mod: "blue",
      icon: "📋",
    },
    {
      label: "Ожидают подтверждения",
      value: pendingBookings.length,
      mod: "orange",
      icon: "⏳",
    },
    {
      label: "Подтверждённых",
      value: confirmedCount,
      mod: "green",
      icon: "✅",
    },
    {
      label: "Активных объектов",
      value: activeObjectsCount,
      mod: "purple",
      icon: "🏡",
    },
  ];

  if (loading) {
    return (
      <div className="my-bookings__centered">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div className="admin-dashboard">
      <Title level={2} className="admin-dashboard__title">
        Панель администратора
      </Title>

      {/* Stat cards */}
      <Row gutter={[16, 16]} className="admin-dashboard__stats">
        {stats.map((s) => (
          <Col xs={24} sm={12} lg={6} key={s.label}>
            <Card className={`stat-card stat-card--${s.mod}`}>
              <div className="stat-card__icon">{s.icon}</div>
              <div className="stat-card__value">{s.value}</div>
              <div className="stat-card__label">{s.label}</div>
            </Card>
          </Col>
        ))}
      </Row>

      {/* Pending bookings */}
      <div className="admin-dashboard__section">
        <div className="admin-dashboard__section-header">
          <Title level={4} className="admin-dashboard__section-title">
            Требуют подтверждения
          </Title>
          <Link href="/admin/bookings">
            <Button type="link" size="small">
              Все бронирования →
            </Button>
          </Link>
        </div>

        {pendingBookings.length === 0 ? (
          <div className="admin-dashboard__empty">
            <Text type="secondary">Нет бронирований, ожидающих подтверждения</Text>
          </div>
        ) : (
          <Table
            dataSource={pendingBookings.slice(0, 8)}
            columns={pendingColumns}
            rowKey="id"
            pagination={false}
            size="small"
            scroll={{ x: 600 }}
          />
        )}
      </div>

      {/* Quick nav */}
      <div className="admin-dashboard__section">
        <Title level={4} className="admin-dashboard__section-title">
          Управление
        </Title>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12}>
            <Link href="/admin/bookings">
              <Card hoverable className="admin-nav-card">
                <div className="admin-nav-card__icon">📅</div>
                <Title level={5} className="admin-nav-card__title">
                  Бронирования
                </Title>
                <Text type="secondary">
                  Просмотр, подтверждение и отмена всех бронирований
                </Text>
              </Card>
            </Link>
          </Col>
          <Col xs={24} sm={12}>
            <Link href="/objects">
              <Card hoverable className="admin-nav-card">
                <div className="admin-nav-card__icon">🏡</div>
                <Title level={5} className="admin-nav-card__title">
                  Объекты
                </Title>
                <Text type="secondary">
                  Создание, редактирование и удаление объектов бронирования
                </Text>
              </Card>
            </Link>
          </Col>
        </Row>
      </div>
    </div>
  );
}
