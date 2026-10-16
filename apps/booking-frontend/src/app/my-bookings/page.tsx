"use client";

import { Alert, Button, Card, Col, Empty, Input, Popconfirm, Row, Select, Spin, Tag, message } from "antd";
import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useBookingMaps } from "../hooks/useBookingMaps";
import {
  MyBooking,
  cancelBooking,
  getMyBookings,
  getStatusColor,
  isCancelledStatus,
} from "../services/bookings";

const formatDateTime = (iso: string) =>
  new Date(iso).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

export default function MyBookingsPage() {
  const { user, isAuthenticated, isDemo } = useAuth();
  const maps = useBookingMaps();
  const [messageApi, contextHolder] = message.useMessage();
  const [bookings, setBookings] = useState<MyBooking[] | null>(null);
  const [bookingsError, setBookingsError] = useState<string | null>(null);
  const [bookingsRetryKey, setBookingsRetryKey] = useState(0);
  const [cancelling, setCancelling] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedStatusId, setSelectedStatusId] = useState<string | null>(null);
  const [pageOpenedAt] = useState(() => Date.now());

  useEffect(() => {
    if (!user) return;
    let cancelled = false;
    getMyBookings(user.id)
      .then((items) => {
        if (!cancelled) setBookings(items);
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setBookingsError(reason instanceof Error ? reason.message : "Не удалось загрузить заявки");
        }
      });
    return () => {
      cancelled = true;
    };
  }, [user, bookingsRetryKey]);

  const retryAll = () => {
    setBookings(null);
    setBookingsError(null);
    setBookingsRetryKey((key) => key + 1);
    maps.retry();
  };

  const handleCancel = async (bookingId: string) => {
    if (!user || cancelling) return;
    setCancelling(bookingId);
    try {
      await cancelBooking(bookingId, user.id);
      setBookings(await getMyBookings(user.id));
      messageApi.success("Демонстрационная заявка отменена, интервал освобождён");
    } catch (error) {
      messageApi.error(error instanceof Error ? error.message : "Не удалось отменить заявку");
    } finally {
      setCancelling(null);
    }
  };

  const loading = maps.loading || (Boolean(user) && bookings === null && !bookingsError);
  const error = bookingsError ?? maps.error;
  const hasActiveFilters = searchQuery !== "" || selectedStatusId !== null;
  const filteredBookings = (bookings ?? []).filter((booking) => {
    const objectName = maps.objectsMap[booking.bookingObjectId]?.name.value.toLowerCase() ?? "";
    if (searchQuery && !objectName.includes(searchQuery.toLowerCase())) return false;
    return !selectedStatusId || booking.statusId === selectedStatusId;
  });

  if (!isAuthenticated && !loading) {
    return (
      <div className="my-bookings__centered">
        <Empty description="Войдите, чтобы увидеть свои бронирования">
          <Link href="/login"><Button type="primary">Войти</Button></Link>
        </Empty>
      </div>
    );
  }

  if (loading) {
    return <div className="my-bookings__centered"><Spin size="large" /></div>;
  }

  if (error) {
    return (
      <section className="my-bookings__state">
        <Alert
          type="error"
          showIcon
          title="Не удалось загрузить мои заявки"
          description={error}
          action={<Button type="primary" onClick={retryAll}>Повторить</Button>}
        />
      </section>
    );
  }

  return (
    <div className="my-bookings-page">
      {contextHolder}
      <header className="my-bookings__header">
        <div>
          <p className="section-kicker">Личный кабинет</p>
          <h1>Мои бронирования</h1>
          <p>
            {isDemo
              ? "Демонстрационные заявки хранятся только до полной перезагрузки приложения."
              : "Здесь отображаются заявки текущего пользователя."}
          </p>
        </div>
        <span className="my-bookings__count">{bookings?.length ?? 0}</span>
      </header>

      {(bookings?.length ?? 0) > 0 && (
        <div className="my-bookings__filters">
          <Input.Search
            aria-label="Поиск по названию объекта"
            placeholder="Поиск по названию объекта"
            allowClear
            value={searchQuery}
            onChange={(event) => setSearchQuery(event.target.value)}
          />
          <Select
            aria-label="Фильтр по статусу"
            placeholder="Статус"
            allowClear
            value={selectedStatusId}
            onChange={(value: string | undefined) => setSelectedStatusId(value ?? null)}
            options={maps.statuses.map((status) => ({
              value: status.id,
              label: status.name.value,
            }))}
          />
          {hasActiveFilters && (
            <Button onClick={() => { setSearchQuery(""); setSelectedStatusId(null); }}>
              Сбросить
            </Button>
          )}
        </div>
      )}

      {(bookings?.length ?? 0) === 0 ? (
        <div className="my-bookings__centered">
          <Empty description="У вас пока нет демонстрационных заявок">
            <Link href="/objects"><Button type="primary">Перейти в каталог</Button></Link>
          </Empty>
        </div>
      ) : filteredBookings.length === 0 ? (
        <div className="my-bookings__centered">
          <Empty description="По выбранным фильтрам заявок не найдено">
            <Button onClick={() => { setSearchQuery(""); setSelectedStatusId(null); }}>
              Сбросить фильтры
            </Button>
          </Empty>
        </div>
      ) : (
        <Row gutter={[20, 20]}>
          {filteredBookings.map((booking) => {
            const object = maps.objectsMap[booking.bookingObjectId];
            const status = maps.statusesMap[booking.statusId];
            const statusName = status?.name.value ?? "Неизвестный статус";
            const cancelled = isCancelledStatus(statusName);
            const finished = new Date(booking.endDateTime).getTime() <= pageOpenedAt;
            return (
              <Col xs={24} md={12} xl={8} key={booking.id}>
                <Card className="booking-item-card">
                  <div className="booking-item-card__header">
                    <div>
                      <small>Заявка {booking.id}</small>
                      <h2>{object?.name.value ?? "Объект"}</h2>
                    </div>
                    <Tag color={getStatusColor(statusName)}>{statusName}</Tag>
                  </div>
                  <div className="booking-item-card__dates">
                    <span>Интервал</span>
                    <strong>{formatDateTime(booking.startDateTime)} — {formatDateTime(booking.endDateTime)}</strong>
                  </div>
                  <div className="booking-item-card__meta">Гостей: <strong>{booking.guestsCount}</strong></div>
                  {booking.comment && <p className="booking-item-card__comment">{booking.comment.value}</p>}
                  {!cancelled && !finished && (
                    <div className="booking-item-card__footer">
                      <Popconfirm
                        title="Отменить демонстрационную заявку?"
                        description="Интервал снова станет свободным в текущей сессии."
                        okText="Да, отменить"
                        cancelText="Назад"
                        okButtonProps={{ danger: true, loading: cancelling === booking.id }}
                        onConfirm={() => handleCancel(booking.id)}
                      >
                        <Button danger loading={cancelling === booking.id} disabled={cancelling !== null}>
                          Отменить заявку
                        </Button>
                      </Popconfirm>
                    </div>
                  )}
                  {!cancelled && finished && (
                    <p className="booking-item-card__history">
                      Завершённая заявка доступна только для просмотра
                    </p>
                  )}
                </Card>
              </Col>
            );
          })}
        </Row>
      )}
    </div>
  );
}
