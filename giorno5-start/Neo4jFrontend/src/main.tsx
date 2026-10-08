import React from 'react';
import { createRoot } from 'react-dom/client';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import './style.css';

import { Layout } from './components/Layout';
import { Movies } from './views/Movies';
import { Explorer } from './views/Explorer';
import { Persons } from './views/Persons';

// Ogni path mostra una view dentro il Layout, senza ricaricare la pagina.
const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      { index: true, element: <Movies /> },
      { path: 'explorer', element: <Explorer /> },
      { path: 'persons', element: <Persons /> },
      { path: '*', element: <h2 className="not-found">404 — Pagina non trovata</h2> },
    ],
  },
]);

const root = document.getElementById('root')!;
createRoot(root).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>
);
