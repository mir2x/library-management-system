import { Table, Text } from '@mantine/core';
import { useBranchInventoryReport } from './useReports';

export function BranchInventoryReport({ branchId }: { branchId?: string }) {
  const { data, isLoading, isError } = useBranchInventoryReport(branchId);

  return (
    <Table.ScrollContainer minWidth={700}>
      <Table verticalSpacing="sm" highlightOnHover>
        <Table.Thead>
          <Table.Tr>
            <Table.Th>Branch</Table.Th>
            <Table.Th>Titles</Table.Th>
            <Table.Th>Total Copies</Table.Th>
            <Table.Th>Available Copies</Table.Th>
            <Table.Th>Utilization</Table.Th>
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
                <Text c="dimmed">No branches found.</Text>
              </Table.Td>
            </Table.Tr>
          )}
          {data?.map((summary) => (
            <Table.Tr key={summary.branchId}>
              <Table.Td>{summary.branchName}</Table.Td>
              <Table.Td>{summary.totalTitles}</Table.Td>
              <Table.Td>{summary.totalCopies}</Table.Td>
              <Table.Td>{summary.availableCopies}</Table.Td>
              <Table.Td>{summary.utilizationPercentage.toFixed(1)}%</Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  );
}
