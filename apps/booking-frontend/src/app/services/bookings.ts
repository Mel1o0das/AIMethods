import { getBookingDataSource } from "./data-source";
import type { CreateBookingPayload } from "./data-contracts";

export type {
  BookingStatus,
  CreateBookingPayload,
  MyBooking,
  OccupiedSlot,
} from "./data-contracts";

export const getStatusColor = (name: string): string => {
  const lower = name.toLowerCase();
  if (lower.includes("ожид")) return "orange";
  if (lower.includes("подтверждено")) return "green";
  if (lower.includes("отмен")) return "red";
  if (lower.includes("своб")) return "cyan";
  return "blue";
};

export const isCancelledStatus = (name: string): boolean =>
  name.toLowerCase().includes("отмен");

export const isConfirmedStatus = (name: string): boolean =>
  name.toLowerCase().includes("подтверждено");

export const isPendingStatus = (name: string): boolean =>
  name.toLowerCase().includes("ожид");

export const getBookingStatuses = () =>
  getBookingDataSource().getBookingStatuses();

export const getMyBookings = (userId: string) =>
  getBookingDataSource().getMyBookings(userId);

export const getAllBookings = () => getBookingDataSource().getAllBookings();

export const confirmBooking = (bookingId: string) =>
  getBookingDataSource().confirmBooking(bookingId);

export const cancelBooking = (bookingId: string, userId?: string) =>
  getBookingDataSource().cancelBooking(bookingId, userId);

export const getOccupiedSlotsForRange = (
  objectId: string,
  from: string,
  to: string,
) => getBookingDataSource().getOccupiedSlotsForRange(objectId, from, to);

export const createBooking = (data: CreateBookingPayload) =>
  getBookingDataSource().createBooking(data);
