"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Alert, Button, Image, Spin, Tag, Typography } from "antd";
import Link from "next/link";
import { BookingObject, getImageUrl, getObjectById } from "../../services/booking.objects";
import { ObjectType, getObjectTypes } from "../../services/object-types";
import { useAuth } from "../../context/AuthContext";
import { BookingFormModal } from "../../components/BookingFormModal";

const { Title, Paragraph, Text } = Typography;

export default function ObjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const { isAuthenticated, user } = useAuth();

  const [object, setObject] = useState<BookingObject | null>(null);
  const [objectTypes, setObjectTypes] = useState<ObjectType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [bookingTarget, setBookingTarget] = useState<BookingObject | null>(null);
  const [bookingSuccess, setBookingSuccess] = useState(false);
  const [bookingId, setBookingId] = useState<string | null>(null);
  const [retryKey, setRetryKey] = useState(0);

  useEffect(() => {
    if (!id) return;
    Promise.all([getObjectById(id), getObjectTypes()])
      .then(([obj, types]) => {
        setObject(obj);
        setObjectTypes(types);
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [id, retryKey]);

  const retry = () => {
    setLoading(true);
    setError(null);
    setRetryKey((key) => key + 1);
  };

  if (loading) {
    return (
      <div className="object-detail__loading">
        <Spin size="large" />
      </div>
    );
  }

  if (error || !object) {
    return (
      <div className="object-detail__error">
        <Title level={3}>Не удалось загрузить объект</Title>
        <Paragraph>{error}</Paragraph>
        <Button type="primary" onClick={retry}>Повторить</Button>
        <Button onClick={() => router.push("/objects")}>
          ← Вернуться к объектам
        </Button>
      </div>
    );
  }

  const imageUrl = getImageUrl(object.imageUrl);
  const objectType = objectTypes.find((t) => t.id === object.objectTypeId);

  return (
    <div className="object-detail">
      <Button
        type="link"
        className="object-detail__back"
        onClick={() => router.push("/objects")}
      >
        ← Все объекты
      </Button>

      <div className="object-detail__card">
        {imageUrl ? (
          <div className="object-detail__image">
            <Image src={imageUrl} alt={object.name.value} preview={false} />
          </div>
        ) : (
          <div className="object-detail__image object-detail__image--placeholder" role="img" aria-label="Стилизованный домик среди деревьев">
            <span className="booking-card__sun" />
            <span className="booking-card__forest" />
            <span className="booking-card__house" />
          </div>
        )}

        <div className="object-detail__body">
          <div className="object-detail__header">
            <Title level={2} className="object-detail__title">
              {object.name.value}
            </Title>
            <Tag
              color={object.isActive ? "success" : "default"}
              className="object-detail__status"
            >
              {object.isActive ? "Активен" : "Недоступен"}
            </Tag>
          </div>

          <Paragraph className="object-detail__description">
            {object.description.value}
          </Paragraph>

          <div className="object-detail__meta">
            <div className="object-detail__meta-item">
              <Text className="object-detail__meta-label">Условная цена</Text>
              <Text strong className="object-detail__meta-value object-detail__price">
                {object.price.toLocaleString("ru-RU")} ₽
              </Text>
            </div>
            <div className="object-detail__meta-item">
              <Text className="object-detail__meta-label">Вместимость</Text>
              <Text strong className="object-detail__meta-value">
                {object.capacity} чел.
              </Text>
            </div>
            {objectType && (
              <div className="object-detail__meta-item">
                <Text className="object-detail__meta-label">Тип</Text>
                <Text strong className="object-detail__meta-value">
                  {objectType.name.value}
                </Text>
              </div>
            )}
            <div className="object-detail__meta-item">
              <Text className="object-detail__meta-label">Добавлен</Text>
              <Text className="object-detail__meta-value">
                {new Date(object.createdAt).toLocaleDateString("ru-RU", {
                  day: "numeric",
                  month: "long",
                  year: "numeric",
                })}
              </Text>
            </div>
          </div>

          {bookingSuccess && (
            <Alert
              className="object-detail__success"
              type="success"
              showIcon
              title={`Демонстрационная заявка ${bookingId ?? ""} создана`}
              description="Реальная заявка в Backend не отправлялась."
              action={<Link href="/my-bookings"><Button>Мои бронирования</Button></Link>}
            />
          )}

          <div className="object-detail__actions">
            {isAuthenticated && object.isActive && (
              <Button
                type="primary"
                size="large"
                onClick={() => setBookingTarget(object)}
              >
                Забронировать
              </Button>
            )}
            <Button size="large" onClick={() => router.push("/objects")}>
              Назад
            </Button>
          </div>
        </div>
      </div>

      {bookingTarget && user && (
        <BookingFormModal
          booking={bookingTarget}
          objectTypes={objectTypes}
          user={user}
          onClose={() => setBookingTarget(null)}
          onSuccess={(createdBookingId) => {
            setBookingSuccess(true);
            setBookingId(createdBookingId);
          }}
        />
      )}
    </div>
  );
}
