import { Button, Container, Group, Text, Title } from '@mantine/core';
import { useAuth } from '../features/auth/useAuth';

// Temporary authenticated landing page — replaced by the real Dashboard module.
export function HomePlaceholder() {
  const { user, logout } = useAuth();

  return (
    <Container py="xl">
      <Title order={2}>Welcome, {user?.fullName}</Title>
      <Text c="dimmed">
        {user?.email} — {user?.roles.join(', ')}
      </Text>
      <Group mt="lg">
        <Button variant="light" color="red" onClick={() => void logout()}>
          Sign out
        </Button>
      </Group>
    </Container>
  );
}
