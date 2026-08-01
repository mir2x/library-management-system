import { useState } from 'react';
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
import { useBranches } from './useBranches';
import { useDeleteBranch } from './useBranchMutations';
import { BranchFormModal } from './BranchFormModal';
import type { Branch } from './types';

const PAGE_SIZE = 20;

export function BranchesPage() {
  const { user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Admin) ?? false;

  const [search, setSearch] = useState('');
  const [debouncedSearch] = useDebouncedValue(search, 300);
  const [pageNumber, setPageNumber] = useState(1);
  const [editingBranch, setEditingBranch] = useState<Branch | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);

  const { data, isLoading, isError } = useBranches({
    search: debouncedSearch,
    pageNumber,
    pageSize: PAGE_SIZE,
  });
  const deleteBranch = useDeleteBranch();

  function openCreateForm() {
    setEditingBranch(null);
    setIsFormOpen(true);
  }

  function openEditForm(branch: Branch) {
    setEditingBranch(branch);
    setIsFormOpen(true);
  }

  function confirmDeactivate(branch: Branch) {
    modals.openConfirmModal({
      title: 'Deactivate branch',
      children: <Text size="sm">Deactivate "{branch.name}"? It can still be viewed but no longer used for new activity.</Text>,
      labels: { confirm: 'Deactivate', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteBranch.mutateAsync(branch.id);
          notifications.show({ color: 'green', message: 'Branch deactivated.' });
        } catch (error) {
          notifications.show({ color: 'red', title: 'Deactivation failed', message: extractErrorMessage(error) });
        }
      },
    });
  }

  return (
    <Container py="xl" size="lg">
      <Group justify="space-between" mb="lg">
        <Title order={2}>Branches</Title>
        {isAdmin && <Button onClick={openCreateForm}>New Branch</Button>}
      </Group>

      <TextInput
        placeholder="Search by name or address"
        leftSection={<IconSearch size={16} />}
        value={search}
        onChange={(event) => {
          setSearch(event.currentTarget.value);
          setPageNumber(1);
        }}
        mb="md"
        maw={400}
      />

      <Table.ScrollContainer minWidth={600}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>Address</Table.Th>
              <Table.Th>Contact</Table.Th>
              <Table.Th>Status</Table.Th>
              {isAdmin && <Table.Th />}
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="red">Failed to load branches.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="dimmed">No branches found.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((branch) => (
              <Table.Tr key={branch.id}>
                <Table.Td>{branch.name}</Table.Td>
                <Table.Td>{branch.address}</Table.Td>
                <Table.Td>{branch.contactNumber ?? branch.email ?? '—'}</Table.Td>
                <Table.Td>
                  <Badge color={branch.isActive ? 'green' : 'gray'} variant="light">
                    {branch.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </Table.Td>
                {isAdmin && (
                  <Table.Td>
                    <Group gap="xs" justify="flex-end">
                      <ActionIcon variant="subtle" onClick={() => openEditForm(branch)} aria-label="Edit branch">
                        <IconEdit size={16} />
                      </ActionIcon>
                      {branch.isActive && (
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => confirmDeactivate(branch)}
                          aria-label="Deactivate branch"
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

      {isAdmin && (
        <BranchFormModal opened={isFormOpen} onClose={() => setIsFormOpen(false)} branch={editingBranch} />
      )}
    </Container>
  );
}
