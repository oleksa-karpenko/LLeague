import { NavLink, useParams } from 'react-router-dom';
import { Brand } from './Layout';
import { IconWrench, IconClipboard, IconFlag, IconBack } from './ui';

export default function DivisionNav({ live }: { live?: boolean }) {
  const { divisionId = '' } = useParams();
  const base = `/division/${divisionId}`;
  const tabs = [
    { to: `${base}/setup`, label: 'Setup', icon: <IconWrench size={16} /> },
    { to: `${base}/score`, label: 'Score', icon: <IconClipboard size={16} /> },
    { to: `${base}/board`, label: 'Board', icon: <IconFlag size={16} /> },
  ];
  return (
    <header className="topbar">
      <Brand />
      <nav className="nav-tabs" style={{ marginLeft: 10 }}>
        {tabs.map((t) => (
          <NavLink
            key={t.to}
            to={t.to}
            className={({ isActive }) => `nav-tab${isActive ? ' active' : ''}`}
          >
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: 7 }}>
              {t.icon}
              {t.label}
            </span>
          </NavLink>
        ))}
      </nav>
      <span className="spacer" />
      {live && (
        <span className="badge badge-live" style={{ marginRight: 4 }}>
          <span className="dot" />
          Live
        </span>
      )}
      <NavLink to="/admin/divisions" className="btn btn-ghost btn-sm">
        <IconBack size={16} /> Divisions
      </NavLink>
    </header>
  );
}
