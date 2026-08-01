import { useEffect } from 'react';
import { Button, Group, Modal, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useCreateBranch, useUpdateBranch } from './useBranchMutations';
import type { Branch } from './types';

interface BranchFormValues {
  name: string;
  address: string;
  contactNumber: string;
  email: string;
}

interface BranchFormModalProps {
  opened: boolean;
  onClose: () => void;
  /** null creates a new branch; a Branch edits it. */
  branch: Branch | null;
}

export function BranchFormModal({ opened, onClose, branch }: BranchFormModalProps) {
  const isEditMode = branch !== null;
  const createBranch = useCreateBranch();
  const updateBranch = useUpdateBranch();

  const form = useForm<BranchFormValues>({
    initialValues: { name: '', address: '', contactNumber: '', email: '' },
    validate: {
      name: (value) => (value.trim().length > 0 ? null : 'Name is required.'),
      address: (value) => (value.trim().length > 0 ? null : 'Address is required.'),
      email: (value) => (value === '' || /^\S+@\S+\.\S+$/.test(value) ? null : 'Enter a valid email address.'),
    },
  });

  useEffect(() => {
    if (opened) {
      form.setValues({
        name: branch?.name ?? '',
        address: branch?.address ?? '',
        contactNumber: branch?.contactNumber ?? '',
        email: branch?.email ?? '',
      });
    }
    // form is intentionally omitted: Mantine's form object is stable in identity but including
    // it here would re-run this effect on every keystroke.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, branch]);

  async function handleSubmit(values: BranchFormValues) {
    const payload = {
      name: values.name.trim(),
      address: values.address.trim(),
      contactNumber: values.contactNumber.trim() || null,
      email: values.email.trim() || null,
    };

    try {
      if (isEditMode) {
        await updateBranch.mutateAsync({ id: branch.id, request: payload });
      } else {
        await createBranch.mutateAsync(payload);
      }

      notifications.show({ color: 'green', message: isEditMode ? 'Branch updated.' : 'Branch created.' });
      onClose();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Save failed', message: extractErrorMessage(error) });
    }
  }

  const isSubmitting = createBranch.isPending || updateBranch.isPending;

  return (
    <Modal opened={opened} onClose={onClose} title={isEditMode ? 'Edit Branch' : 'New Branch'} centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput name="name" label="Name" required {...form.getInputProps('name')} />
        <TextInput name="address" label="Address" required mt="sm" {...form.getInputProps('address')} />
        <TextInput name="contactNumber" label="Contact Number" mt="sm" {...form.getInputProps('contactNumber')} />
        <TextInput name="email" type="email" label="Email" mt="sm" {...form.getInputProps('email')} />

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
