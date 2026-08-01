import { useForm } from '@mantine/form';
import { Button, Group, Modal, Select } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBranches } from '../branches/useBranches';
import { useBooks } from '../books/useBooks';
import { useMembers } from '../members/useMembers';
import { useCreateReservation } from './useReservationMutations';

interface CreateReservationFormValues {
  memberId: string;
  bookId: string;
  branchId: string;
}

interface CreateReservationModalProps {
  opened: boolean;
  onClose: () => void;
}

export function CreateReservationModal({ opened, onClose }: CreateReservationModalProps) {
  const { data: membersPage } = useMembers({ pageNumber: 1, pageSize: 100 });
  const { data: booksPage } = useBooks({ pageNumber: 1, pageSize: 100 });
  const { data: branchesPage } = useBranches({ pageNumber: 1, pageSize: 100 });
  const createReservation = useCreateReservation();

  const form = useForm<CreateReservationFormValues>({
    initialValues: { memberId: '', bookId: '', branchId: '' },
    validate: {
      memberId: (value) => (value ? null : 'Select a member.'),
      bookId: (value) => (value ? null : 'Select a book.'),
      branchId: (value) => (value ? null : 'Select a branch.'),
    },
  });

  async function handleSubmit(values: CreateReservationFormValues) {
    try {
      await createReservation.mutateAsync(values);
      notifications.show({ color: 'green', message: 'Reservation created.' });
      form.reset();
      onClose();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Reservation failed', message: extractErrorMessage(error) });
    }
  }

  const memberOptions =
    membersPage?.items.map((member) => ({
      value: member.id,
      label: `${member.fullName} (${member.membershipNumber})`,
    })) ?? [];
  const bookOptions = booksPage?.items.map((book) => ({ value: book.id, label: `${book.title} — ${book.author}` })) ?? [];
  const branchOptions = branchesPage?.items.map((branch) => ({ value: branch.id, label: branch.name })) ?? [];

  return (
    <Modal
      opened={opened}
      onClose={() => {
        form.reset();
        onClose();
      }}
      title="Reserve a Book"
      centered
    >
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          label="Member"
          placeholder="Search for a member"
          required
          searchable
          data={memberOptions}
          {...form.getInputProps('memberId')}
        />
        <Select
          label="Book"
          placeholder="Search for a book"
          required
          searchable
          mt="sm"
          data={bookOptions}
          {...form.getInputProps('bookId')}
        />
        <Select
          label="Branch"
          placeholder="Select a branch"
          required
          mt="sm"
          data={branchOptions}
          {...form.getInputProps('branchId')}
        />

        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={createReservation.isPending}>
            Reserve
          </Button>
        </Group>
      </form>
    </Modal>
  );
}
