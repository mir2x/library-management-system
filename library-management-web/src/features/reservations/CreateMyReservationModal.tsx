import { useForm } from '@mantine/form';
import { Button, Group, Modal, Select } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBranches } from '../branches/useBranches';
import { useBooks } from '../books/useBooks';
import { useCreateMyReservation } from './useReservationMutations';

interface CreateMyReservationFormValues {
  bookId: string;
  branchId: string;
}

interface CreateMyReservationModalProps {
  opened: boolean;
  onClose: () => void;
}

export function CreateMyReservationModal({ opened, onClose }: CreateMyReservationModalProps) {
  const { data: booksPage } = useBooks({ pageNumber: 1, pageSize: 100 });
  const { data: branchesPage } = useBranches({ pageNumber: 1, pageSize: 100 });
  const createMyReservation = useCreateMyReservation();

  const form = useForm<CreateMyReservationFormValues>({
    initialValues: { bookId: '', branchId: '' },
    validate: {
      bookId: (value) => (value ? null : 'Select a book.'),
      branchId: (value) => (value ? null : 'Select a branch.'),
    },
  });

  async function handleSubmit(values: CreateMyReservationFormValues) {
    try {
      await createMyReservation.mutateAsync(values);
      notifications.show({ color: 'green', message: 'Reservation created.' });
      form.reset();
      onClose();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Reservation failed', message: extractErrorMessage(error) });
    }
  }

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
          label="Book"
          placeholder="Search for a book"
          required
          searchable
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
          <Button type="submit" loading={createMyReservation.isPending}>
            Reserve
          </Button>
        </Group>
      </form>
    </Modal>
  );
}
