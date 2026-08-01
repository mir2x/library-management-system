import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ActionIcon,
  Badge,
  Button,
  Center,
  Container,
  Group,
  Loader,
  Paper,
  Table,
  Text,
  Title,
} from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconArrowLeft, IconEdit, IconPlus } from '@tabler/icons-react';
import { useAuth } from '../auth/useAuth';
import { Roles } from '../../lib/roles';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBook } from './useBooks';
import { useDeleteBook } from './useBookMutations';
import { BookFormModal } from './BookFormModal';
import { SetInventoryModal } from './SetInventoryModal';

export function BookDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Admin) ?? false;

  const { data: book, isLoading, isError } = useBook(id!);
  const deleteBook = useDeleteBook();

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [inventoryModalBranchId, setInventoryModalBranchId] = useState<string | undefined>(undefined);
  const [isInventoryModalOpen, setIsInventoryModalOpen] = useState(false);

  function openInventoryModal(branchId?: string) {
    setInventoryModalBranchId(branchId);
    setIsInventoryModalOpen(true);
  }

  function confirmDeactivate() {
    if (!book) return;
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

  if (isLoading) {
    return (
      <Center py="xl">
        <Loader />
      </Center>
    );
  }

  if (isError || !book) {
    return (
      <Container py="xl">
        <Text c="red">Failed to load this book.</Text>
      </Container>
    );
  }

  return (
    <Container py="xl" size="lg">
      <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/books')} mb="md">
        Back to Books
      </Button>

      <Group justify="space-between" align="flex-start" mb="lg">
        <div>
          <Group gap="sm">
            <Title order={2}>{book.title}</Title>
            <Badge color={book.isActive ? 'green' : 'gray'} variant="light">
              {book.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </Group>
          <Text c="dimmed">{book.author}</Text>
        </div>
        {isAdmin && (
          <Group>
            <Button variant="default" leftSection={<IconEdit size={16} />} onClick={() => setIsFormOpen(true)}>
              Edit
            </Button>
            {book.isActive && (
              <Button color="red" variant="light" onClick={confirmDeactivate}>
                Deactivate
              </Button>
            )}
          </Group>
        )}
      </Group>

      <Paper withBorder p="md" radius="md" mb="lg">
        <Table>
          <Table.Tbody>
            <Table.Tr>
              <Table.Th w={160}>ISBN</Table.Th>
              <Table.Td>{book.isbn}</Table.Td>
            </Table.Tr>
            <Table.Tr>
              <Table.Th>Genre</Table.Th>
              <Table.Td>{book.genre}</Table.Td>
            </Table.Tr>
            <Table.Tr>
              <Table.Th>Published Year</Table.Th>
              <Table.Td>{book.publishedYear}</Table.Td>
            </Table.Tr>
            <Table.Tr>
              <Table.Th>Description</Table.Th>
              <Table.Td>{book.description ?? '—'}</Table.Td>
            </Table.Tr>
          </Table.Tbody>
        </Table>
      </Paper>

      <Group justify="space-between" mb="sm">
        <Title order={4}>Branch Inventory</Title>
        {isAdmin && (
          <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => openInventoryModal()}>
            Set Inventory
          </Button>
        )}
      </Group>

      <Table.ScrollContainer minWidth={500}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Branch</Table.Th>
              <Table.Th>Total Copies</Table.Th>
              <Table.Th>Available Copies</Table.Th>
              {isAdmin && <Table.Th />}
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {book.inventory.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={4}>
                  <Text c="dimmed">No inventory set at any branch yet.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {book.inventory.map((inventory) => (
              <Table.Tr key={inventory.branchId}>
                <Table.Td>{inventory.branchName}</Table.Td>
                <Table.Td>{inventory.totalCopies}</Table.Td>
                <Table.Td>{inventory.availableCopies}</Table.Td>
                {isAdmin && (
                  <Table.Td>
                    <ActionIcon
                      variant="subtle"
                      onClick={() => openInventoryModal(inventory.branchId)}
                      aria-label="Edit inventory"
                    >
                      <IconEdit size={16} />
                    </ActionIcon>
                  </Table.Td>
                )}
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>

      {isAdmin && (
        <>
          <BookFormModal opened={isFormOpen} onClose={() => setIsFormOpen(false)} book={book} />
          <SetInventoryModal
            opened={isInventoryModalOpen}
            onClose={() => setIsInventoryModalOpen(false)}
            bookId={book.id}
            existingInventory={book.inventory}
            initialBranchId={inventoryModalBranchId}
          />
        </>
      )}
    </Container>
  );
}
