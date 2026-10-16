"use client";

import { useState } from "react";
import { Button, Card, Form, Input, Typography, message } from "antd";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "../context/AuthContext";
import { login } from "../services/auth";
import { isMockMode } from "../services/data-mode";

const { Title, Text } = Typography;

export default function LoginPage() {
  const demoMode = isMockMode();
  const router = useRouter();
  const { authenticate } = useAuth();
  const [loading, setLoading] = useState(false);
  const [messageApi, contextHolder] = message.useMessage();

  const onFinish = async (values: { email: string; password: string }) => {
    setLoading(true);
    try {
      const result = await login(values);
      await authenticate(result.token);
      router.push("/");
    } catch (e) {
      messageApi.error(e instanceof Error ? e.message : "Ошибка входа");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      {contextHolder}
      <Card className="auth-card">
        {demoMode ? (
          <div className="auth-demo">
            <p className="section-kicker">Демонстрационный режим</p>
            <Title level={3}>Вы уже вошли как Демо-пользователь</Title>
            <Text>
              Настоящий пароль и JWT не используются. Все заявки существуют
              только в памяти текущей демонстрационной сессии.
            </Text>
            <Link href="/objects">
              <Button type="primary" block>Перейти в каталог</Button>
            </Link>
          </div>
        ) : (
          <>
        <Title level={3} className="auth-heading">
          Вход в систему
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
            rules={[{ required: true, message: "Введите пароль" }]}
          >
            <Input.Password placeholder="Минимум 8 символов" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading} block>
              Войти
            </Button>
          </Form.Item>
        </Form>
        <Text className="auth-footer">
          Нет аккаунта? <Link href="/register">Зарегистрироваться</Link>
        </Text>
          </>
        )}
      </Card>
    </div>
  );
}
