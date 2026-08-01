import { useState } from 'react';
import { Badge, Button, Container, Group, Pagination, Table, Text, Title } from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useMyReservations } from './useReservations';
import { useCancelReservation } from './useReservationMutations';
import { CreateMyReservationModal } from './CreateMyReservationModal';
import type { Reservation, ReservationStatus } from './types';

const PAGE_SIZE = 20;

const STATUS_COLOR: Record<ReservationStatus, string> = {
  Pending: 'yellow',
  Ready: 'green',
  Fulfilled: 'blue',
  Cancelled: 'gray',
};

export function MyReservationsPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const { data, isLoading, isError } = useMyReservations({ pageNumber, pageSize: PAGE_SIZE });
  const cancelReservation = useCancelReservation();

  function confirmCancel(reservation: Reservation) {
    modals.openConfirmModal({
      title: 'Cancel reservation',
      children: <Text size="sm">Cancel your reservation for "{reservation.bookTitle}"?</Text>,
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
        <Title order={2}>My Reservations</Title>
        <Button onClick={() => setIsCreateModalOpen(true)}>Reserve a Book</Button>
      </Group>

      <Table.ScrollContainer minWidth={600}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Book</Table.Th>
              <Table.Th>Branch</Table.Th>
              <Table.Th>Queue Position</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="red">Failed to load your reservations.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="dimmed">You have no reservations yet.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((reservation) => (
              <Table.Tr key={reservation.id}>
                <Table.Td>{reservation.bookTitle}</Table.Td>
                <Table.Td>{reservation.branchName}</Table.Td>
                <Table.Td>{reservation.status === 'Pending' ? reservation.queuePosition : '—'}</Table.Td>
                <Table.Td>
                  <Badge color={STATUS_COLOR[reservation.status]} variant="light">
                    {reservation.status}
                  </Badge>
                </Table.Td>
                <Table.Td>
                  {(reservation.status === 'Pending' || reservation.status === 'Ready') && (
                    <Button size="xs" variant="subtle" color="red" onClick={() => confirmCancel(reservation)}>
                      Cancel
                    </Button>
                  )}
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

      <CreateMyReservationModal opened={isCreateModalOpen} onClose={() => setIsCreateModalOpen(false)} />
    </Container>
  );
}
