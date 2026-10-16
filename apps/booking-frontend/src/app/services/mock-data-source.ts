import type {
  BookingDataSource,
  BookingObject,
  BookingStatus,
  CreateBookingPayload,
  GetObjectsParams,
  MyBooking,
  ObjectType,
  OccupiedSlot,
  PagedResult,
} from "./data-contracts";

const TYPE_SAUNA = "type-sauna";
const TYPE_GLAMPING = "type-glamping";
const STATUS_PENDING = "status-pending";
const STATUS_CONFIRMED = "status-confirmed";
const STATUS_CANCELLED = "status-cancelled";

const objectTypes: ObjectType[] = [
  { id: TYPE_SAUNA, name: { value: "Баня" } },
  { id: TYPE_GLAMPING, name: { value: "Глэмпинг" } },
];

const bookingStatuses: BookingStatus[] = [
  { id: STATUS_PENDING, name: { value: "Ожидает подтверждения" } },
  { id: STATUS_CONFIRMED, name: { value: "Подтверждено" } },
  { id: STATUS_CANCELLED, name: { value: "Отменено" } },
];

const bookingObjects: BookingObject[] = [
  {
    id: "object-forest-sauna",
    name: { value: "Лесная баня" },
    description: { value: "Демонстрационная баня у соснового леса." },
    price: 4200,
    capacity: 6,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-10T09:00:00.000Z",
    objectTypeId: TYPE_SAUNA,
  },
  {
    id: "object-lake-glamping",
    name: { value: "Дом у озера" },
    description: { value: "Демонстрационный глэмпинг с видом на воду." },
    price: 9800,
    capacity: 4,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-12T09:00:00.000Z",
    objectTypeId: TYPE_GLAMPING,
  },
  {
    id: "object-pine-sauna",
    name: { value: "Сосновый пар" },
    description: { value: "Демонстрационная камерная баня." },
    price: 3600,
    capacity: 4,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-14T09:00:00.000Z",
    objectTypeId: TYPE_SAUNA,
  },
  {
    id: "object-river-glamping",
    name: { value: "Сфера у реки" },
    description: { value: "Демонстрационный купольный домик рядом с рекой." },
    price: 7600,
    capacity: 2,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-16T09:00:00.000Z",
    objectTypeId: TYPE_GLAMPING,
  },
  {
    id: "object-hill-glamping",
    name: { value: "Дом на холме" },
    description: { value: "Демонстрационный домик с видом на лесную долину." },
    price: 11200,
    capacity: 5,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-18T09:00:00.000Z",
    objectTypeId: TYPE_GLAMPING,
  },
  {
    id: "object-meadow-glamping",
    name: { value: "Тихая поляна" },
    description: { value: "Демонстрационный глэмпинг на краю соснового леса." },
    price: 6800,
    capacity: 3,
    imageUrl: null,
    isActive: true,
    createdAt: "2026-01-20T09:00:00.000Z",
    objectTypeId: TYPE_GLAMPING,
  },
];

const initialBookings: MyBooking[] = [
  {
    id: "booking-demo-1",
    userId: "mock-user",
    bookingObjectId: "object-lake-glamping",
    startDateTime: "2026-11-14T12:00:00.000Z",
    endDateTime: "2026-11-16T09:00:00.000Z",
    guestsCount: 2,
    comment: { value: "Демонстрационная заявка" },
    statusId: STATUS_CONFIRMED,
    createdAt: "2026-10-01T10:00:00.000Z",
  },
];

let bookings = initialBookings.map((booking) => ({ ...booking }));
let nextBookingNumber = 2;

const copy = <T>(value: T): T => structuredClone(value);

const compareObjects = (
  left: BookingObject,
  right: BookingObject,
  sortBy: string,
): number => {
  if (sortBy === "name") return left.name.value.localeCompare(right.name.value, "ru");
  if (sortBy === "price") return left.price - right.price;
  if (sortBy === "capacity") return left.capacity - right.capacity;
  return left.createdAt.localeCompare(right.createdAt);
};

const overlaps = (
  start: string,
  end: string,
  booking: MyBooking,
): boolean =>
  new Date(start).getTime() < new Date(booking.endDateTime).getTime() &&
  new Date(end).getTime() > new Date(booking.startDateTime).getTime();

export const resetMockData = (): void => {
  bookings = initialBookings.map((booking) => ({ ...booking }));
  nextBookingNumber = 2;
};

