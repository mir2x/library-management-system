import { useEffect } from 'react';
import { Button, Group, Modal, NumberInput, Textarea, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useCreateBook, useUpdateBook } from './useBookMutations';
import type { Book } from './types';

const CURRENT_YEAR = new Date().getFullYear();

interface BookFormValues {
  title: string;
  author: string;
  isbn: string;
  genre: string;
  publishedYear: number;
  description: string;
}

interface BookFormModalProps {
  opened: boolean;
  onClose: () => void;
  /** null creates a new book; a Book edits it (ISBN becomes read-only). */
  book: Book | null;
  onSaved?: () => void;
}

export function BookFormModal({ opened, onClose, book, onSaved }: BookFormModalProps) {
  const isEditMode = book !== null;
  const createBook = useCreateBook();
  const updateBook = useUpdateBook(book?.id ?? '');

  const form = useForm<BookFormValues>({
    initialValues: { title: '', author: '', isbn: '', genre: '', publishedYear: CURRENT_YEAR, description: '' },
    validate: {
      title: (value) => (value.trim().length > 0 ? null : 'Title is required.'),
      author: (value) => (value.trim().length > 0 ? null : 'Author is required.'),
      isbn: (value) => (isEditMode || value.trim().length > 0 ? null : 'ISBN is required.'),
      genre: (value) => (value.trim().length > 0 ? null : 'Genre is required.'),
      publishedYear: (value) =>
        value >= 1450 && value <= CURRENT_YEAR ? null : `Must be between 1450 and ${CURRENT_YEAR}.`,
    },
  });

  useEffect(() => {
    if (opened) {
      form.setValues({
        title: book?.title ?? '',
        author: book?.author ?? '',
        isbn: book?.isbn ?? '',
        genre: book?.genre ?? '',
        publishedYear: book?.publishedYear ?? CURRENT_YEAR,
        description: book?.description ?? '',
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, book]);

  async function handleSubmit(values: BookFormValues) {
    try {
      if (isEditMode) {
        await updateBook.mutateAsync({
          title: values.title.trim(),
          author: values.author.trim(),
          genre: values.genre.trim(),
          publishedYear: values.publishedYear,
          description: values.description.trim() || null,
        });
      } else {
        await createBook.mutateAsync({
          title: values.title.trim(),
          author: values.author.trim(),
          isbn: values.isbn.trim(),
          genre: values.genre.trim(),
          publishedYear: values.publishedYear,
          description: values.description.trim() || null,
        });
      }

      notifications.show({ color: 'green', message: isEditMode ? 'Book updated.' : 'Book created.' });
      onClose();
      onSaved?.();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Save failed', message: extractErrorMessage(error) });
    }
  }

  const isSubmitting = createBook.isPending || updateBook.isPending;

  return (
    <Modal opened={opened} onClose={onClose} title={isEditMode ? 'Edit Book' : 'New Book'} centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput name="title" label="Title" required {...form.getInputProps('title')} />
        <TextInput name="author" label="Author" required mt="sm" {...form.getInputProps('author')} />
        <TextInput
          name="isbn"
          label="ISBN"
          required
          mt="sm"
          disabled={isEditMode}
          description={isEditMode ? 'ISBN cannot be changed after creation.' : undefined}
          {...form.getInputProps('isbn')}
        />
        <TextInput name="genre" label="Genre" required mt="sm" {...form.getInputProps('genre')} />
        <NumberInput
          name="publishedYear"
          label="Published Year"
          required
          mt="sm"
          min={1450}
          max={CURRENT_YEAR}
          {...form.getInputProps('publishedYear')}
        />
        <Textarea name="description" label="Description" mt="sm" autosize minRows={2} {...form.getInputProps('description')} />

        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isSubmitting}>
            {isEditMode ? 'Save' : 'Create'}
          </Button>
        </Group>
      </form>
    </Modal>
  );
}
