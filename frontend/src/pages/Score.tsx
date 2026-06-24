import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  listMatches,
  getMissionCatalog,
  getScoresheet,
  upsertScoresheet,
  submitScoresheet,
  startMatch,
  completeMatch,
} from '../api/endpoints';
import type { MissionValue } from '../api/types';
import DivisionNav from '../components/DivisionNav';
import {
  Loading,
  ErrorBanner,
  EmptyState,
  MatchStatusBadge,
  SheetStatusBadge,
  IconClipboard,
  IconPlay,
  IconCheck,
} from '../components/ui';

export default function Score() {
  const { divisionId = '' } = useParams();
  const qc = useQueryClient();

  const { data: matches = [], isLoading: loadingMatches } = useQuery({
    queryKey: ['matches', divisionId],
    queryFn: () => listMatches(divisionId),
    refetchInterval: 5000,
  });
  const { data: catalog = [] } = useQuery({ queryKey: ['catalog'], queryFn: getMissionCatalog });

  const [participantId, setParticipantId] = useState('');
  const { data: sheet } = useQuery({
    queryKey: ['scoresheet', participantId],
    queryFn: () => getScoresheet(participantId),
    enabled: !!participantId,
  });

  // Seed the editable form from the loaded scoresheet. Done during render (not in an
  // effect) per React guidance for adjusting state when data changes — re-seeds whenever
  // the sheet reference changes (load / save / submit), without cascading effect renders.
  const [values, setValues] = useState<Record<string, string>>({});
  const [seededSheet, setSeededSheet] = useState<typeof sheet>(undefined);
  if (sheet !== seededSheet) {
    setSeededSheet(sheet);
    const next: Record<string, string> = {};
    for (const mv of sheet?.missions ?? [])
      next[`${mv.missionId}:${mv.clauseIndex}`] = mv.value ?? '';
    setValues(next);
  }

  function setVal(key: string, v: string) {
    setValues((prev) => ({ ...prev, [key]: v }));
  }

  function collect(): MissionValue[] {
    const out: MissionValue[] = [];
    for (const mission of catalog) {
      for (const clause of mission.clauses) {
        const key = `${mission.missionId}:${clause.index}`;
        out.push({
          missionId: mission.missionId,
          clauseIndex: clause.index,
          valueType: clause.type,
          value: values[key] ?? null,
        });
      }
    }
    return out;
  }

  const save = useMutation({
    mutationFn: () => upsertScoresheet(participantId, collect()),
    onSuccess: (updated) => {
      qc.setQueryData(['scoresheet', participantId], updated);
      qc.invalidateQueries({ queryKey: ['matches', divisionId] });
    },
  });
  const submit = useMutation({
    mutationFn: () => submitScoresheet(participantId),
    onSuccess: (updated) => {
      qc.setQueryData(['scoresheet', participantId], updated);
      qc.invalidateQueries({ queryKey: ['matches', divisionId] });
    },
  });
  const start = useMutation({
    mutationFn: (id: string) => startMatch(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['matches', divisionId] }),
  });
  const complete = useMutation({
    mutationFn: (id: string) => completeMatch(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['matches', divisionId] }),
  });

  const selected = matches.flatMap((m) => m.participants).find((p) => p.id === participantId);

  return (
    <div className="shell">
      <DivisionNav />
      <main className="page">
        <div className="page-head rise">
          <div className="titles">
            <p className="eyebrow">Division · Score</p>
            <h1>Scoring station</h1>
          </div>
        </div>

        <div className="score-grid rise rise-1">
          {/* ---- Match rail ---- */}
          <aside className="score-rail">
            <div className="rail-title">
              <IconClipboard size={16} /> Matches{' '}
              <span className="dim mono" style={{ fontSize: '0.72rem' }}>
                · live
              </span>
            </div>
            {loadingMatches ? (
              <Loading />
            ) : matches.length === 0 ? (
              <EmptyState
                icon={<IconClipboard size={22} />}
                title="No matches"
                hint="Create matches on the Setup page."
              />
            ) : (
              matches.map((m) => (
                <div key={m.id} className="match-card">
                  <div className="match-card-head">
                    <span className="mono cell-strong">
                      R{m.round} · M{m.number}
                    </span>
                    <MatchStatusBadge status={m.status} />
                  </div>
                  <div className="row" style={{ gap: 6, marginBottom: 8 }}>
                    <button
                      className="btn btn-sm"
                      onClick={() => start.mutate(m.id)}
                      disabled={m.status !== 'NotStarted' || start.isPending}
                    >
                      <IconPlay size={13} /> Start
                    </button>
                    <button
                      className="btn btn-sm"
                      onClick={() => complete.mutate(m.id)}
                      disabled={m.status !== 'InProgress' || complete.isPending}
                    >
                      <IconCheck size={14} /> Complete
                    </button>
                  </div>
                  <div className="stack" style={{ gap: 5 }}>
                    {m.participants.map((p) => (
                      <button
                        key={p.id}
                        onClick={() => setParticipantId(p.id)}
                        className={`participant-btn${p.id === participantId ? ' active' : ''}`}
                      >
                        <span className="chip-table">{p.tableName}</span>
                        <span className="p-team">
                          <span className="mono dim">#{p.teamNumber}</span> {p.teamName}
                        </span>
                        <span className="mono p-score">{p.score}</span>
                        <SheetStatusBadge status={p.scoresheetStatus} />
                      </button>
                    ))}
                  </div>
                </div>
              ))
            )}
          </aside>

          {/* ---- Scoresheet ---- */}
          <section className="card score-sheet">
            <div className="card-head">
              <h2>Scoresheet</h2>
              {selected && (
                <span className="muted mono" style={{ fontSize: '0.82rem' }}>
                  {selected.tableName} · #{selected.teamNumber} {selected.teamName}
                </span>
              )}
            </div>

            {!participantId ? (
              <EmptyState
                icon={<IconClipboard size={22} />}
                title="Pick a participant"
                hint="Select a team from a match on the left to open its scoresheet."
              />
            ) : (
              <>
                <div className="card-body stack" style={{ gap: 14 }}>
                  {catalog.map((mission) => (
                    <fieldset key={mission.missionId} className="mission">
                      <legend>
                        <span className="mono mission-id">{mission.missionId}</span>
                        <span>{mission.title}</span>
                      </legend>
                      <div className="stack" style={{ gap: 2 }}>
                        {mission.clauses.map((clause) => {
                          const key = `${mission.missionId}:${clause.index}`;
                          const val = values[key] ?? '';
                          return (
                            <div key={key} className="clause">
                              <span className="clause-label">{clause.label}</span>
                              {clause.type === 'boolean' && (
                                <label className="checkbox">
                                  <input
                                    type="checkbox"
                                    checked={val === 'true'}
                                    onChange={(e) =>
                                      setVal(key, e.target.checked ? 'true' : 'false')
                                    }
                                  />
                                  <span className="box" />
                                </label>
                              )}
                              {clause.type === 'number' && (
                                <input
                                  className="input mono"
                                  style={{ width: 92 }}
                                  type="number"
                                  min={0}
                                  max={clause.max ?? undefined}
                                  value={val}
                                  onChange={(e) => setVal(key, e.target.value)}
                                />
                              )}
                              {clause.type === 'enum' && (
                                <select
                                  className="select"
                                  style={{ width: 'auto', minWidth: 160 }}
                                  value={val}
                                  onChange={(e) => setVal(key, e.target.value)}
                                >
                                  <option value="">—</option>
                                  {clause.options?.map((o) => (
                                    <option key={o.value} value={o.value}>
                                      {o.label} ({o.points})
                                    </option>
                                  ))}
                                </select>
                              )}
                            </div>
                          );
                        })}
                      </div>
                    </fieldset>
                  ))}
                  {(save.error || submit.error) && (
                    <ErrorBanner>{((save.error || submit.error) as Error).message}</ErrorBanner>
                  )}
                </div>

                <div className="score-actions">
                  <div className="score-total">
                    <span className="eyebrow">Score</span>
                    <span className="score-num mono">{sheet?.score ?? 0}</span>
                  </div>
                  {sheet && <SheetStatusBadge status={sheet.status} />}
                  <span className="spacer" />
                  <button className="btn" onClick={() => save.mutate()} disabled={save.isPending}>
                    Save draft
                  </button>
                  <button
                    className="btn btn-primary"
                    onClick={() => submit.mutate()}
                    disabled={submit.isPending}
                  >
                    <IconCheck size={16} /> Submit
                  </button>
                </div>
              </>
            )}
          </section>
        </div>
      </main>
    </div>
  );
}
