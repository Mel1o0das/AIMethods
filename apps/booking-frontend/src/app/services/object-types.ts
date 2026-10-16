import { getBookingDataSource } from "./data-source";

export type { ObjectType } from "./data-contracts";

export const getObjectTypes = () => getBookingDataSource().getObjectTypes();
