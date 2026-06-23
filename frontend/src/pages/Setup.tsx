import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  listEnrolledTeams, enrollTeam, markArrived, listTables, createTable,
  listTeams, listMatches, createMatch,
} from '../api/endpoints';
import DivisionNav from '../components/DivisionNav';
import {
  Loading, ErrorBanner, EmptyState, MatchStatusBadge, StageBadge,
  IconPlus, IconUsers, IconGrid, IconCheck, IconClipboard, IconTrash,
} from '../components/ui';

export default function Setup() {
  const { divisionId = '' } = useParams();
  const qc = useQueryClient();
  const refetch = (key: unknown[]) => qc.invalidateQueries({ queryKey: key });

  const { data: enrolled = [], isLoading: loadingEnrolled } = useQuery({ queryKey: ['enrolled', divisionId], queryFn: () => listEnrolledTeams(divisionId) });
  const { data: allTeams = [] } = useQuery({ queryKey: ['teams'], queryFn: listTeams });
  const { data: tables = [] }   = useQuery({ queryKey: ['tables', divisionId], queryFn: () => listTables(divisionId) });
  const { data: matches = [] }  = useQuery({ queryKey: ['matches', divisionId], queryFn: () => listMatches(divisionId) });

  const [teamToEnroll, setTeamToEnroll] = useState('');
  const enroll = useMutation({ mutationFn: () => enrollTeam(divisionId, teamToEnroll), onSuccess: () => { refetch(['enrolled', divisionId]); setTeamToEnroll(''); } });
  const arrive = useMutation({ mutationFn: (teamId: string) => markArrived(divisionId, teamId), onSuccess: () => refetch(['enrolled', divisionId]) });

  const [tableName, setTableName] = useState('');
  const addTable = useMutation({ mutationFn: () => createTable(divisionId, tableName.trim()), onSuccess: () => { refetch(['tables', divisionId]); setTableName(''); } });

  const [round, setRound] = useState(1);
  const [number, setNumber] = useState(1);
  const [stage, setStage] = useState('Ranking');
  const [rows, setRows] = useState<{ tableId: string; teamId: string }[]>([{ tableId: '', teamId: '' }]);
  const addMatch = useMutation({
    mutationFn: () => createMatch(divisionId, { round, number, stage, participants: rows.filter((r) => r.tableId && r.teamId) }),
    onSuccess: () => { refetch(['matches', divisionId]); setNumber((n) => n + 1); setRows([{ tableId: '', teamId: '' }]); },
  });

  const notEnrolled = allTeams.filter((t) => !enrolled.some((e) => e.teamId === t.id));
  const setRow = (i: number, patch: Partial<{ tableId: string; teamId: string }>) =>
    setRows((prev) => prev.map((r, idx) => (idx === i ? { ...r, ...patch } : r)));
  const arrivedCount = enrolled.filter((e) => e.arrived).length;

  return (
    <div className="shell">
      <DivisionNav />
      <main className="page">
        <div className="page-head rise">
          <div className="titles">
            <p className="eyebrow">Division · Setup</p>
            <h1>Event setup</h1>
          </div>
          <div className="row-wrap">
            <span className="chip"><IconUsers size={15} /> {enrolled.length} enrolled</span>
            <span className="chip"><IconGrid size={15} /> {tables.length} tables</span>
            <span className="chip"><IconClipboard size={15} /> {matches.length} matches</span>
          </div>
        </div>

        <div className="grid-2 rise rise-1">
          {/* ---- Teams ---- */}
          <section className="card">
            <div className="card-head">
              <h2><IconUsers size={18} /> Teams</h2>
              <span className="badge badge-slate">{arrivedCount}/{enrolled.length} arrived</span>
            </div>
            <div className="card-body stack">
              <div className="row">
                <select className="select" value={teamToEnroll} onChange={(e) => setTeamToEnroll(e.target.value)}>
                  <option value="">Enroll a team…</option>
                  {notEnrolled.map((t) => <option key={t.id} value={t.id}>#{t.number} · {t.name}</option>)}
                </select>
                <button className="btn btn-primary" onClick={() => teamToEnroll && enroll.mutate()} disabled={!teamToEnroll || enroll.isPending}>
                  <IconPlus size={16} /> Enroll
                </button>
              </div>
              {loadingEnrolled ? <Loading />
                : enrolled.length === 0 ? <EmptyState icon={<IconUsers size={22} />} title="No teams enrolled" hint="Add teams in the Teams catalog first, then enroll them here." />
                : (
                  <ul className="list">
                    {enrolled.map((e) => (
                      <li key={e.teamId} className="list-row">
                        <span className="mono" style={{ color: 'var(--primary)', minWidth: 56 }}>#{e.number}</span>
                        <span className="cell-strong" style={{ flex: 1 }}>{e.name}</span>
                        {e.arrived
                          ? <span className="badge badge-live"><IconCheck size={12} /> Arrived</span>
                          : <button className="btn btn-sm" onClick={() => arrive.mutate(e.teamId)} disabled={arrive.isPending}>Mark arrived</button>}
                      </li>
                    ))}
                  </ul>
                )}
            </div>
          </section>

          {/* ---- Tables ---- */}
          <section className="card">
            <div className="card-head"><h2><IconGrid size={18} /> Tables</h2></div>
            <div className="card-body stack">
              <div className="row">
                <input className="input" placeholder="Table name (e.g. Table A)" value={tableName} onChange={(e) => setTableName(e.target.value)}
                       onKeyDown={(e) => { if (e.key === 'Enter' && tableName.trim()) addTable.mutate(); }} />
                <button className="btn btn-primary" onClick={() => tableName.trim() && addTable.mutate()} disabled={addTable.isPending || !tableName.trim()}>
                  <IconPlus size={16} /> Add
                </button>
              </div>
              {tables.length === 0
                ? <EmptyState icon={<IconGrid size={22} />} title="No tables" hint="Add the competition tables where matches are played." />
                : (
                  <div className="chip-grid">
                    {tables.map((t) => <span key={t.id} className="chip"><IconGrid size={14} /> {t.name}</span>)}
                  </div>
                )}
            </div>
          </section>
        </div>

        <div className="grid-2 rise rise-2" style={{ marginTop: 20 }}>
          {/* ---- New match ---- */}
          <section className="card">
            <div className="card-head"><h2><IconPlus size={18} /> New match</h2></div>
            <div className="card-body stack">
              <div className="row-wrap">
                <div className="field" style={{ width: 90 }}>
                  <label className="label">Round</label>
                  <input className="input mono" type="number" value={round} onChange={(e) => setRound(Number(e.target.value))} />
                </div>
                <div className="field" style={{ width: 90 }}>
                  <label className="label">Match</label>
                  <input className="input mono" type="number" value={number} onChange={(e) => setNumber(Number(e.target.value))} />
                </div>
                <div className="field" style={{ flex: 1, minWidth: 130 }}>
                  <label className="label">Stage</label>
                  <select className="select" value={stage} onChange={(e) => setStage(e.target.value)}>
                    <option>Practice</option><option>Ranking</option><option>Test</option>
                  </select>
                </div>
              </div>

              <div className="stack" style={{ gap: 8 }}>
                <label className="label">Participants</label>
                {rows.map((r, i) => (
                  <div key={i} className="row">
                    <select className="select" value={r.tableId} onChange={(e) => setRow(i, { tableId: e.target.value })}>
                      <option value="">Table…</option>
                      {tables.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
                    </select>
                    <select className="select" value={r.teamId} onChange={(e) => setRow(i, { teamId: e.target.value })}>
                      <option value="">Team…</option>
                      {enrolled.map((e) => <option key={e.teamId} value={e.teamId}>#{e.number} {e.name}</option>)}
                    </select>
                    {rows.length > 1 && (
                      <button className="btn btn-ghost btn-icon" title="Remove" onClick={() => setRows((p) => p.filter((_, idx) => idx !== i))}>
                        <IconTrash size={15} />
                      </button>
                    )}
                  </div>
                ))}
              </div>

              <div className="row">
                <button className="btn btn-sm" onClick={() => setRows((p) => [...p, { tableId: '', teamId: '' }])}>
                  <IconPlus size={14} /> Participant
                </button>
                <span className="spacer" />
                <button className="btn btn-primary" onClick={() => addMatch.mutate()} disabled={addMatch.isPending}>
                  Create match
                </button>
              </div>
              {addMatch.error && <ErrorBanner>{(addMatch.error as Error).message}</ErrorBanner>}
            </div>
          </section>

          {/* ---- Matches ---- */}
          <section className="card">
            <div className="card-head">
              <h2><IconClipboard size={18} /> Matches</h2>
              <span className="chip">{matches.length}</span>
            </div>
            <div className="card-body">
              {matches.length === 0
                ? <EmptyState icon={<IconClipboard size={22} />} title="No matches scheduled" hint="Build a match on the left to add it to the run of show." />
                : (
                  <ul className="list">
                    {matches.map((m) => (
                      <li key={m.id} className="list-row">
                        <span className="mono cell-strong" style={{ minWidth: 74 }}>R{m.round}·M{m.number}</span>
                        <StageBadge stage={m.stage} />
                        <span className="dim mono" style={{ flex: 1, fontSize: '0.78rem' }}>{m.participants.length} teams</span>
                        <MatchStatusBadge status={m.status} />
                      </li>
                    ))}
                  </ul>
                )}
            </div>
          </section>
        </div>
      </main>
    </div>
  );
}
