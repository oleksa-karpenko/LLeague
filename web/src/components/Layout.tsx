import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { logout } from '../api/endpoints';
import { IconCalendar, IconFlag, IconLayers, IconUsers, IconLogout } from './ui';

const tabs = [
  { to: '/admin/seasons',   label: 'Seasons',   icon: <IconCalendar size={16} /> },
  { to: '/admin/events',    label: 'Events',    icon: <IconFlag size={16} /> },
  { to: '/admin/divisions', label: 'Divisions', icon: <IconLayers size={16} /> },
  { to: '/admin/teams',     label: 'Teams',     icon: <IconUsers size={16} /> },
];

export function Brand() {
  return (
    <NavLink to="/admin" className="brand">
      <span className="brand-mark">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round">
          <path d="M7 6v12h5M14 6v12h5" />
        </svg>
      </span>
      <span style={{ display: 'flex', flexDirection: 'column' }}>
        <span className="brand-name">LLeague</span>
        <span className="brand-sub">Tournament Control</span>
      </span>
    </NavLink>
  );
}

export default function Layout() {
  const navigate = useNavigate();
  return (
    <div className="shell">
      <header className="topbar">
        <Brand />
        <nav className="nav-tabs" style={{ marginLeft: 10 }}>
          {tabs.map((t) => (
            <NavLink key={t.to} to={t.to} className={({ isActive }) => `nav-tab${isActive ? ' active' : ''}`}>
              {t.label}
            </NavLink>
          ))}
        </nav>
        <span className="spacer" />
        <button className="btn btn-ghost btn-sm" onClick={() => { logout(); navigate('/login'); }}>
          <IconLogout size={16} /> Log out
        </button>
      </header>
      <main className="page">
        <Outlet />
      </main>
    </div>
  );
}
