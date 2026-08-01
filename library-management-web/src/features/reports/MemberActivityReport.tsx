import { useState } from 'react';
import { Group, Pagination, Table, Text } from '@mantine/core';
import { useMemberActivityReport } from './useReports';

const PAGE_SIZE = 20;

export function MemberActivityReport({ branchId }: { branchId?: string }) {
  const [pageNumber, setPageNumber] = useState(1);
  const { data, isLoading, isError } = useMemberActivityReport({ branchId, pageNumber, pageSize: PAGE_SIZE });

  return (
    <>
      <Table.ScrollContainer minWidth={700}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Member</Table.Th>
              <Table.Th>Active Loans</Table.Th>
              <Table.Th>Total Loans</Table.Th>
              <Table.Th>Overdue</Table.Th>
              <Table.Th>Active Reservations</Table.Th>
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
                  <Text c="dimmed">No members found.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((member) => (
              <Table.Tr key={member.memberId}>
                <Table.Td>
                  {member.memberName} ({member.membershipNumber})
                </Table.Td>
                <Table.Td>{member.activeLoanCount}</Table.Td>
                <Table.Td>{member.totalLoanCount}</Table.Td>
                <Table.Td>{member.overdueLoanCount}</Table.Td>
                <Table.Td>{member.activeReservationCount}</Table.Td>
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
