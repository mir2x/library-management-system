import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ActionIcon,
  Badge,
  Button,
  Container,
  Group,
  Pagination,
  Table,
  Text,
  TextInput,
  Title,
} from '@mantine/core';
import { useDebouncedValue } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconEdit, IconSearch, IconTrash } from '@tabler/icons-react';
import { useAuth } from '../auth/useAuth';
import { Roles } from '../../lib/roles';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBooks } from './useBooks';
import { useDeleteBook } from './useBookMutations';
import { BookFormModal } from './BookFormModal';
import type { Book } from './types';

const PAGE_SIZE = 20;

export function BooksPage() {
  const { user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Admin) ?? false;
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [debouncedSearch] = useDebouncedValue(search, 300);
  const [pageNumber, setPageNumber] = useState(1);
  const [editingBook, setEditingBook] = useState<Book | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);

  const { data, isLoading, isError } = useBooks({ search: debouncedSearch, pageNumber, pageSize: PAGE_SIZE });
  const deleteBook = useDeleteBook();

  function openCreateForm() {
    setEditingBook(null);
    setIsFormOpen(true);
  }

  function openEditForm(event: React.MouseEvent, book: Book) {
    event.stopPropagation();
    setEditingBook(book);
    setIsFormOpen(true);
  }

  function confirmDeactivate(event: React.MouseEvent, book: Book) {
    event.stopPropagation();
    modals.openConfirmModal({
      title: 'Deactivate book',
      children: <Text size="sm">Deactivate "{book.title}"? It will no longer be available to borrow or reserve.</Text>,
      labels: { confirm: 'Deactivate', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteBook.mutateAsync(book.id);
          notifications.show({ color: 'green', message: 'Book deactivated.' });
        } catch (error) {
          notifications.show({ color: 'red', title: 'Deactivation failed', message: extractErrorMessage(error) });
        }
      },
    });
  }

  return (
    <Container py="xl" size="lg">
      <Group justify="space-between" mb="lg">
        <Title order={2}>Books</Title>
        {isAdmin && <Button onClick={openCreateForm}>New Book</Button>}
      </Group>

      <TextInput
        placeholder="Search by title, author, ISBN, or genre"
        leftSection={<IconSearch size={16} />}
        value={search}
        onChange={(event) => {
          setSearch(event.currentTarget.value);
          setPageNumber(1);
        }}
        mb="md"
        maw={400}
      />

      <Table.ScrollContainer minWidth={700}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Title</Table.Th>
              <Table.Th>Author</Table.Th>
              <Table.Th>Genre</Table.Th>
              <Table.Th>Year</Table.Th>
              <Table.Th>Status</Table.Th>
              {isAdmin && <Table.Th />}
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="red">Failed to load books.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="dimmed">No books found.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((book) => (
              <Table.Tr key={book.id} onClick={() => navigate(`/books/${book.id}`)} style={{ cursor: 'pointer' }}>
                <Table.Td>{book.title}</Table.Td>
                <Table.Td>{book.author}</Table.Td>
                <Table.Td>{book.genre}</Table.Td>
                <Table.Td>{book.publishedYear}</Table.Td>
                <Table.Td>
                  <Badge color={book.isActive ? 'green' : 'gray'} variant="light">
                    {book.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </Table.Td>
                {isAdmin && (
                  <Table.Td>
                    <Group gap="xs" justify="flex-end">
                      <ActionIcon variant="subtle" onClick={(event) => openEditForm(event, book)} aria-label="Edit book">
                        <IconEdit size={16} />
                      </ActionIcon>
                      {book.isActive && (
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={(event) => confirmDeactivate(event, book)}
                          aria-label="Deactivate book"
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      )}
                    </Group>
                  </Table.Td>
                )}
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

      {isAdmin && <BookFormModal opened={isFormOpen} onClose={() => setIsFormOpen(false)} book={editingBook} />}
    </Container>
  );
}
