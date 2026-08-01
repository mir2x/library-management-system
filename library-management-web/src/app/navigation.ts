import {
  IconArrowsExchange,
  IconBooks,
  IconBookmarks,
  IconBuildingStore,
  IconChartBar,
  IconLayoutDashboard,
  IconUsers,
  type Icon,
} from '@tabler/icons-react';
import { Roles } from '../lib/roles';

const member = [Roles.Member];

export interface NavItem {
  label: string;
  path: string;
  icon: Icon;
  /** Roles allowed to see this item. Omit to show it to any authenticated user. */
  roles?: string[];
}

const staff = [Roles.Admin, Roles.Librarian];

export const navItems: NavItem[] = [
  { label: 'Dashboard', path: '/', icon: IconLayoutDashboard },
  { label: 'Branches', path: '/branches', icon: IconBuildingStore, roles: staff },
  { label: 'Books', path: '/books', icon: IconBooks, roles: staff },
  { label: 'Members', path: '/members', icon: IconUsers, roles: staff },
  { label: 'Borrow & Return', path: '/loans', icon: IconArrowsExchange, roles: staff },
  { label: 'My Loans', path: '/my-loans', icon: IconArrowsExchange, roles: member },
  { label: 'Reservations', path: '/reservations', icon: IconBookmarks, roles: staff },
  { label: 'Reports', path: '/reports', icon: IconChartBar, roles: staff },
];
