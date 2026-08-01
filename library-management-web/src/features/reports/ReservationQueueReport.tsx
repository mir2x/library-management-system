import { Badge, Table, Text } from '@mantine/core';
import { useReservationQueuesReport } from './useReports';

export function ReservationQueueReport({ branchId }: { branchId?: string }) {
  const { data, isLoading, isError } = useReservationQueuesReport(branchId);

  return (
    <Table.ScrollContainer minWidth={700}>
      <Table verticalSpacing="sm" highlightOnHover>
        <Table.Thead>
          <Table.Tr>
            <Table.Th>Book</Table.Th>
            <Table.Th>Branch</Table.Th>
            <Table.Th>Pending</Table.Th>
            <Table.Th>Ready Copy Waiting</Table.Th>
            <Table.Th>Oldest Pending Since</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {isError && (
            <Table.Tr>
              <Table.Td colSpan={5}>
                <Text c="red">Failed to load this report.</Text>
              </Table.Td>
            </Table.Tr>
          )}
          {!isError && !isLoading && data?.length === 0 && (
            <Table.Tr>
              <Table.Td colSpan={5}>
                <Text c="dimmed">No active reservation queues.</Text>
              </Table.Td>
            </Table.Tr>
          )}
          {data?.map((queue) => (
            <Table.Tr key={`${queue.bookId}-${queue.branchId}`}>
              <Table.Td>{queue.bookTitle}</Table.Td>
              <Table.Td>{queue.branchName}</Table.Td>
              <Table.Td>{queue.pendingCount}</Table.Td>
              <Table.Td>
                {queue.hasReadyCopy && (
                  <Badge color="green" variant="light">
                    Yes
                  </Badge>
                )}
              </Table.Td>
              <Table.Td>
                {queue.oldestPendingSinceUtc ? new Date(queue.oldestPendingSinceUtc).toLocaleDateString() : '—'}
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  );
}
