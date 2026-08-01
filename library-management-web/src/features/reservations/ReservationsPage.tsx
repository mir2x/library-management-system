import { useState } from 'react';
import { Badge, Button, Container, Group, Pagination, Select, Table, Text, Title } from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useReservations } from './useReservations';
import { useCancelReservation, useFulfillReservation } from './useReservationMutations';
import { CreateReservationModal } from './CreateReservationModal';
import type { Reservation, ReservationStatus } from './types';

const PAGE_SIZE = 20;

const STATUS_COLOR: Record<ReservationStatus, string> = {
  Pending: 'yellow',
  Ready: 'green',
  Fulfilled: 'blue',
  Cancelled: 'gray',
};

const STATUS_OPTIONS = [
  { value: '', label: 'All statuses' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Ready', label: 'Ready' },
  { value: 'Fulfilled', label: 'Fulfilled' },
  { value: 'Cancelled', label: 'Cancelled' },
];

export function ReservationsPage() {
  const [status, setStatus] = useState<ReservationStatus | ''>('');
  const [pageNumber, setPageNumber] = useState(1);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const { data, isLoading, isError } = useReservations({
    status: status || undefined,
    pageNumber,
    pageSize: PAGE_SIZE,
  });
  const fulfillReservation = useFulfillReservation();
  const cancelReservation = useCancelReservation();

  async function handleFulfill(reservation: Reservation) {
    try {
      await fulfillReservation.mutateAsync(reservation.id);
      notifications.show({ color: 'green', message: 'Reservation fulfilled — loan created.' });
    } catch (error) {
      notifications.show({ color: 'red', title: 'Fulfill failed', message: extractErrorMessage(error) });
    }
  }

  function confirmCancel(reservation: Reservation) {
    modals.openConfirmModal({
      title: 'Cancel reservation',
      children: (
        <Text size="sm">
          Cancel {reservation.memberName}'s reservation for "{reservation.bookTitle}"?
        </Text>
      ),
      labels: { confirm: 'Cancel Reservation', cancel: 'Back' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await cancelReservation.mutateAsync(reservation.id);
          notifications.show({ color: 'green', message: 'Reservation cancelled.' });
        } catch (error) {
          notifications.show({ color: 'red', title: 'Cancel failed', message: extractErrorMessage(error) });
        }
      },
    });
  }

  return (
    <Container py="xl" size="lg">
      <Group justify="space-between" mb="lg">
        <Title order={2}>Reservation Queue</Title>
        <Button onClick={() => setIsCreateModalOpen(true)}>Reserve Book</Button>
      </Group>

      <Select
        data={STATUS_OPTIONS}
        value={status}
        onChange={(value) => {
          setStatus((value as ReservationStatus | '') ?? '');
          setPageNumber(1);
        }}
        allowDeselect={false}
        mb="md"
        maw={220}
      />

      <Table.ScrollContainer minWidth={800}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Member</Table.Th>
              <Table.Th>Book</Table.Th>
              <Table.Th>Branch</Table.Th>
              <Table.Th>Queue #</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="red">Failed to load reservations.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="dimmed">No reservations found.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((reservation) => (
              <Table.Tr key={reservation.id}>
                <Table.Td>{reservation.memberName}</Table.Td>
                <Table.Td>{reservation.bookTitle}</Table.Td>
                <Table.Td>{reservation.branchName}</Table.Td>
                <Table.Td>{reservation.status === 'Pending' ? reservation.queuePosition : '—'}</Table.Td>
                <Table.Td>
                  <Badge color={STATUS_COLOR[reservation.status]} variant="light">
                    {reservation.status}
                  </Badge>
                </Table.Td>
                <Table.Td>
                  <Group gap="xs" justify="flex-end">
                    {reservation.status === 'Ready' && (
                      <Button size="xs" variant="light" onClick={() => void handleFulfill(reservation)}>
                        Fulfill
                      </Button>
                    )}
                    {(reservation.status === 'Pending' || reservation.status === 'Ready') && (
                      <Button size="xs" variant="subtle" color="red" onClick={() => confirmCancel(reservation)}>
                        Cancel
                      </Button>
                    )}
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>

      {data && data.totalPages > 1 && (
        <Group justify="center" mt="lg">
          <Pagination value={pageNumber} onChange={setPageNumber} total={data.totalPages} />
        </Group>
      )}

      <CreateReservationModal opened={isCreateModalOpen} onClose={() => setIsCreateModalOpen(false)} />
    </Container>
  );
}
