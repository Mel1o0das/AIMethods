"use client";

import { useEffect, useState } from "react";
import {
  Button,
  Drawer,
  Empty,
  Input,
  Popconfirm,
  Select,
  Spin,
  Table,
  Tag,
  Typography,
  message,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useRouter } from "next/navigation";
import { useAuth } from "../../context/AuthContext";
import {
  BookingStatus,
  MyBooking,
  cancelBooking,
  confirmBooking,
  getAllBookings,
  getBookingStatuses,
  getStatusColor,
  isCancelledStatus,
  isConfirmedStatus,
} from "../../services/bookings";
import { BookingObject, getAllObjects } from "../../services/booking.objects";
import { AdminUser, getAllUsers, getFullName } from "../../services/users";

const { Title, Text } = Typography;

const formatDt = (iso: string) =>
  new Date(iso).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

export default function AdminBookingsPage() {
  const { isAdmin, isAuthenticated } = useAuth();
  const router = useRouter();
  const [messageApi, contextHolder] = message.useMessage();

  const [bookings, setBookings] = useState<MyBooking[]>([]);
  const [objectsMap, setObjectsMap] = useState<Record<string, BookingObject>>(
    {},
  );
  const [statusesMap, setStatusesMap] = useState<Record<string, BookingStatus>>(
    {},
  );
  const [usersMap, setUsersMap] = useState<Record<string, AdminUser>>({});
  const [statuses, setStatuses] = useState<BookingStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [selectedStatusId, setSelectedStatusId] = useState<string | null>(null);
  const [clientDrawer, setClientDrawer] = useState<{
    user: AdminUser;
    booking: MyBooking;
  } | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;
    if (!isAdmin) {
      router.replace("/");
      return;
    }
    Promise.all([
      getAllBookings(),
      getAllObjects(),
      getBookingStatuses(),
      getAllUsers(),
    ])
      .then(([allBookings, objects, statusList, users]) => {
        setBookings(allBookings);
        setObjectsMap(Object.fromEntries(objects.map((o) => [o.id, o])));
        setStatusesMap(Object.fromEntries(statusList.map((s) => [s.id, s])));
        setUsersMap(Object.fromEntries(users.map((u) => [u.id, u])));
        setStatuses(statusList);
      })
      .catch(() => messageApi.error("Не удалось загрузить данные"))
      .finally(() => setLoading(false));
  }, [isAuthenticated, isAdmin, messageApi, router]);

  const patchStatus = (id: string, newStatusId: string) =>
    setBookings((prev) =>
      prev.map((b) => (b.id === id ? { ...b, statusId: newStatusId } : b)),
    );

  const findStatusId = (predicate: (name: string) => boolean) =>
    Object.entries(statusesMap).find(([, s]) => predicate(s.name.value))?.[0] ??
    "";

  const handleConfirm = async (bookingId: string) => {
    setActionLoading(bookingId);
    try {
      await confirmBooking(bookingId);
      patchStatus(bookingId, findStatusId(isConfirmedStatus));
      messageApi.success("Бронирование подтверждено");
    } catch (e) {
      messageApi.error((e as Error).message);
    } finally {
      setActionLoading(null);
    }
  };

  const handleCancel = async (bookingId: string) => {
    setActionLoading(bookingId);
    try {
      await cancelBooking(bookingId);
      patchStatus(bookingId, findStatusId(isCancelledStatus));
      messageApi.success("Бронирование отменено");
    } catch (e) {
      messageApi.error((e as Error).message);
    } finally {
      setActionLoading(null);
    }
  };

  const filteredBookings = bookings.filter((b) => {
    if (searchQuery) {
      const name =
        objectsMap[b.bookingObjectId]?.name.value.toLowerCase() ?? "";
      if (!name.includes(searchQuery.toLowerCase())) return false;
    }
    if (selectedStatusId && b.statusId !== selectedStatusId) return false;
    return true;
  });

  const columns: ColumnsType<MyBooking> = [
    {
      title: "Клиент",
      key: "client",
      render: (_, b) => {
        const user = usersMap[b.userId];
        if (!user) return <span className="admin-bookings__no-comment">—</span>;
        return (
          <Button
            type="link"
            size="small"
            className="admin-bookings__client-btn"
            onClick={() => setClientDrawer({ user, booking: b })}
          >
            {getFullName(user)}
          </Button>
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
      responsive: ["sm"],
      render: (_, b) => (
        <span className="admin-bookings__period">
          {formatDt(b.startDateTime)}
          <br />
          {formatDt(b.endDateTime)}
        </span>
      ),
    },
    {
      title: "Гостей",
      dataIndex: "guestsCount",
      key: "guestsCount",
      width: 80,
      align: "center",
      responsive: ["md"],
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
    {
      title: "Создано",
      key: "createdAt",
      width: 150,
      responsive: ["md"],
      render: (_, b) => formatDt(b.createdAt),
    },
    {
      title: "Действия",
      key: "actions",
      width: 190,
      render: (_, b) => {
        const statusName = statusesMap[b.statusId]?.name.value ?? "";
        const cancelled = isCancelledStatus(statusName);
        const confirmed = isConfirmedStatus(statusName);
        const busy = actionLoading === b.id;

        return (
          <div className="admin-bookings__row-actions">
            {!confirmed && !cancelled && (
              <Popconfirm
                title="Подтвердить бронирование?"
                okText="Да"
                cancelText="Нет"
                onConfirm={() => handleConfirm(b.id)}
              >
                <Button type="primary" size="small" loading={busy}>
                  Подтвердить
                </Button>
              </Popconfirm>
            )}
            {!cancelled && (
              <Popconfirm
                title="Отменить бронирование?"
                description="Это действие нельзя отменить."
                okText="Да, отменить"
                cancelText="Назад"
                okButtonProps={{ danger: true }}
                onConfirm={() => handleCancel(b.id)}
              >
                <Button danger size="small" loading={busy}>
                  Отменить
                </Button>
              </Popconfirm>
            )}
          </div>
        );
      },
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
    <>
      {contextHolder}

      <div className="my-bookings__header">
        <Title level={2} className="my-bookings__title">
          Все бронирования
        </Title>
        {bookings.length > 0 && (
          <span className="my-bookings__count">{bookings.length}</span>
        )}
      </div>

      <div className="objects-filters">
        <Input.Search
          placeholder="Поиск по объекту"
          allowClear
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onSearch={(v) => setSearchQuery(v)}
          className="objects-filters__search"
        />
        <Select
          placeholder="Статус"
          allowClear
          value={selectedStatusId}
          onChange={(v: string | undefined) => setSelectedStatusId(v ?? null)}
          className="objects-filters__select"
          options={statuses.map((s) => ({
            value: s.id,
            label: (
              <Tag color={getStatusColor(s.name.value)} variant="filled">
                {s.name.value}
              </Tag>
            ),
          }))}
        />
        {(searchQuery || selectedStatusId) && (
          <Button
            onClick={() => {
              setSearchQuery("");
              setSelectedStatusId(null);
            }}
          >
            Сбросить
          </Button>
        )}
      </div>

      {filteredBookings.length === 0 ? (
        <Empty
          className="admin-bookings__empty"
          description={
            searchQuery || selectedStatusId
              ? "Бронирования не найдены. Попробуйте изменить фильтры."
              : "Бронирований пока нет"
          }
        />
      ) : (
        <Table
          dataSource={filteredBookings}
          columns={columns}
          rowKey="id"
          pagination={{ pageSize: 20, showSizeChanger: false }}
          className="admin-bookings__table"
        />
      )}

      <Drawer
        open={clientDrawer !== null}
        onClose={() => setClientDrawer(null)}
        title="Информация о клиенте"
        size={360}
      >
        {clientDrawer && (
          <div className="client-drawer">
            <div className="client-drawer__field">
              <Text className="client-drawer__label">ФИО</Text>
              <Text strong className="client-drawer__value">
                {getFullName(clientDrawer.user)}
              </Text>
            </div>

            <div className="client-drawer__field">
              <Text className="client-drawer__label">Email</Text>
              <a
                href={`mailto:${clientDrawer.user.email.value}`}
                className="client-drawer__link"
              >
                {clientDrawer.user.email.value}
              </a>
            </div>

            <div className="client-drawer__field">
              <Text className="client-drawer__label">Телефон</Text>
              <a
                href={`tel:${clientDrawer.user.phone.value}`}
                className="client-drawer__link"
              >
                {clientDrawer.user.phone.value}
              </a>
            </div>

            <div className="client-drawer__field">
              <Text className="client-drawer__label">Зарегистрирован</Text>
              <Text className="client-drawer__value">
                {new Date(clientDrawer.user.createdAt).toLocaleDateString(
                  "ru-RU",
                  { day: "2-digit", month: "long", year: "numeric" },
                )}
              </Text>
            </div>

            <div className="client-drawer__field">
              <Text className="client-drawer__label">Комментарий к брони</Text>
              {clientDrawer.booking.comment?.value ? (
                <Text className="client-drawer__value">
                  {clientDrawer.booking.comment.value}
                </Text>
              ) : (
                <Text className="admin-bookings__no-comment">Не указан</Text>
              )}
            </div>
          </div>
        )}
      </Drawer>
    </>
  );
}
