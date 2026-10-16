"use client";

import { Button, Input } from "antd";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";

const collection = [
  {
    key: "sauna",
    eyebrow: "Тёплый пар",
    title: "Бани среди сосен",
    description: "Камерные пространства для спокойного вечера за городом.",
    href: "/objects?search=баня",
  },
  {
    key: "glamping",
    eyebrow: "Тишина у воды",
    title: "Домики и глэмпинги",
    description: "Природа рядом, привычный комфорт — внутри.",
    href: "/objects?search=дом",
  },
  {
    key: "all",
    eyebrow: "Вся коллекция",
    title: "Найдите своё место",
    description: "Сравните вместимость, описание и условную стоимость.",
    href: "/objects",
  },
];

export default function Home() {
  const router = useRouter();
  const [search, setSearch] = useState("");

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const query = search.trim();
    router.push(query ? `/objects?search=${encodeURIComponent(query)}` : "/objects");
  };

  return (
    <div className="home-page">
      <section className="premium-hero" aria-labelledby="home-title">
        <div className="premium-hero__content">
          <p className="section-kicker">Бани и глэмпинги за городом</p>
          <h1 id="home-title" className="premium-hero__title">
            Место, где время замедляется
          </h1>
          <p className="premium-hero__lead">
            Тёплый свет, жар печи и тишина леса. Выберите пространство для
            отдыха и изучите детали без спешки.
          </p>

          <form className="hero-search" onSubmit={handleSearch} role="search">
            <label className="hero-search__label" htmlFor="home-search">
              Найти место по названию или описанию
            </label>
            <div className="hero-search__controls">
              <Input
                id="home-search"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Например, баня или дом у озера"
                allowClear
                size="large"
              />
              <Button htmlType="submit" type="primary" size="large">
                Найти
              </Button>
            </div>
          </form>

          <div className="premium-hero__meta">
            <Link href="/objects" className="premium-hero__cta">
              Исследовать все места <span aria-hidden="true">→</span>
            </Link>
            <span>Каталог бань и глэмпингов</span>
          </div>
        </div>

        <div className="premium-hero__visual" aria-label="Стилизованный домик в лесу">
          <div className="premium-hero__moon" />
          <div className="premium-hero__trees premium-hero__trees--back" />
          <div className="premium-hero__cabin">
            <span className="premium-hero__window" />
            <span className="premium-hero__door" />
          </div>
          <div className="premium-hero__trees premium-hero__trees--front" />
          <p className="premium-hero__caption">Вечер у воды</p>
        </div>
      </section>

      <section className="home-collection" aria-labelledby="collection-title">
        <div className="home-section-heading">
          <div>
            <p className="section-kicker">Коллекция мест</p>
            <h2 id="collection-title">Выберите настроение отдыха</h2>
          </div>
          <p>
            Поиск и фильтры доступны в каталоге. Даты и время выбираются на
            странице конкретного объекта.
          </p>
        </div>

        <div className="collection-grid">
          {collection.map((item, index) => (
            <Link
              href={item.href}
              className={`collection-card collection-card--${index + 1}`}
              key={item.key}
            >
              <span className="collection-card__number">0{index + 1}</span>
              <div>
                <span className="collection-card__eyebrow">{item.eyebrow}</span>
                <h3>{item.title}</h3>
                <p>{item.description}</p>
              </div>
              <span className="collection-card__link">
                Смотреть в каталоге <span aria-hidden="true">↗</span>
              </span>
            </Link>
          ))}
        </div>
      </section>

      <section className="home-note" aria-label="Как работает сервис">
        <p className="section-kicker">Простой маршрут</p>
        <div className="home-note__steps">
          <span><b>01</b> Найдите объект</span>
          <span><b>02</b> Изучите детали</span>
          <span><b>03</b> Выберите время</span>
        </div>
        <p>
          Предварительная занятость помогает выбрать интервал, но заявка
          считается созданной только после успешного ответа сервиса.
        </p>
      </section>
    </div>
  );
}
