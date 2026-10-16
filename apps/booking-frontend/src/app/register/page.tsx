"use client";

import { useState } from "react";
import { Button, Card, Form, Input, Typography, message } from "antd";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { register } from "../services/auth";

const { Title, Text } = Typography;

export default function RegisterPage() {
  const [loading, setLoading] = useState(false);
  const [messageApi, contextHolder] = message.useMessage();
  const router = useRouter();

  const onFinish = async (values: {
    email: string;
    password: string;
    lastName: string;
    firstName: string;
    patronymic: string;
    phone: string;
  }) => {
    setLoading(true);
    try {
      await register(values);
      messageApi.success("Регистрация успешна!");
      setTimeout(() => router.push("/login"), 1000);
    } catch (e) {
      messageApi.error(e instanceof Error ? e.message : "Ошибка регистрации");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      {contextHolder}
      <Card className="auth-card auth-card--wide">
        <Title level={3} className="auth-heading">
          Регистрация
        </Title>
        <Form layout="vertical" onFinish={onFinish}>
          <Form.Item
            label="Email"
            name="email"
            rules={[
              { required: true, message: "Введите email" },
              { type: "email", message: "Некорректный email" },
            ]}
          >
            <Input placeholder="you@example.com" />
          </Form.Item>
          <Form.Item
            label="Пароль"
            name="password"
            rules={[
              { required: true, message: "Введите пароль" },
              { min: 8, message: "Минимум 8 символов" },
            ]}
          >
            <Input.Password placeholder="Минимум 8 символов" />
          </Form.Item>
          <Form.Item
            label="Фамилия"
            name="lastName"
            rules={[{ required: true, message: "Введите фамилию" }]}
          >
            <Input placeholder="Иванов" />
          </Form.Item>
          <Form.Item
            label="Имя"
            name="firstName"
            rules={[{ required: true, message: "Введите имя" }]}
          >
            <Input placeholder="Иван" />
          </Form.Item>
          <Form.Item
            label="Отчество"
            name="patronymic"
            rules={[{ required: true, message: "Введите отчество" }]}
          >
            <Input placeholder="Иванович" />
          </Form.Item>
          <Form.Item
            label="Телефон"
            name="phone"
            rules={[{ required: true, message: "Введите телефон" }]}
          >
            <Input placeholder="+79001234567" />
          </Form.Item>
          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              block
            >
              Зарегистрироваться
            </Button>
          </Form.Item>
        </Form>
        <Text className="auth-footer">
          Уже есть аккаунт? <Link href="/login">Войти</Link>
        </Text>
      </Card>
    </div>
  );
}
