import { useState } from 'react';
import { Badge, Group, Pagination, Table, Text } from '@mantine/core';
import { useOverdueLoansReport } from './useReports';

const PAGE_SIZE = 20;

export function OverdueLoansReport({ branchId }: { branchId?: string }) {
  const [pageNumber, setPageNumber] = useState(1);
  const { data, isLoading, isError } = useOverdueLoansReport({ branchId, pageNumber, pageSize: PAGE_SIZE });

  return (
    <>
      <Table.ScrollContainer minWidth={700}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Member</Table.Th>
              <Table.Th>Book</Table.Th>
              <Table.Th>Branch</Table.Th>
              <Table.Th>Due Date</Table.Th>
              <Table.Th>Days Overdue</Table.Th>
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
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="dimmed">No overdue loans.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((loan) => (
              <Table.Tr key={loan.loanId}>
                <Table.Td>{loan.memberName}</Table.Td>
                <Table.Td>{loan.bookTitle}</Table.Td>
                <Table.Td>{loan.branchName}</Table.Td>
                <Table.Td>{new Date(loan.dueDateUtc).toLocaleDateString()}</Table.Td>
                <Table.Td>
                  <Badge color="red" variant="light">
                    {loan.daysOverdue} {loan.daysOverdue === 1 ? 'day' : 'days'}
                  </Badge>
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
    </>
  );
}
