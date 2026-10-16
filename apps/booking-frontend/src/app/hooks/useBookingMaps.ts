import { useEffect, useState } from "react";
import { BookingStatus, getBookingStatuses } from "../services/bookings";
import { BookingObject, getAllObjects } from "../services/booking.objects";

export interface BookingMaps {
  objectsMap: Record<string, BookingObject>;
  statusesMap: Record<string, BookingStatus>;
  statuses: BookingStatus[];
  loading: boolean;
  error: string | null;
  retry: () => void;
}

export function useBookingMaps(): BookingMaps {
  const [objectsMap, setObjectsMap] = useState<Record<string, BookingObject>>({});
  const [statusesMap, setStatusesMap] = useState<Record<string, BookingStatus>>({});
  const [statuses, setStatuses] = useState<BookingStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [retryKey, setRetryKey] = useState(0);

  useEffect(() => {
    let cancelled = false;
    Promise.all([getAllObjects(), getBookingStatuses()])
      .then(([objects, statusList]) => {
        if (!cancelled) {
          setObjectsMap(Object.fromEntries(objects.map((object) => [object.id, object])));
          setStatusesMap(Object.fromEntries(statusList.map((status) => [status.id, status])));
          setStatuses(statusList);
        }
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setError(reason instanceof Error ? reason.message : "Не удалось загрузить справочники");
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [retryKey]);

  const retry = () => {
    setLoading(true);
    setError(null);
    setRetryKey((key) => key + 1);
  };

  return { objectsMap, statusesMap, statuses, loading, error, retry };
}
