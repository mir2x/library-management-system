import { useState } from 'react';
import { Badge, Button, Container, Group, Pagination, Switch, Table, Text, Title } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useLoans } from './useLoans';
import { useReturnBook } from './useLoanMutations';
import { BorrowBookModal } from './BorrowBookModal';
import type { Loan } from './types';

const PAGE_SIZE = 20;

export function LoansPage() {
  const [onlyOverdue, setOnlyOverdue] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [isBorrowModalOpen, setIsBorrowModalOpen] = useState(false);

  const { data, isLoading, isError } = useLoans({ onlyOverdue, pageNumber, pageSize: PAGE_SIZE });
  const returnBook = useReturnBook();

  async function handleReturn(loan: Loan) {
    try {
      await returnBook.mutateAsync(loan.id);
      notifications.show({ color: 'green', message: 'Book returned.' });
    } catch (error) {
      notifications.show({ color: 'red', title: 'Return failed', message: extractErrorMessage(error) });
    }
  }

  return (
    <Container py="xl" size="lg">
      <Group justify="space-between" mb="lg">
        <Title order={2}>Borrow &amp; Return</Title>
        <Button onClick={() => setIsBorrowModalOpen(true)}>Borrow Book</Button>
      </Group>

      <Switch
        label="Only show overdue loans"
        checked={onlyOverdue}
        onChange={(event) => {
          setOnlyOverdue(event.currentTarget.checked);
          setPageNumber(1);
        }}
        mb="md"
      />

      <Table.ScrollContainer minWidth={800}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Member</Table.Th>
              <Table.Th>Book</Table.Th>
              <Table.Th>Branch</Table.Th>
              <Table.Th>Due Date</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="red">Failed to load loans.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="dimmed">No loans found.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((loan) => (
              <Table.Tr key={loan.id}>
                <Table.Td>{loan.memberName}</Table.Td>
                <Table.Td>{loan.bookTitle}</Table.Td>
                <Table.Td>{loan.branchName}</Table.Td>
                <Table.Td>{new Date(loan.dueDateUtc).toLocaleDateString()}</Table.Td>
                <Table.Td>
                  {loan.isOverdue ? (
                    <Badge color="red" variant="light">
                      Overdue
                    </Badge>
                  ) : (
                    <Badge color={loan.status === 'Active' ? 'green' : 'gray'} variant="light">
                      {loan.status}
                    </Badge>
                  )}
                </Table.Td>
                <Table.Td>
                  {loan.status === 'Active' && (
                    <Button size="xs" variant="light" onClick={() => void handleReturn(loan)}>
                      Return
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

      <BorrowBookModal opened={isBorrowModalOpen} onClose={() => setIsBorrowModalOpen(false)} />
    </Container>
  );
}
