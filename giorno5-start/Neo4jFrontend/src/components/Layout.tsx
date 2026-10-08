import { NavLink, Outlet } from 'react-router-dom';

const LINKS = [
  { path: '/', label: 'Film' },
  { path: '/explorer', label: 'Explorer' },
  { path: '/persons', label: 'Persone' },
];

export function Layout() {
  return (
    <>
      <nav id="navbar" className="navbar">
        {LINKS.map(l => (
          <NavLink
            key={l.path}
            to={l.path}
            end
            className={({ isActive }) => (isActive ? 'active' : '')}
          >
            {l.label}
          </NavLink>
        ))}
      </nav>
      {/* Qui React Router mostra la view della route attiva. */}
      <main id="main-content" className="main-content">
        <Outlet />
      </main>
    </>
  );
}
