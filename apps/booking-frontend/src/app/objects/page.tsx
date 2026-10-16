"use client";

import { Alert, Button, Flex, Spin } from "antd";
import { useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { BookingObjects } from "../components/BookingObjects";
import {
  BookingObject,
  PagedResult,
  getObjectsPaged,
} from "../services/booking.objects";
import { isMockMode } from "../services/data-mode";
import { ObjectType, getObjectTypes } from "../services/object-types";

const DEFAULT_PAGE_SIZE = 10;

const emptyResult: PagedResult<BookingObject> = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: DEFAULT_PAGE_SIZE,
  totalPages: 1,
};

const errorMessage = (error: unknown, fallback: string) =>
  error instanceof Error && error.message ? error.message : fallback;

export function BookingObjectsPageContent() {
  const searchParams = useSearchParams();
  const initialSearch = searchParams.get("search")?.trim() ?? "";
  const [result, setResult] = useState<PagedResult<BookingObject> | null>(null);
  const [loading, setLoading] = useState(true);
  const [initialLoad, setInitialLoad] = useState(true);
  const [catalogError, setCatalogError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [search, setSearch] = useState(initialSearch);
  const [sortBy, setSortBy] = useState("createdAt");
  const [sortDir, setSortDir] = useState("asc");
  const [typeId, setTypeId] = useState<string | null>(null);
  const [maxPrice, setMaxPrice] = useState<number | null>(null);
  const [minCapacity, setMinCapacity] = useState<number | null>(null);
  const [objectTypes, setObjectTypes] = useState<ObjectType[]>([]);
  const [typesLoading, setTypesLoading] = useState(true);
  const [typesError, setTypesError] = useState<string | null>(null);
  const [catalogRetryKey, setCatalogRetryKey] = useState(0);
  const [typesRetryKey, setTypesRetryKey] = useState(0);

  useEffect(() => {
    let cancelled = false;

    getObjectTypes()
      .then((types) => {
        if (!cancelled) setObjectTypes(types);
      })
      .catch((error: unknown) => {
        if (!cancelled) {
          setObjectTypes([]);
          setTypesError(errorMessage(error, "Не удалось загрузить типы объектов"));
        }
      })
      .finally(() => {
        if (!cancelled) setTypesLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [typesRetryKey]);

  useEffect(() => {
    let cancelled = false;

    getObjectsPaged({
      page,
      pageSize,
      search: search || undefined,
      sortBy,
      sortDir,
      typeId,
      maxPrice,
      minCapacity,
    })
      .then((data) => {
        if (!cancelled) setResult(data);
      })
      .catch((error: unknown) => {
        if (!cancelled) {
          setCatalogError(errorMessage(error, "Не удалось загрузить каталог"));
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
          setInitialLoad(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [page, pageSize, search, sortBy, sortDir, typeId, maxPrice, minCapacity, catalogRetryKey]);

  const beginCatalogLoad = () => {
    setLoading(true);
    setCatalogError(null);
    setResult(null);
  };

  const resetToFirstPage = () => setPage(1);

  const handleSearchChange = (value: string) => {
    beginCatalogLoad();
    setSearch(value);
    resetToFirstPage();
  };

  const handleSortChange = (newSortBy: string, newSortDir: string) => {
    beginCatalogLoad();
    setSortBy(newSortBy);
    setSortDir(newSortDir);
    resetToFirstPage();
  };

  const handlePageSizeChange = (newSize: number) => {
    beginCatalogLoad();
    setPageSize(newSize);
    resetToFirstPage();
  };

  const handleTypeChange = (value: string | null) => {
    beginCatalogLoad();
    setTypeId(value);
    resetToFirstPage();
  };

  const handleMaxPriceChange = (value: number | null) => {
    beginCatalogLoad();
    setMaxPrice(value);
    resetToFirstPage();
  };

  const handleMinCapacityChange = (value: number | null) => {
    beginCatalogLoad();
    setMinCapacity(value);
    resetToFirstPage();
  };

  const retryCatalog = () => {
    beginCatalogLoad();
    setCatalogRetryKey((key) => key + 1);
  };

  const retryTypes = () => {
    setTypesLoading(true);
    setTypesError(null);
    setTypesRetryKey((key) => key + 1);
  };

  const handlePageChange = (newPage: number) => {
    beginCatalogLoad();
    setPage(newPage);
  };

  if (initialLoad && loading && result === null) {
    return (
      <Flex vertical align="center" justify="center" className="objects-loading">
        <Spin size="large" />
        <div className="objects-loading__text">Подбираем места…</div>
      </Flex>
    );
  }

  if (catalogError) {
    return (
      <section className="objects-state" aria-labelledby="catalog-error-title">
        <p className="section-kicker">Каталог временно недоступен</p>
        <Alert
          type="error"
          showIcon
          title={<span id="catalog-error-title">Не удалось загрузить объекты</span>}
          description={catalogError}
          action={
            <Button onClick={retryCatalog} type="primary">
              Повторить
            </Button>
          }
        />
      </section>
    );
  }

  return (
    <BookingObjects
      items={(result ?? emptyResult).items}
      totalCount={(result ?? emptyResult).totalCount}
      page={(result ?? emptyResult).page}
      pageSize={(result ?? emptyResult).pageSize}
      totalPages={(result ?? emptyResult).totalPages}
      loading={loading}
      search={search}
      sortBy={sortBy}
      sortDir={sortDir}
      typeId={typeId}
      maxPrice={maxPrice}
      minCapacity={minCapacity}
      objectTypes={objectTypes}
      typesLoading={typesLoading}
      typesError={typesError}
      isDemo={isMockMode()}
      onRetryTypes={retryTypes}
      onSearchChange={handleSearchChange}
      onSortChange={handleSortChange}
      onPageSizeChange={handlePageSizeChange}
      onPageChange={handlePageChange}
      onTypeChange={handleTypeChange}
      onMaxPriceChange={handleMaxPriceChange}
      onMinCapacityChange={handleMinCapacityChange}
      onRefresh={retryCatalog}
    />
  );
}

export default function BookingObjectsPage() {
  return (
    <Suspense
      fallback={
        <Flex vertical align="center" justify="center" className="objects-loading">
          <Spin size="large" />
          <div className="objects-loading__text">Открываем каталог…</div>
        </Flex>
      }
    >
      <BookingObjectsPageContent />
    </Suspense>
  );
}
