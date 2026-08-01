import { useEffect } from 'react';
import { Button, Group, Modal, Select, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBranches } from '../branches/useBranches';
import { useCreateMember, useUpdateMember } from './useMemberMutations';
import type { Member } from './types';

interface MemberFormValues {
  fullName: string;
  email: string;
  phone: string;
  address: string;
  homeBranchId: string;
}

interface MemberFormModalProps {
  opened: boolean;
  onClose: () => void;
  /** null creates a new walk-in member; a Member edits it. */
  member: Member | null;
}

export function MemberFormModal({ opened, onClose, member }: MemberFormModalProps) {
  const isEditMode = member !== null;
  const { data: branchesPage } = useBranches({ pageNumber: 1, pageSize: 100 });
  const createMember = useCreateMember();
  const updateMember = useUpdateMember(member?.id ?? '');

  const form = useForm<MemberFormValues>({
    initialValues: { fullName: '', email: '', phone: '', address: '', homeBranchId: '' },
    validate: {
      fullName: (value) => (value.trim().length > 0 ? null : 'Full name is required.'),
      email: (value) => (/^\S+@\S+\.\S+$/.test(value) ? null : 'Enter a valid email address.'),
      homeBranchId: (value) => (value ? null : 'Select a home branch.'),
    },
  });

  useEffect(() => {
    if (opened) {
      form.setValues({
        fullName: member?.fullName ?? '',
        email: member?.email ?? '',
        phone: member?.phone ?? '',
        address: member?.address ?? '',
        homeBranchId: member?.homeBranchId ?? '',
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, member]);

  async function handleSubmit(values: MemberFormValues) {
    const payload = {
      fullName: values.fullName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim() || null,
      address: values.address.trim() || null,
      homeBranchId: values.homeBranchId,
    };

    try {
      if (isEditMode) {
        await updateMember.mutateAsync(payload);
      } else {
        await createMember.mutateAsync(payload);
      }

      notifications.show({ color: 'green', message: isEditMode ? 'Member updated.' : 'Member registered.' });
      onClose();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Save failed', message: extractErrorMessage(error) });
    }
  }

  const branchOptions = branchesPage?.items.map((branch) => ({ value: branch.id, label: branch.name })) ?? [];
  const isSubmitting = createMember.isPending || updateMember.isPending;

  return (
    <Modal opened={opened} onClose={onClose} title={isEditMode ? 'Edit Member' : 'Register Walk-in Member'} centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput name="fullName" label="Full Name" required {...form.getInputProps('fullName')} />
        <TextInput name="email" type="email" label="Email" required mt="sm" {...form.getInputProps('email')} />
        <TextInput name="phone" label="Phone" mt="sm" {...form.getInputProps('phone')} />
        <TextInput name="address" label="Address" mt="sm" {...form.getInputProps('address')} />
        <Select
          label="Home Branch"
          placeholder="Select a branch"
          required
          mt="sm"
          data={branchOptions}
          {...form.getInputProps('homeBranchId')}
        />

        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isSubmitting}>
            {isEditMode ? 'Save' : 'Register'}
          </Button>
        </Group>
      </form>
    </Modal>
  );
}
