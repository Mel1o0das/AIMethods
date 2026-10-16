"use client";

import { Alert, Button, DatePicker, Form, Input, InputNumber, Modal, Result, Spin } from "antd";
import type { Dayjs } from "dayjs";
import dayjs from "dayjs";
import Link from "next/link";
import { useEffect, useState } from "react";
import { BookingObject } from "../services/booking.objects";
import {
  BookingStatus,
  OccupiedSlot,
  createBooking,
  getBookingStatuses,
  getOccupiedSlotsForRange,
} from "../services/bookings";
import { ObjectType } from "../services/object-types";

const { TextArea } = Input;

interface BookingFormValues {
  startDateTime: Dayjs;
  endDateTime: Dayjs;
  guestsCount: number;
  comment?: string;
}

interface Props {
  booking: BookingObject;
  objectTypes: ObjectType[];
  user: { id: string };
  onClose: () => void;
  onSuccess: (bookingId: string) => void;
}

const intervalsOverlap = (
  start: Dayjs,
  end: Dayjs,
  slot: OccupiedSlot,
): boolean => start.isBefore(dayjs(slot.endDateTime)) && end.isAfter(dayjs(slot.startDateTime));

export const BookingFormModal = ({ booking, objectTypes, user, onClose, onSuccess }: Props) => {
  const [form] = Form.useForm<BookingFormValues>();
  const [statuses, setStatuses] = useState<BookingStatus[]>([]);
  const [occupiedSlots, setOccupiedSlots] = useState<OccupiedSlot[]>([]);
  const [availabilityLoading, setAvailabilityLoading] = useState(true);
  const [availabilityError, setAvailabilityError] = useState<string | null>(null);
  const [retryKey, setRetryKey] = useState(0);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [successId, setSuccessId] = useState<string | null>(null);

  const bookingObjectType = objectTypes.find((type) => type.id === booking.objectTypeId);
  const isSauna = bookingObjectType?.name.value.toLowerCase().includes("баня") ?? false;

  useEffect(() => {
    let cancelled = false;
    const from = dayjs().startOf("day").toISOString();
    const to = dayjs().add(365, "day").endOf("day").toISOString();

    Promise.all([
      getBookingStatuses(),
      getOccupiedSlotsForRange(booking.id, from, to),
    ])
      .then(([statusList, slots]) => {
        if (!cancelled) {
          setStatuses(statusList);
          setOccupiedSlots(slots);
        }
      })
      .catch((error: unknown) => {
        if (!cancelled) {
          setAvailabilityError(
            error instanceof Error ? error.message : "Не удалось проверить занятость",
          );
        }
      })
      .finally(() => {
        if (!cancelled) setAvailabilityLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [booking.id, retryKey]);

  const retryAvailability = () => {
    setAvailabilityLoading(true);
    setAvailabilityError(null);
    setRetryKey((key) => key + 1);
  };

  const normalizeInterval = (values: BookingFormValues) => {
    if (isSauna) {
      return {
        start: values.startDateTime.second(0).millisecond(0),
        end: values.endDateTime.second(0).millisecond(0),
      };
    }
    return {
      start: values.startDateTime.hour(15).minute(0).second(0).millisecond(0),
      end: values.endDateTime.hour(12).minute(0).second(0).millisecond(0),
    };
  };

  const onSubmit = async (values: BookingFormValues) => {
    setSubmitError(null);
    const pendingStatus = statuses.find((status) =>
      status.name.value.toLowerCase().includes("ожидает"),
    );
    if (!pendingStatus) {
      setSubmitError("Статус «Ожидает подтверждения» не загружен. Повторите проверку.");
      return;
    }

    const { start, end } = normalizeInterval(values);
    if (occupiedSlots.some((slot) => intervalsOverlap(start, end, slot))) {
      setSubmitError("Выбранный интервал пересекается с занятой демонстрационной заявкой.");
      return;
    }

    setSubmitting(true);
    try {
      const bookingId = await createBooking({
        userId: user.id,
        bookingObjectId: booking.id,
        startDateTime: start.toISOString(),
        endDateTime: end.toISOString(),
        guestsCount: values.guestsCount,
        comment: values.comment,
        statusId: pendingStatus.id,
      });
      setSuccessId(bookingId);
      onSuccess(bookingId);
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : "Не удалось создать заявку");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      open
      className="booking-form-modal"
      onCancel={onClose}
      title={`Демо-заявка: ${booking.name.value}`}
      width={560}
      footer={null}
      destroyOnHidden
    >
      {successId ? (
        <Result
          status="success"
          title="Демонстрационная заявка создана"
          subTitle={`Номер: ${successId}. Реальная заявка в Backend не отправлялась.`}
          extra={[
            <Link href="/my-bookings" key="bookings" onClick={onClose}>
              <Button type="primary">Мои бронирования</Button>
            </Link>,
            <Button key="close" onClick={onClose}>Закрыть</Button>,
          ]}
        />
      ) : availabilityLoading ? (
        <div className="booking-form__state" role="status">
          <Spin />
          <span>Проверяем занятость и статусы…</span>
        </div>
      ) : availabilityError ? (
        <Alert
          type="error"
          showIcon
          title="Не удалось проверить занятость"
          description={availabilityError}
          action={<Button onClick={retryAvailability}>Повторить</Button>}
        />
      ) : (
        <Form form={form} layout="vertical" onFinish={onSubmit} className="booking-form">
          <div className="booking-form__hint">
            <strong>Демонстрационные условия</strong>
            <span>Вместимость: до {booking.capacity} гостей · условная сумма {booking.price.toLocaleString("ru-RU")} ₽</span>
            <span className="booking-form__hint-times">
              {isSauna
                ? "Демо-допущение: выбор времени с шагом 30 минут."
                : "Демо-допущение: заезд 15:00, выезд 12:00."}
            </span>
          </div>

          <Alert
            className="booking-form__availability"
            type={occupiedSlots.length === 0 ? "success" : "info"}
            showIcon
            title={
              occupiedSlots.length === 0
                ? "В ближайший год занятых интервалов не найдено"
                : `Учтено занятых интервалов: ${occupiedSlots.length}`
            }
          />

          <Form.Item
            label={isSauna ? "Начало сеанса" : "Дата заезда"}
            name="startDateTime"
            rules={[
              { required: true, message: "Укажите начало" },
              {
                validator: (_, value: Dayjs | undefined) =>
                  !value || value.isAfter(dayjs())
                    ? Promise.resolve()
                    : Promise.reject(new Error("Начало должно быть в будущем")),
              },
            ]}
          >
            <DatePicker
              className="booking-form__datepicker"
              placeholder={isSauna ? "Выберите дату и время начала" : "Выберите дату заезда"}
              disabledDate={(current) => current.isBefore(dayjs().startOf("day"))}
              showTime={isSauna ? { format: "HH:mm", minuteStep: 30 } : false}
              format={isSauna ? "DD.MM.YYYY HH:mm" : "DD.MM.YYYY"}
            />
          </Form.Item>

          <Form.Item
            label={isSauna ? "Конец сеанса" : "Дата выезда"}
            name="endDateTime"
            dependencies={["startDateTime"]}
            rules={[
              { required: true, message: "Укажите окончание" },
              ({ getFieldValue }) => ({
                validator: (_, value: Dayjs | undefined) => {
                  const start = getFieldValue("startDateTime") as Dayjs | undefined;
                  return !value || !start || value.isAfter(start)
                    ? Promise.resolve()
                    : Promise.reject(new Error("Окончание должно быть позже начала"));
                },
              }),
            ]}
          >
            <DatePicker
              className="booking-form__datepicker"
              placeholder={isSauna ? "Выберите дату и время окончания" : "Выберите дату выезда"}
              disabledDate={(current) => current.isBefore(dayjs().startOf("day"))}
              showTime={isSauna ? { format: "HH:mm", minuteStep: 30 } : false}
              format={isSauna ? "DD.MM.YYYY HH:mm" : "DD.MM.YYYY"}
            />
          </Form.Item>

          <Form.Item
            label="Количество гостей"
            name="guestsCount"
            rules={[
              { required: true, message: "Укажите количество гостей" },
              {
                validator: (_, value: number | undefined) =>
                  value === undefined || (Number.isInteger(value) && value >= 1 && value <= booking.capacity)
                    ? Promise.resolve()
                    : Promise.reject(new Error(`Допустимо от 1 до ${booking.capacity} гостей`)),
              },
            ]}
          >
            <InputNumber min={1} max={booking.capacity} className="booking-form__number" />
          </Form.Item>

          <Form.Item label="Комментарий" name="comment">
            <TextArea rows={3} maxLength={500} showCount />
          </Form.Item>

          {submitError && <Alert className="booking-form__error" type="error" showIcon title={submitError} />}

          <div className="booking-form__status-note">
            Это имитационная заявка. Проверяется только локальная Mock-занятость;
            доступность реального сервера не запрашивается и не подтверждается.
          </div>
          <div className="booking-form__actions">
            <Button onClick={onClose} disabled={submitting}>Закрыть</Button>
            <Button type="primary" htmlType="submit" loading={submitting} disabled={submitting}>
              Создать демо-заявку
            </Button>
          </div>
        </Form>
      )}
    </Modal>
  );
};
