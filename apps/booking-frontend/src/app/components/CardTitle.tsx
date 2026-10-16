interface Props {
  name: string;
  price: number;
}

export const CardTitle = ({ name, price }: Props) => {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        justifyContent: "space-between",
      }}
    >
      <p className="card__title">{name}</p>
      <p className="card__price">{price}</p>
    </div>
  );
};
