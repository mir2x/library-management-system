import { Container, Text, Title } from '@mantine/core';

interface ComingSoonPageProps {
  title: string;
}

// Placeholder for a module not yet built — replaced with the real feature page when that
// module's turn comes up, same as HomePlaceholder was for the Dashboard.
export function ComingSoonPage({ title }: ComingSoonPageProps) {
  return (
    <Container py="xl">
      <Title order={2}>{title}</Title>
      <Text c="dimmed" mt="xs">
        This module hasn't been built yet.
      </Text>
    </Container>
  );
}
