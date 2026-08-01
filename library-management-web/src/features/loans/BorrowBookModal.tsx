import { useForm } from '@mantine/form';
import { Button, Group, Modal, Select } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBranches } from '../branches/useBranches';
import { useBooks } from '../books/useBooks';
import { useMembers } from '../members/useMembers';
import { useBorrowBook } from './useLoanMutations';

interface BorrowBookFormValues {
  memberId: string;
  bookId: string;
  branchId: string;
}

interface BorrowBookModalProps {
  opened: boolean;
  onClose: () => void;
}

export function BorrowBookModal({ opened, onClose }: BorrowBookModalProps) {
  const { data: membersPage } = useMembers({ pageNumber: 1, pageSize: 100 });
  const { data: booksPage } = useBooks({ pageNumber: 1, pageSize: 100 });
  const { data: branchesPage } = useBranches({ pageNumber: 1, pageSize: 100 });
  const borrowBook = useBorrowBook();

  const form = useForm<BorrowBookFormValues>({
    initialValues: { memberId: '', bookId: '', branchId: '' },
    validate: {
      memberId: (value) => (value ? null : 'Select a member.'),
      bookId: (value) => (value ? null : 'Select a book.'),
      branchId: (value) => (value ? null : 'Select a branch.'),
    },
  });

  async function handleSubmit(values: BorrowBookFormValues) {
    try {
      await borrowBook.mutateAsync(values);
      notifications.show({ color: 'green', message: 'Book borrowed.' });
      form.reset();
      onClose();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Borrow failed', message: extractErrorMessage(error) });
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
      title="Borrow a Book"
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
          <Button type="submit" loading={borrowBook.isPending}>
            Borrow
          </Button>
        </Group>
      </form>
    </Modal>
  );
}
