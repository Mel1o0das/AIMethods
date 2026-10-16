"use client";

import {
  Alert,
  Button,
  Card,
  Col,
  Drawer,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Radio,
  Row,
  Select,
  Spin,
  Switch,
  Tag,
  message,
} from "antd";
import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import {
  BookingObject,
  CreateObjectPayload,
  UpdateObjectPayload,
  createObject,
  deleteObject,
  getImageUrl,
  updateObject,
} from "../services/booking.objects";
import { ObjectType } from "../services/object-types";
import { BookingFormModal } from "./BookingFormModal";

interface ObjectFormValues {
  name: string;
  description: string;
  price: number;
  capacity: number;
  imageUrl?: string;
  isActive: boolean;
  objectTypeId?: string;
}

interface Props {
  items: BookingObject[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  loading: boolean;
  search: string;
  sortBy: string;
  sortDir: string;
  typeId: string | null;
  maxPrice: number | null;
  minCapacity: number | null;
  objectTypes: ObjectType[];
  typesLoading: boolean;
  typesError: string | null;
  isDemo: boolean;
  onRetryTypes: () => void;
  onSearchChange: (value: string) => void;
  onSortChange: (sortBy: string, sortDir: string) => void;
  onPageSizeChange: (size: number) => void;
  onPageChange: (page: number) => void;
  onTypeChange: (value: string | null) => void;
  onMaxPriceChange: (value: number | null) => void;
  onMinCapacityChange: (value: number | null) => void;
  onRefresh: () => void;
}

export const BookingObjects = ({
  items,
  totalCount,
  page,
  pageSize,
  totalPages,
  loading,
  search,
  sortBy,
  sortDir,
  typeId,
  maxPrice,
  minCapacity,
  objectTypes,
  typesLoading,
  typesError,
  isDemo,
  onRetryTypes,
  onSearchChange,
  onSortChange,
  onPageSizeChange,
  onPageChange,
  onTypeChange,
  onMaxPriceChange,
  onMinCapacityChange,
  onRefresh,
}: Props) => {
  const { isAdmin, isAuthenticated, user } = useAuth();
  const [bookingTarget, setBookingTarget] = useState<BookingObject | null>(null);
  const [editingObject, setEditingObject] = useState<BookingObject | null>();
  const [objectFormLoading, setObjectFormLoading] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [draftTypeId, setDraftTypeId] = useState<string | null>(typeId);
  const [draftMaxPrice, setDraftMaxPrice] = useState<number | null>(maxPrice);
  const [draftMinCapacity, setDraftMinCapacity] = useState<number | null>(minCapacity);
  const [objectForm] = Form.useForm<ObjectFormValues>();
  const [messageApi, contextHolder] = message.useMessage();

  const hasActiveFilters =
    search !== "" || typeId !== null || maxPrice !== null || minCapacity !== null;
  const activeFilterCount = [typeId, maxPrice, minCapacity].filter(
    (value) => value !== null,
  ).length;

  const resetFilters = () => {
    onSearchChange("");
    onTypeChange(null);
    onMaxPriceChange(null);
    onMinCapacityChange(null);
  };

  const openMobileFilters = () => {
    setDraftTypeId(typeId);
    setDraftMaxPrice(maxPrice);
    setDraftMinCapacity(minCapacity);
    setFiltersOpen(true);
  };

  const applyMobileFilters = () => {
    if (draftTypeId !== typeId) onTypeChange(draftTypeId);
    if (draftMaxPrice !== maxPrice) onMaxPriceChange(draftMaxPrice);
    if (draftMinCapacity !== minCapacity) onMinCapacityChange(draftMinCapacity);
    setFiltersOpen(false);
  };

  const resetMobileFilters = () => {
    setDraftTypeId(null);
    setDraftMaxPrice(null);
    setDraftMinCapacity(null);
    if (hasActiveFilters) resetFilters();
    setFiltersOpen(false);
  };

  useEffect(() => {
    if (editingObject === undefined) return;
    if (editingObject === null) {
      objectForm.resetFields();
      objectForm.setFieldValue("isActive", true);
    } else {
      objectForm.setFieldsValue({
        name: editingObject.name.value,
        description: editingObject.description.value,
        price: editingObject.price,
        capacity: editingObject.capacity,
        imageUrl: editingObject.imageUrl ?? undefined,
        isActive: editingObject.isActive,
        objectTypeId: editingObject.objectTypeId ?? undefined,
      });
    }
  }, [editingObject, objectForm]);

  const closeObjectModal = () => setEditingObject(undefined);

  const onObjectFormSubmit = async (values: ObjectFormValues) => {
    setObjectFormLoading(true);
    try {
      if (editingObject === null) {
        const payload: CreateObjectPayload = {
          ...values,
          imageUrl: values.imageUrl ?? "",
          objectTypeId: values.objectTypeId ?? null,
        };
        await createObject(payload);
        messageApi.success("Объект создан");
      } else if (editingObject !== undefined) {
        const payload: UpdateObjectPayload = {
          ...values,
          imageUrl: values.imageUrl ?? null,
          objectTypeId: values.objectTypeId ?? null,
        };
        await updateObject(editingObject.id, payload);
        messageApi.success("Объект обновлён");
      }
      closeObjectModal();
      onRefresh();
    } catch (error) {
      messageApi.error(error instanceof Error ? error.message : "Ошибка сохранения");
    } finally {
      setObjectFormLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteObject(id);
      messageApi.success("Объект удалён");
      onRefresh();
    } catch (error) {
      messageApi.error(error instanceof Error ? error.message : "Ошибка удаления");
    }
  };

  const getActions = (object: BookingObject) => {
    if (isAdmin) {
      return [
        <Button key="edit" type="text" onClick={() => setEditingObject(object)}>
          Изменить
        </Button>,
        <Popconfirm
          key="delete"
          title="Удалить объект?"
          description="Это действие нельзя отменить."
          okText="Да, удалить"
          cancelText="Отмена"
          okButtonProps={{ danger: true }}
          onConfirm={() => handleDelete(object.id)}
        >
          <Button type="text" danger>Удалить</Button>
        </Popconfirm>,
      ];
    }

    const actions = [
      <Link key="info" href={`/objects/${object.id}`}>
        <Button type="text">Подробнее</Button>
      </Link>,
    ];
    if (isAuthenticated) {
      actions.push(
        <Button key="book" type="text" onClick={() => setBookingTarget(object)}>
          Забронировать
        </Button>,
      );
    }
    return actions;
  };

  const typeOptions = objectTypes.map((type) => ({
    value: type.id,
    label: type.name.value,
  }));

  return (
    <div className="catalog-page">
      {contextHolder}

      <header className="catalog-heading">
        <div>
          <p className="section-kicker">Коллекция мест</p>
          <h1>Бани и глэмпинги</h1>
          <p>
            Найдите место по атмосфере, вместимости и стоимости. Доступность
            интервала проверяется отдельно перед созданием заявки.
          </p>
        </div>
        <div className="catalog-heading__count" aria-live="polite">
          <strong>{totalCount}</strong>
          <span>мест в подборке</span>
        </div>
      </header>

      {isAdmin && (
        <Button type="primary" className="booking-objects__create-btn" onClick={() => setEditingObject(null)}>
          Создать объект
        </Button>
      )}

      <section className="catalog-controls" aria-label="Поиск и фильтры каталога">
        <div className="catalog-search-row">
          <Input.Search
            aria-label="Поиск по названию или описанию"
            placeholder="Поиск по названию или описанию"
            allowClear
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
            onSearch={onSearchChange}
            className="objects-filters__search"
            size="large"
          />
          <Button className="objects-filters-mobile-trigger" size="large" onClick={openMobileFilters}>
            Фильтры{activeFilterCount > 0 ? ` · ${activeFilterCount}` : ""}
          </Button>
        </div>

        <div className="objects-filters objects-filters--desktop">
          <Select
            aria-label="Тип объекта"
            placeholder="Тип объекта"
            allowClear
            loading={typesLoading}
            disabled={typesLoading || Boolean(typesError)}
            value={typeId}
            onChange={(value: string | undefined) => onTypeChange(value ?? null)}
            className="objects-filters__select"
            options={typeOptions}
          />
          <InputNumber
            aria-label="Максимальная цена"
            placeholder="Макс. цена"
            min={0}
            value={maxPrice}
            onChange={(value) => onMaxPriceChange(value ?? null)}
            className="objects-filters__number"
          />
          <InputNumber
            aria-label="Минимальная вместимость"
            placeholder="Мин. гостей"
            min={1}
            value={minCapacity}
            onChange={(value) => onMinCapacityChange(value ?? null)}
            className="objects-filters__number"
          />
          {hasActiveFilters && items.length > 0 && (
            <Button onClick={resetFilters}>Сбросить фильтры</Button>
          )}
        </div>

        {typesError && (
          <Alert
            className="objects-types-error"
            type="warning"
            showIcon
            title="Фильтр по типу временно недоступен"
            description={typesError}
            action={<Button onClick={onRetryTypes}>Повторить</Button>}
          />
        )}

        <div className="objects-toolbar">
          <div className="objects-sort">
            <span className="objects-sort__label">Сортировать</span>
            <Select
              aria-label="Сортировка"
              value={sortBy}
              onChange={(value) => onSortChange(value, sortDir)}
              className="objects-sort__select"
              options={[
                { value: "createdAt", label: "По дате" },
                { value: "name", label: "По названию" },
                { value: "price", label: "По цене" },
                { value: "capacity", label: "По вместимости" },
              ]}
            />
            <Radio.Group
              aria-label="Направление сортировки"
              value={sortDir}
              onChange={(event) => onSortChange(sortBy, event.target.value as string)}
              optionType="button"
              buttonStyle="solid"
            >
              <Radio.Button value="asc" aria-label="По возрастанию">↑</Radio.Button>
              <Radio.Button value="desc" aria-label="По убыванию">↓</Radio.Button>
            </Radio.Group>
          </div>

          <div className="objects-page-size">
            <span className="objects-page-size__label">Показывать</span>
            <Radio.Group
              aria-label="Количество объектов на странице"
              value={pageSize}
              onChange={(event) => onPageSizeChange(event.target.value as number)}
              optionType="button"
              buttonStyle="solid"
            >
              <Radio.Button value={5}>5</Radio.Button>
              <Radio.Button value={10}>10</Radio.Button>
              <Radio.Button value={15}>15</Radio.Button>
            </Radio.Group>
          </div>
        </div>
      </section>

      {isDemo && <p className="catalog-demo-prices">Все суммы — демонстрационные и условные.</p>}

      <div className="objects-grid-wrapper" aria-busy={loading}>
        {loading && <div className="objects-grid-overlay"><Spin size="large" /></div>}
        <Row gutter={[20, 20]}>
          {items.length === 0 && !loading ? (
            <Col span={24}>
              <Empty
                description={
                  hasActiveFilters
                    ? "По выбранным условиям ничего не найдено."
                    : "В каталоге пока нет объектов."
                }
                className="objects-filters__empty"
              >
                {hasActiveFilters && <Button onClick={resetFilters}>Сбросить фильтры</Button>}
              </Empty>
            </Col>
          ) : (
            items.map((object) => {
              const imageUrl = getImageUrl(object.imageUrl);
              const objectType = objectTypes.find((type) => type.id === object.objectTypeId);
              return (
                <Col key={object.id} xs={24} sm={12} lg={8} className="booking-cards-col">
                  <Card
                    className={`booking-card${isAdmin && !object.isActive ? " booking-card--inactive" : ""}`}
                    cover={
                      <div className="booking-card__cover-wrapper">
                        {imageUrl ? (
                          // eslint-disable-next-line @next/next/no-img-element
                          <img src={imageUrl} alt={object.name.value} />
                        ) : (
                          <div className="booking-card__cover-placeholder" role="img" aria-label="Стилизованный домик среди деревьев">
                            <span className="booking-card__sun" />
                            <span className="booking-card__forest" />
                            <span className="booking-card__house" />
                          </div>
                        )}
                        {objectType && <span className="booking-card__type">{objectType.name.value}</span>}
                      </div>
                    }
                    actions={getActions(object)}
                  >
                    <Card.Meta
                      title={
                        <div className="booking-card__title">
                          <span className="booking-card__name">{object.name.value}</span>
                          {isAdmin && !object.isActive && <Tag color="red">Неактивен</Tag>}
                        </div>
                      }
                      description={<span className="booking-card__description">{object.description.value}</span>}
                    />
                    <div className="booking-card__meta-row">
                      <span>До {object.capacity} гостей</span>
                      <span className="booking-card__price-block">
                        <small>{isDemo ? "Условная цена" : "Цена"}</small>
                        <strong>{object.price.toLocaleString("ru-RU")} ₽</strong>
                      </span>
                    </div>
                  </Card>
                </Col>
              );
            })
          )}
        </Row>
      </div>

      {totalCount > 0 && (
        <nav className="objects-pagination" aria-label="Пагинация каталога">
          <Button disabled={page <= 1 || loading} onClick={() => onPageChange(page - 1)}>← Назад</Button>
          <span className="objects-pagination__info">Страница {page} из {totalPages}</span>
          <Button disabled={page >= totalPages || loading} onClick={() => onPageChange(page + 1)}>Вперёд →</Button>
          <span className="objects-pagination__total">Всего: {totalCount}</span>
        </nav>
      )}

      <Drawer
        title="Фильтры каталога"
        placement="bottom"
        size={420}
        open={filtersOpen}
        onClose={() => setFiltersOpen(false)}
        className="objects-filter-drawer"
      >
        <div className="objects-filter-drawer__fields">
          <label>
            <span>Тип объекта</span>
            <Select
              aria-label="Тип объекта в мобильных фильтрах"
              placeholder="Любой тип"
              allowClear
              loading={typesLoading}
              disabled={typesLoading || Boolean(typesError)}
              value={draftTypeId}
              onChange={(value: string | undefined) => setDraftTypeId(value ?? null)}
              options={typeOptions}
            />
          </label>
          <label>
            <span>Максимальная цена</span>
            <InputNumber
              aria-label="Максимальная цена в мобильных фильтрах"
              min={0}
              value={draftMaxPrice}
              onChange={(value) => setDraftMaxPrice(value ?? null)}
            />
          </label>
          <label>
            <span>Минимум гостей</span>
            <InputNumber
              aria-label="Минимальная вместимость в мобильных фильтрах"
              min={1}
              value={draftMinCapacity}
              onChange={(value) => setDraftMinCapacity(value ?? null)}
            />
          </label>
        </div>
        <div className="objects-filter-drawer__actions">
          <Button onClick={resetMobileFilters}>Сбросить</Button>
          <Button type="primary" onClick={applyMobileFilters}>Применить</Button>
        </div>
      </Drawer>

      <Modal
        open={editingObject !== undefined}
        onCancel={closeObjectModal}
        title={editingObject === null ? "Создать объект" : `Редактировать: ${editingObject?.name.value}`}
        width={560}
        footer={null}
        destroyOnHidden
      >
        <Form form={objectForm} layout="vertical" onFinish={onObjectFormSubmit} className="booking-form">
          <Form.Item label="Название" name="name" rules={[{ required: true, message: "Укажите название" }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Описание" name="description" rules={[{ required: true, message: "Укажите описание" }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Цена" name="price" rules={[{ required: true, message: "Укажите цену" }]}>
            <InputNumber min={0} className="booking-form__number" />
          </Form.Item>
          <Form.Item label="Вместимость" name="capacity" rules={[{ required: true, message: "Укажите вместимость" }]}>
            <InputNumber min={1} className="booking-form__number" />
          </Form.Item>
          <Form.Item label="URL изображения" name="imageUrl"><Input /></Form.Item>
          <Form.Item label="Тип объекта" name="objectTypeId">
            <Select allowClear options={typeOptions} />
          </Form.Item>
          <Form.Item label="Активен" name="isActive" valuePropName="checked"><Switch /></Form.Item>
          <div className="booking-form__actions">
            <Button onClick={closeObjectModal}>Отмена</Button>
            <Button type="primary" htmlType="submit" loading={objectFormLoading}>
              {editingObject === null ? "Создать" : "Сохранить"}
            </Button>
          </div>
        </Form>
      </Modal>

      {bookingTarget && user && (
        <BookingFormModal
          booking={bookingTarget}
          objectTypes={objectTypes}
          user={user}
          onClose={() => setBookingTarget(null)}
          onSuccess={(bookingId) => messageApi.success(`Демо-заявка ${bookingId} создана`)}
        />
      )}
    </div>
  );
};
