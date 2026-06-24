import type { ReactNode, SVGProps } from 'react';
import type { MatchStatus, ScoresheetStatus, MatchStage } from '../api/types';

/* ============================================================================
   Icons — minimal line set, inherit currentColor
   ========================================================================== */
type IP = SVGProps<SVGSVGElement> & { size?: number };
function svg(size: number | undefined, rest: SVGProps<SVGSVGElement>) {
  const s = size ?? 18;
  return {
    width: s,
    height: s,
    viewBox: '0 0 24 24',
    fill: 'none',
    stroke: 'currentColor',
    strokeWidth: 1.8,
    strokeLinecap: 'round' as const,
    strokeLinejoin: 'round' as const,
    ...rest,
  };
}
export const IconTrophy = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M6 4h12v4a6 6 0 0 1-12 0V4Z" />
    <path d="M6 6H3v1a4 4 0 0 0 4 4M18 6h3v1a4 4 0 0 1-4 4" />
    <path d="M12 14v3m-4 4h8m-6 0v-1a2 2 0 0 1 2-2v0a2 2 0 0 1 2 2v1" />
  </svg>
);
export const IconFlag = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M5 21V4m0 1h11l-2 4 2 4H5" />
  </svg>
);
export const IconWrench = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M15 6a3.5 3.5 0 0 0 4.5 4.5L21 9V4h-5l-1.5 2Z" />
    <path d="M14 9 4.5 18.5a2.1 2.1 0 0 0 3 3L17 12" />
  </svg>
);
export const IconClipboard = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <rect x="5" y="4" width="14" height="17" rx="2" />
    <path d="M9 4h6v3H9z" />
    <path d="M9 12h6M9 16h4" />
  </svg>
);
export const IconPlus = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M12 5v14M5 12h14" />
  </svg>
);
export const IconTrash = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M4 7h16M9 7V5h6v2m-8 0 1 13h8l1-13" />
  </svg>
);
export const IconLogout = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
    <path d="M16 17l5-5-5-5M21 12H9" />
  </svg>
);
export const IconBack = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M15 19l-7-7 7-7" />
  </svg>
);
export const IconCalendar = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <rect x="3" y="5" width="18" height="16" rx="2" />
    <path d="M3 9h18M8 3v4M16 3v4" />
  </svg>
);
export const IconLayers = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="m12 3 9 5-9 5-9-5 9-5Z" />
    <path d="m3 13 9 5 9-5" />
  </svg>
);
export const IconUsers = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <circle cx="9" cy="8" r="3.2" />
    <path d="M3.5 20a5.5 5.5 0 0 1 11 0M16 6a3 3 0 0 1 0 6m1 8a5 5 0 0 0-3-4.6" />
  </svg>
);
export const IconGrid = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <rect x="3" y="3" width="7" height="7" rx="1.5" />
    <rect x="14" y="3" width="7" height="7" rx="1.5" />
    <rect x="3" y="14" width="7" height="7" rx="1.5" />
    <rect x="14" y="14" width="7" height="7" rx="1.5" />
  </svg>
);
export const IconCheck = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="m20 6-11 11-5-5" />
  </svg>
);
export const IconPlay = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M7 4v16l13-8-13-8Z" />
  </svg>
);
export const IconCalendarClock = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <path d="M21 10V6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h6M3 9h18M8 3v3M16 3v3" />
    <circle cx="17.5" cy="16.5" r="4.5" />
    <path d="M17.5 14.8v1.7l1.2 1" />
  </svg>
);
export const IconLock = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <rect x="4" y="10" width="16" height="11" rx="2" />
    <path d="M8 10V7a4 4 0 0 1 8 0v3" />
  </svg>
);
export const IconSearch = ({ size, ...p }: IP) => (
  <svg {...svg(size, p)}>
    <circle cx="11" cy="11" r="7" />
    <path d="m21 21-4.3-4.3" />
  </svg>
);

/* ============================================================================
   State components
   ========================================================================== */
export function Spinner() {
  return <span className="spinner" aria-hidden />;
}

export function Loading({ label = 'Loading…' }: { label?: string }) {
  return (
    <div className="loading-row">
      <Spinner /> <span>{label}</span>
    </div>
  );
}

export function ErrorBanner({ children }: { children: ReactNode }) {
  return (
    <div className="banner-error" role="alert">
      ⚠ {children}
    </div>
  );
}

export function EmptyState({
  icon,
  title,
  hint,
}: {
  icon?: ReactNode;
  title: string;
  hint?: string;
}) {
  return (
    <div className="empty">
      <div className="empty-icon">{icon ?? <IconSearch size={22} />}</div>
      <h3>{title}</h3>
      {hint && (
        <p className="dim" style={{ maxWidth: 320 }}>
          {hint}
        </p>
      )}
    </div>
  );
}

/* ============================================================================
   Status badges
   ========================================================================== */
export function MatchStatusBadge({ status }: { status: MatchStatus }) {
  const map: Record<MatchStatus, { cls: string; label: string }> = {
    NotStarted: { cls: 'badge-slate', label: 'Queued' },
    InProgress: { cls: 'badge-live', label: 'Live' },
    Completed: { cls: 'badge-amber', label: 'Final' },
  };
  const { cls, label } = map[status];
  return (
    <span className={`badge ${cls}`}>
      <span className="dot" />
      {label}
    </span>
  );
}

export function SheetStatusBadge({ status }: { status: ScoresheetStatus }) {
  const map: Record<ScoresheetStatus, string> = {
    Empty: 'badge-slate',
    Draft: 'badge-info',
    Completed: 'badge-live',
  };
  return <span className={`badge ${map[status]}`}>{status}</span>;
}

export function StageBadge({ stage }: { stage: MatchStage }) {
  return <span className="badge badge-slate">{stage}</span>;
}
