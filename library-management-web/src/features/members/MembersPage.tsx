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
  Tooltip,
} from '@mantine/core';
import { useDebouncedValue } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconEdit, IconPlayerPlay, IconPlayerPause, IconSearch, IconUserOff } from '@tabler/icons-react';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useMembers } from './useMembers';
import { useDeactivateMember, useReactivateMember, useSuspendMember } from './useMemberMutations';
import { MemberFormModal } from './MemberFormModal';
import type { Member, MembershipStatus } from './types';

const PAGE_SIZE = 20;

const STATUS_COLOR: Record<MembershipStatus, string> = {
  Active: 'green',
  Suspended: 'yellow',
  Deactivated: 'gray',
};

export function MembersPage() {
  const [search, setSearch] = useState('');
  const [debouncedSearch] = useDebouncedValue(search, 300);
  const [pageNumber, setPageNumber] = useState(1);
  const [editingMember, setEditingMember] = useState<Member | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);

  const { data, isLoading, isError } = useMembers({ search: debouncedSearch, pageNumber, pageSize: PAGE_SIZE });
  const suspendMember = useSuspendMember();
  const reactivateMember = useReactivateMember();
  const deactivateMember = useDeactivateMember();

  function openCreateForm() {
    setEditingMember(null);
    setIsFormOpen(true);
  }

  function openEditForm(member: Member) {
    setEditingMember(member);
    setIsFormOpen(true);
  }

  async function handleSuspend(member: Member) {
    try {
      await suspendMember.mutateAsync(member.id);
      notifications.show({ color: 'green', message: 'Membership suspended.' });
    } catch (error) {
      notifications.show({ color: 'red', title: 'Suspend failed', message: extractErrorMessage(error) });
    }
  }

  async function handleReactivate(member: Member) {
    try {
      await reactivateMember.mutateAsync(member.id);
      notifications.show({ color: 'green', message: 'Membership reactivated.' });
    } catch (error) {
      notifications.show({ color: 'red', title: 'Reactivate failed', message: extractErrorMessage(error) });
    }
  }

  function confirmDeactivate(member: Member) {
    modals.openConfirmModal({
      title: 'Deactivate membership',
      children: (
        <Text size="sm">
          Deactivate {member.fullName}'s membership? This cannot be undone through the UI.
        </Text>
      ),
      labels: { confirm: 'Deactivate', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deactivateMember.mutateAsync(member.id);
          notifications.show({ color: 'green', message: 'Membership deactivated.' });
        } catch (error) {
          notifications.show({ color: 'red', title: 'Deactivation failed', message: extractErrorMessage(error) });
        }
      },
    });
  }

  return (
    <Container py="xl" size="lg">
      <Group justify="space-between" mb="lg">
        <Title order={2}>Members</Title>
        <Button onClick={openCreateForm}>New Member</Button>
      </Group>

      <TextInput
        placeholder="Search by name, email, or membership number"
        leftSection={<IconSearch size={16} />}
        value={search}
        onChange={(event) => {
          setSearch(event.currentTarget.value);
          setPageNumber(1);
        }}
        mb="md"
        maw={400}
      />

      <Table.ScrollContainer minWidth={800}>
        <Table verticalSpacing="sm" highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Membership #</Table.Th>
              <Table.Th>Name</Table.Th>
              <Table.Th>Email</Table.Th>
              <Table.Th>Home Branch</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {isError && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="red">Failed to load members.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {!isError && !isLoading && data?.items.length === 0 && (
              <Table.Tr>
                <Table.Td colSpan={6}>
                  <Text c="dimmed">No members found.</Text>
                </Table.Td>
              </Table.Tr>
            )}
            {data?.items.map((member) => (
              <Table.Tr key={member.id}>
                <Table.Td>{member.membershipNumber}</Table.Td>
                <Table.Td>{member.fullName}</Table.Td>
                <Table.Td>{member.email}</Table.Td>
                <Table.Td>{member.homeBranchName}</Table.Td>
                <Table.Td>
                  <Badge color={STATUS_COLOR[member.status]} variant="light">
                    {member.status}
                  </Badge>
                </Table.Td>
                <Table.Td>
                  <Group gap="xs" justify="flex-end">
                    <ActionIcon variant="subtle" onClick={() => openEditForm(member)} aria-label="Edit member">
                      <IconEdit size={16} />
                    </ActionIcon>
                    {member.status === 'Active' && (
                      <Tooltip label="Suspend">
                        <ActionIcon
                          variant="subtle"
                          color="yellow"
                          onClick={() => void handleSuspend(member)}
                          aria-label="Suspend member"
                        >
                          <IconPlayerPause size={16} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                    {member.status === 'Suspended' && (
                      <Tooltip label="Reactivate">
                        <ActionIcon
                          variant="subtle"
                          color="green"
                          onClick={() => void handleReactivate(member)}
                          aria-label="Reactivate member"
                        >
                          <IconPlayerPlay size={16} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                    {member.status !== 'Deactivated' && (
                      <Tooltip label="Deactivate">
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => confirmDeactivate(member)}
                          aria-label="Deactivate member"
                        >
                          <IconUserOff size={16} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                  </Group>
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

      <MemberFormModal opened={isFormOpen} onClose={() => setIsFormOpen(false)} member={editingMember} />
    </Container>
  );
}
