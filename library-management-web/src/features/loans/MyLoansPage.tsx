import { useState } from 'react';
import { Badge, Container, Pagination, Table, Text, Title, Group } from '@mantine/core';
import { useMyLoans } from './useLoans';

const PAGE_SIZE = 20;

export function MyLoansPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const { data, isLoading, isError } = useMyLoans({ pageNumber, pageSize: PAGE_SIZE });

  return (
    <Container py="xl" size="lg">
      <Title order={2} mb="lg">
        My Loans
      </Title>

      <Table.ScrollContainer minWidth={600}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Book</Table.Th>
              <Table.Th>Branch</Table.Th>
              <Table.Th>Borrowed</Table.Th>
              <Table.Th>Due Date</Table.Th>
              <Table.Th>Status</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="red">Failed to load your loans.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="dimmed">You have no loan history yet.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((loan) => (
              <Table.Tr key={loan.id}>
                <Table.Td>{loan.bookTitle}</Table.Td>
                <Table.Td>{loan.branchName}</Table.Td>
                <Table.Td>{new Date(loan.borrowedAtUtc).toLocaleDateString()}</Table.Td>
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
    </Container>
  );
}
