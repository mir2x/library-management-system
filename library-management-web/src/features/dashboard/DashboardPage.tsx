import type { UseQueryResult } from '@tanstack/react-query';
import { Container, Paper, Skeleton, SimpleGrid, Text, Title } from '@mantine/core';
import { useAuth } from '../auth/useAuth';
import { Roles } from '../../lib/roles';
import { useMemberDashboardStats, useStaffDashboardStats } from './useDashboardStats';

function StatCard({ label, query }: { label: string; query: UseQueryResult<number> }) {
  return (
    <Paper withBorder p="md" radius="md">
      <Text size="sm" c="dimmed">
        {label}
      </Text>
      {query.isLoading ? (
        <Skeleton height={32} mt={4} width={60} />
      ) : query.isError ? (
        <Text size="xl" fw={700} c="red">
          —
        </Text>
      ) : (
        <Text size="xl" fw={700}>
          {query.data}
        </Text>
      )}
    </Paper>
  );
}

function StaffStats() {
  const { branchCount, bookCount, overdueLoanCount, pendingReservationCount } = useStaffDashboardStats();

  return (
    <SimpleGrid cols={{ base: 1, sm: 2, md: 4 }}>
      <StatCard label="Branches" query={branchCount} />
      <StatCard label="Books" query={bookCount} />
      <StatCard label="Overdue Loans" query={overdueLoanCount} />
      <StatCard label="Pending Reservations" query={pendingReservationCount} />
    </SimpleGrid>
  );
}

function MemberStats() {
  const { activeLoanCount, activeReservationCount } = useMemberDashboardStats();

  return (
    <SimpleGrid cols={{ base: 1, sm: 2 }}>
      <StatCard label="My Active Loans" query={activeLoanCount} />
      <StatCard label="My Active Reservations" query={activeReservationCount} />
    </SimpleGrid>
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const isStaff = user?.roles.includes(Roles.Admin) || user?.roles.includes(Roles.Librarian);

  return (
    <Container py="xl" size="lg">
      <Title order={2} mb="lg">
        Dashboard
      </Title>
      {isStaff ? <StaffStats /> : <MemberStats />}
    </Container>
  );
}
