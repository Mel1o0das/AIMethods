export interface BookingObject {
  id: string;
  name: { value: string };
  description: { value: string };
  price: number;
  capacity: number;
  imageUrl: string | null;
  isActive: boolean;
  createdAt: string;
  objectTypeId: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface GetObjectsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDir?: string;
  typeId?: string | null;
  maxPrice?: number | null;
  minCapacity?: number | null;
}

export interface ObjectType {
  id: string;
  name: { value: string };
}

export interface BookingStatus {
  id: string;
  name: { value: string };
}

export interface MyBooking {
  id: string;
  userId: string;
  bookingObjectId: string;
  startDateTime: string;
  endDateTime: string;
  guestsCount: number;
  comment: { value: string } | null;
  statusId: string;
  createdAt: string;
}

export interface CreateBookingPayload {
  userId: string;
  bookingObjectId: string;
  startDateTime: string;
  endDateTime: string;
  guestsCount: number;
  comment?: string;
  statusId: string;
}

export interface OccupiedSlot {
  startDateTime: string;
  endDateTime: string;
}

export interface BookingDataSource {
  getObjectsPaged(params?: GetObjectsParams): Promise<PagedResult<BookingObject>>;
  getObjectById(id: string): Promise<BookingObject>;
  getObjectTypes(): Promise<ObjectType[]>;
  getBookingStatuses(): Promise<BookingStatus[]>;
  getMyBookings(userId: string): Promise<MyBooking[]>;
  getAllBookings(): Promise<MyBooking[]>;
  confirmBooking(bookingId: string): Promise<void>;
  cancelBooking(bookingId: string, userId?: string): Promise<void>;
  getOccupiedSlotsForRange(
    objectId: string,
    from: string,
    to: string,
  ): Promise<OccupiedSlot[]>;
  createBooking(data: CreateBookingPayload): Promise<string>;
}
