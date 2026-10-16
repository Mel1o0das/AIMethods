export type DataMode = "api" | "mock";

export const getDataMode = (): DataMode => {
  const configuredMode = process.env.NEXT_PUBLIC_DATA_MODE;

  if (configuredMode === undefined || configuredMode === "api") return "api";
  if (configuredMode === "mock") return "mock";

  throw new Error(
    `Unsupported NEXT_PUBLIC_DATA_MODE: ${configuredMode}. Use "api" or "mock".`,
  );
};

export const isMockMode = (): boolean => getDataMode() === "mock";

export const requireApiMode = (operation: string): void => {
  if (isMockMode()) {
    throw new Error(
      `${operation} недоступна в демонстрационном режиме; HTTP-запрос заблокирован`,
    );
  }
};
