import { useEffect } from 'react';
import { Button, Group, Modal, NumberInput, Select } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { extractErrorMessage } from '../../lib/errorMessage';
import { useBranches } from '../branches/useBranches';
import { useSetBookInventory } from './useBookMutations';
import type { BookInventory } from './types';

interface SetInventoryFormValues {
  branchId: string;
  totalCopies: number;
}

interface SetInventoryModalProps {
  opened: boolean;
  onClose: () => void;
  bookId: string;
  /** Existing inventory rows, so picking (or preselecting) a branch pre-fills its current copy count. */
  existingInventory: BookInventory[];
  /** Preselect a branch, e.g. when opened from that branch's row in the inventory table. */
  initialBranchId?: string;
}

export function SetInventoryModal({ opened, onClose, bookId, existingInventory, initialBranchId }: SetInventoryModalProps) {
  const { data: branchesPage } = useBranches({ pageNumber: 1, pageSize: 100 });
  const setInventory = useSetBookInventory(bookId);

  const form = useForm<SetInventoryFormValues>({
    initialValues: { branchId: '', totalCopies: 1 },
    validate: {
      branchId: (value) => (value ? null : 'Select a branch.'),
      totalCopies: (value) => (value >= 0 ? null : 'Must be zero or more.'),
    },
  });

  useEffect(() => {
    if (opened) {
      const existing = existingInventory.find((inventory) => inventory.branchId === initialBranchId);
      form.setValues({ branchId: initialBranchId ?? '', totalCopies: existing?.totalCopies ?? 1 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, initialBranchId]);

  function handleBranchChange(branchId: string | null) {
    form.setFieldValue('branchId', branchId ?? '');
    const existing = existingInventory.find((inventory) => inventory.branchId === branchId);
    form.setFieldValue('totalCopies', existing?.totalCopies ?? 1);
  }

  async function handleSubmit(values: SetInventoryFormValues) {
    try {
      await setInventory.mutateAsync({ branchId: values.branchId, totalCopies: values.totalCopies });
      notifications.show({ color: 'green', message: 'Inventory updated.' });
      onClose();
    } catch (error) {
      notifications.show({ color: 'red', title: 'Update failed', message: extractErrorMessage(error) });
    }
  }

  const branchOptions =
    branchesPage?.items.map((branch) => ({ value: branch.id, label: branch.name })) ?? [];

  return (
    <Modal opened={opened} onClose={onClose} title="Set Branch Inventory" centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          label="Branch"
          placeholder="Select a branch"
          required
          data={branchOptions}
          {...form.getInputProps('branchId')}
          onChange={handleBranchChange}
        />
        <NumberInput
          label="Total Copies"
          required
          mt="sm"
          min={0}
          {...form.getInputProps('totalCopies')}
        />

        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={setInventory.isPending}>
            Save
          </Button>
        </Group>
      </form>
    </Modal>
  );
}