export const mockDataSource: BookingDataSource = {
  async getObjectsPaged(
    params: GetObjectsParams = {},
  ): Promise<PagedResult<BookingObject>> {
    const page = Math.max(1, params.page ?? 1);
    const pageSize = Math.max(1, params.pageSize ?? 10);
    const query = params.search?.trim().toLocaleLowerCase("ru") ?? "";
    const direction = params.sortDir === "desc" ? -1 : 1;

    const filtered = bookingObjects
      .filter((item) => item.isActive)
      .filter(
        (item) =>
          !query ||
          item.name.value.toLocaleLowerCase("ru").includes(query) ||
          item.description.value.toLocaleLowerCase("ru").includes(query),
      )
      .filter((item) => !params.typeId || item.objectTypeId === params.typeId)
      .filter((item) => params.maxPrice == null || item.price <= params.maxPrice)
      .filter(
        (item) =>
          params.minCapacity == null || item.capacity >= params.minCapacity,
      )
      .sort(
        (left, right) =>
          compareObjects(left, right, params.sortBy ?? "createdAt") * direction,
      );

    const totalCount = filtered.length;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const start = (page - 1) * pageSize;

    return copy({
      items: filtered.slice(start, start + pageSize),
      totalCount,
      page,
      pageSize,
      totalPages,
    });
  },

  async getObjectById(id: string): Promise<BookingObject> {
    const item = bookingObjects.find((object) => object.id === id);
    if (!item) throw new Error("Демонстрационный объект не найден");
    return copy(item);
  },

  async getObjectTypes(): Promise<ObjectType[]> {
    return copy(objectTypes);
  },

  async getBookingStatuses(): Promise<BookingStatus[]> {
    return copy(bookingStatuses);
  },

  async getMyBookings(userId: string): Promise<MyBooking[]> {
    return copy(bookings.filter((booking) => booking.userId === userId));
  },

  async getAllBookings(): Promise<MyBooking[]> {
    return copy(bookings);
  },

  async confirmBooking(bookingId: string): Promise<void> {
    const booking = bookings.find((item) => item.id === bookingId);
    if (!booking) throw new Error("Демонстрационная заявка не найдена");
    booking.statusId = STATUS_CONFIRMED;
  },

  async cancelBooking(bookingId: string, userId?: string): Promise<void> {
    const booking = bookings.find((item) => item.id === bookingId);
    if (!booking) throw new Error("Демонстрационная заявка не найдена");
    if (userId && booking.userId !== userId) {
      throw new Error("Нельзя отменить чужую демонстрационную заявку");
    }
    if (booking.statusId === STATUS_CANCELLED) {
      throw new Error("Демонстрационная заявка уже отменена");
    }
    if (new Date(booking.endDateTime).getTime() <= Date.now()) {
      throw new Error("Завершённую демонстрационную заявку отменить нельзя");
    }
    booking.statusId = STATUS_CANCELLED;
  },

  async getOccupiedSlotsForRange(
    objectId: string,
    from: string,
    to: string,
  ): Promise<OccupiedSlot[]> {
    const fromTime = new Date(from).getTime();
    const toTime = new Date(to).getTime();
    return copy(
      bookings
        .filter(
          (booking) =>
            booking.bookingObjectId === objectId &&
            booking.statusId !== STATUS_CANCELLED &&
            new Date(booking.startDateTime).getTime() < toTime &&
            new Date(booking.endDateTime).getTime() > fromTime,
        )
        .map(({ startDateTime, endDateTime }) => ({
          startDateTime,
          endDateTime,
        })),
    );
  },

  async createBooking(data: CreateBookingPayload): Promise<string> {
    const object = bookingObjects.find((item) => item.id === data.bookingObjectId);
    if (!object) throw new Error("Демонстрационный объект не найден");
    if (!object.isActive) throw new Error("Демонстрационный объект недоступен");

    const startTime = new Date(data.startDateTime).getTime();
    const endTime = new Date(data.endDateTime).getTime();
    if (!Number.isFinite(startTime) || !Number.isFinite(endTime)) {
      throw new Error("Укажите корректный демонстрационный интервал");
    }
    if (startTime >= endTime) {
      throw new Error("Начало должно быть раньше окончания");
    }
    if (startTime <= Date.now()) {
      throw new Error("Начало должно быть в будущем");
    }
    if (!Number.isInteger(data.guestsCount) || data.guestsCount < 1 || data.guestsCount > object.capacity) {
      throw new Error(`Количество гостей должно быть от 1 до ${object.capacity}`);
    }
    if ((data.comment?.length ?? 0) > 500) {
      throw new Error("Комментарий не должен превышать 500 символов");
    }
    if (!bookingStatuses.some((status) => status.id === data.statusId)) {
      throw new Error("Неизвестный демонстрационный статус");
    }
    if (
      bookings.some(
        (booking) =>
          booking.bookingObjectId === data.bookingObjectId &&
          booking.statusId !== STATUS_CANCELLED &&
          overlaps(data.startDateTime, data.endDateTime, booking),
      )
    ) {
      throw new Error("Выбранное демонстрационное время уже занято");
    }

    const id = `booking-demo-${nextBookingNumber++}`;
    bookings.push({
      id,
      userId: data.userId,
      bookingObjectId: data.bookingObjectId,
      startDateTime: data.startDateTime,
      endDateTime: data.endDateTime,
      guestsCount: data.guestsCount,
      comment: data.comment ? { value: data.comment } : null,
      statusId: data.statusId || STATUS_PENDING,
      createdAt: "2026-10-16T09:00:00.000Z",
    });
    return id;
  },
};
