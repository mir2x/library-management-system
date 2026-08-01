import { Center, Loader } from '@mantine/core';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';

interface ProtectedRouteProps {
  /** Roles allowed to view the nested routes. Omit to allow any authenticated user. */
  roles?: string[];
}

export function ProtectedRoute({ roles }: ProtectedRouteProps) {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Center h="100vh">
        <Loader />
      </Center>
    );
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  if (roles && !roles.some((role) => user.roles.includes(role))) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
