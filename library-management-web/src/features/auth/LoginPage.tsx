import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useForm } from '@mantine/form';
import { Button, Container, Paper, PasswordInput, TextInput, Title, Alert } from '@mantine/core';
import { useAuth } from './useAuth';
import type { LoginRequest } from './types';

export function LoginPage() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (user) {
      navigate('/', { replace: true });
    }
  }, [user, navigate]);

  const form = useForm<LoginRequest>({
    initialValues: { email: '', password: '' },
    validate: {
      email: (value) => (/^\S+@\S+\.\S+$/.test(value) ? null : 'Enter a valid email address.'),
      password: (value) => (value.length > 0 ? null : 'Password is required.'),
    },
  });

  async function handleSubmit(values: LoginRequest) {
    setError(null);
    setIsSubmitting(true);
    try {
      await login(values);
      const redirectTo = (location.state as { from?: string } | null)?.from ?? '/';
      navigate(redirectTo, { replace: true });
    } catch (err) {
      const message =
        isAxiosError(err) && err.response?.status === 401
          ? 'Invalid email or password.'
          : 'Something went wrong. Please try again.';
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Container size={420} my={80}>
      <Title ta="center">Library Management System</Title>

      <Paper withBorder shadow="md" p={30} mt={30} radius="md">
        <form onSubmit={form.onSubmit(handleSubmit)}>
          {error && (
            <Alert color="red" mb="md" title="Sign in failed">
              {error}
            </Alert>
          )}

          <TextInput
            type="email"
            label="Email"
            placeholder="you@example.com"
            required
            autoComplete="email"
            {...form.getInputProps('email')}
          />
          <PasswordInput
            label="Password"
            placeholder="Your password"
            required
            autoComplete="current-password"
            mt="md"
            {...form.getInputProps('password')}
          />

          <Button type="submit" fullWidth mt="xl" loading={isSubmitting}>
            Sign in
          </Button>
        </form>
      </Paper>
    </Container>
  );
}
