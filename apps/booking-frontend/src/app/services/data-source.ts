import { apiDataSource } from "./api-data-source";
import type { BookingDataSource } from "./data-contracts";
import { getDataMode } from "./data-mode";
import { mockDataSource } from "./mock-data-source";

export const getBookingDataSource = (): BookingDataSource =>
  getDataMode() === "mock" ? mockDataSource : apiDataSource;
