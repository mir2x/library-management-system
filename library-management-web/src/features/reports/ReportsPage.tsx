import { useState } from 'react';
import { Container, Select, Tabs, Title } from '@mantine/core';
import { useBranches } from '../branches/useBranches';
import { OverdueLoansReport } from './OverdueLoansReport';
import { MostBorrowedBooksReport } from './MostBorrowedBooksReport';
import { BranchInventoryReport } from './BranchInventoryReport';
import { MemberActivityReport } from './MemberActivityReport';
import { ReservationQueueReport } from './ReservationQueueReport';

export function ReportsPage() {
  const [branchId, setBranchId] = useState<string | undefined>(undefined);
  const { data: branchesPage } = useBranches({ pageNumber: 1, pageSize: 100 });

  const branchOptions = [
    { value: '', label: 'All branches' },
    ...(branchesPage?.items.map((branch) => ({ value: branch.id, label: branch.name })) ?? []),
  ];

  return (
    <Container py="xl" size="lg">
      <Title order={2} mb="lg">
        Reports
      </Title>

      <Select
        label="Branch"
        data={branchOptions}
        value={branchId ?? ''}
        onChange={(value) => setBranchId(value || undefined)}
        allowDeselect={false}
        mb="lg"
        maw={260}
      />

      <Tabs defaultValue="overdue-loans">
        <Tabs.List>
          <Tabs.Tab value="overdue-loans">Overdue Loans</Tabs.Tab>
          <Tabs.Tab value="most-borrowed">Most Borrowed Books</Tabs.Tab>
          <Tabs.Tab value="branch-inventory">Branch Inventory</Tabs.Tab>
          <Tabs.Tab value="member-activity">Member Activity</Tabs.Tab>
          <Tabs.Tab value="reservation-queues">Reservation Queues</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="overdue-loans" pt="md">
          <OverdueLoansReport branchId={branchId} />
        </Tabs.Panel>
        <Tabs.Panel value="most-borrowed" pt="md">
          <MostBorrowedBooksReport branchId={branchId} />
        </Tabs.Panel>
        <Tabs.Panel value="branch-inventory" pt="md">
          <BranchInventoryReport branchId={branchId} />
        </Tabs.Panel>
        <Tabs.Panel value="member-activity" pt="md">
          <MemberActivityReport branchId={branchId} />
        </Tabs.Panel>
        <Tabs.Panel value="reservation-queues" pt="md">
          <ReservationQueueReport branchId={branchId} />
        </Tabs.Panel>
      </Tabs>
    </Container>
  );
}
