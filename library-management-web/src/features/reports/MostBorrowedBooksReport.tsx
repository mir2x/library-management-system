import { useState } from 'react';
import { NumberInput, Table, Text } from '@mantine/core';
import { useMostBorrowedBooksReport } from './useReports';

export function MostBorrowedBooksReport({ branchId }: { branchId?: string }) {
  const [top, setTop] = useState(10);
  const { data, isLoading, isError } = useMostBorrowedBooksReport({ branchId, top });

  return (
    <>
      <NumberInput label="Show top" value={top} onChange={(value) => setTop(Number(value) || 10)} min={1} max={100} mb="md" maw={160} />

      <Table.ScrollContainer minWidth={500}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Title</Table.Th>
              <Table.Th>Author</Table.Th>
              <Table.Th>Borrow Count</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={3}>
                  <Text c="red">Failed to load this report.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={3}>
                  <Text c="dimmed">No borrowing history yet.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.map((book) => (
              <Table.Tr key={book.bookId}>
                <Table.Td>{book.title}</Table.Td>
                <Table.Td>{book.author}</Table.Td>
                <Table.Td>{book.borrowCount}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>
    </>
  );
}
