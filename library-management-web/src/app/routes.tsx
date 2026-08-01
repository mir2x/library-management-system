import { createBrowserRouter } from 'react-router-dom';

export const router = createBrowserRouter([
  {
    path: '/',
    // Placeholder root route — replaced by the Auth module with the real login page and
    // role-guarded route tree.
    element: <div style={{ padding: 'var(--mantine-spacing-md)' }}>Library Management System</div>,
  },
]);
