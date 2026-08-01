import {
  AppShell as MantineAppShell,
  Avatar,
  Burger,
  Button,
  Group,
  NavLink as MantineNavLink,
  Text,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { NavLink as RouterNavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';
import { navItems } from './navigation';

export function AppShell() {
  const [opened, { toggle }] = useDisclosure();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const visibleNavItems = navItems.filter(
    (item) => !item.roles || item.roles.some((role) => user?.roles.includes(role)),
  );

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <MantineAppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <MantineAppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group>
            <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
            <Text fw={700}>Library Management System</Text>
          </Group>
          <Group>
            <div style={{ textAlign: 'right' }}>
              <Text size="sm" fw={500}>
                {user?.fullName}
              </Text>
              <Text size="xs" c="dimmed">
                {user?.roles.join(', ')}
              </Text>
            </div>
            <Avatar radius="xl" color="blue">
              {user?.fullName.charAt(0)}
            </Avatar>
            <Button variant="subtle" color="red" onClick={() => void handleLogout()}>
              Sign out
            </Button>
          </Group>
        </Group>
      </MantineAppShell.Header>

      <MantineAppShell.Navbar p="md">
        {visibleNavItems.map((item) => (
          <MantineNavLink
            key={item.path}
            component={RouterNavLink}
            to={item.path}
            label={item.label}
            leftSection={<item.icon size={18} stroke={1.5} />}
            active={item.path === '/' ? location.pathname === '/' : location.pathname.startsWith(item.path)}
          />
        ))}
      </MantineAppShell.Navbar>

      <MantineAppShell.Main>
        <Outlet />
      </MantineAppShell.Main>
    </MantineAppShell>
  );
}
