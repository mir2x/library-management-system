import { createBrowserRouter } from 'react-router-dom';
import { LoginPage } from '../features/auth/LoginPage';
import { DashboardPage } from '../features/dashboard/DashboardPage';
import { BranchesPage } from '../features/branches/BranchesPage';
import { BooksPage } from '../features/books/BooksPage';
import { BookDetailPage } from '../features/books/BookDetailPage';
import { MembersPage } from '../features/members/MembersPage';
import { LoansPage } from '../features/loans/LoansPage';
import { MyLoansPage } from '../features/loans/MyLoansPage';
import { ReservationsPage } from '../features/reservations/ReservationsPage';
import { MyReservationsPage } from '../features/reservations/MyReservationsPage';
import { ComingSoonPage } from '../components/ComingSoonPage';
import { ProtectedRoute } from './ProtectedRoute';
import { AppShell } from './AppShell';
import { Roles } from '../lib/roles';

const staffRoles = [Roles.Admin, Roles.Librarian];

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { path: '/', element: <DashboardPage /> },
          { path: '/my-loans', element: <MyLoansPage /> },
          { path: '/my-reservations', element: <MyReservationsPage /> },
          {
            element: <ProtectedRoute roles={staffRoles} />,
            children: [
              { path: '/branches', element: <BranchesPage /> },
              { path: '/books', element: <BooksPage /> },
              { path: '/books/:id', element: <BookDetailPage /> },
              { path: '/members', element: <MembersPage /> },
              { path: '/loans', element: <LoansPage /> },
              { path: '/reservations', element: <ReservationsPage /> },
              { path: '/reports', element: <ComingSoonPage title="Reports" /> },
            ],
          },
        ],
      },
    ],
  },
]);
