import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { listMatches, getStandings } from '../api/endpoints';
import DivisionNav from '../components/DivisionNav';
import { EmptyState, MatchStatusBadge, StageBadge, IconTrophy, IconFlag } from '../components/ui';

const medalClass = (rank: number) =>
  rank === 1 ? 'gold' : rank === 2 ? 'silver' : rank === 3 ? 'bronze' : '';

export default function Board() {
  const { divisionId = '' } = useParams();

  const { data: matches = [] } = useQuery({
    queryKey: ['matches', divisionId],
    queryFn: () => listMatches(divisionId),
    refetchInterval: 4000,
  });
  const { data: standings = [] } = useQuery({
    queryKey: ['standings', divisionId],
    queryFn: () => getStandings(divisionId),
    refetchInterval: 4000,
  });

  const liveCount = matches.filter((m) => m.status === 'InProgress').length;

  return (
    <div className="shell">
      <DivisionNav live />
      <main className="page board">
        <div className="page-head rise">
          <div className="titles">
            <p className="eyebrow">Division · Live Board</p>
            <h1>Scoreboard</h1>
          </div>
          <span className="chip">
            <span
              className="badge badge-live"
              style={{ padding: 0, border: 0, background: 'none' }}
            >
              <span className="dot" />
            </span>
            {liveCount > 0
              ? `${liveCount} match${liveCount > 1 ? 'es' : ''} live`
              : 'Auto-refreshing'}
          </span>
        </div>

        <div className="board-grid rise rise-1">
          {/* ---- Standings ---- */}
          <section className="card">
            <div className="card-head">
              <h2>
                <IconTrophy size={18} /> Standings
              </h2>
              <span className="chip">{standings.length} teams</span>
            </div>
            {standings.length === 0 ? (
              <EmptyState
                icon={<IconTrophy size={22} />}
                title="No ranked scores yet"
                hint="Standings appear once ranking matches are completed."
              />
            ) : (
              <div className="standings">
                {standings.map((r) => (
                  <div key={r.teamId} className={`standing-row ${medalClass(r.rank)}`}>
                    <span className={`rank ${medalClass(r.rank)}`}>{r.rank}</span>
                    <div className="standing-team">
                      <span className="mono team-num">#{r.teamNumber}</span>
                      <span className="team-name">{r.teamName}</span>
                    </div>
                    <div className="standing-stats">
                      <span className="stat best">
                        <b className="mono">{r.bestScore}</b>
                        <i>best</i>
                      </span>
                      <span className="stat">
                        <b className="mono">{r.totalScore}</b>
                        <i>total</i>
                      </span>
                      <span className="stat">
                        <b className="mono">{r.matchesPlayed}</b>
                        <i>played</i>
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </section>

          {/* ---- Live matches ---- */}
          <section className="card">
            <div className="card-head">
              <h2>
                <IconFlag size={18} /> Matches
              </h2>
            </div>
            <div className="card-body stack" style={{ gap: 12 }}>
              {matches.length === 0 ? (
                <EmptyState
                  icon={<IconFlag size={22} />}
                  title="No matches yet"
                  hint="Scheduled matches will show here."
                />
              ) : (
                matches.map((m) => (
                  <div
                    key={m.id}
                    className={`board-match${m.status === 'InProgress' ? ' is-live' : ''}`}
                  >
                    <div className="board-match-head">
                      <span className="mono cell-strong">
                        R{m.round} · M{m.number}
                      </span>
                      <StageBadge stage={m.stage} />
                      <span className="spacer" />
                      <MatchStatusBadge status={m.status} />
                    </div>
                    <ul className="board-parts">
                      {m.participants.map((p) => (
                        <li key={p.id}>
                          <span className="chip-table">{p.tableName}</span>
                          <span className="bp-team">
                            <span className="mono dim">#{p.teamNumber}</span> {p.teamName}
                          </span>
                          <span className="mono bp-score">{p.score}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))
              )}
            </div>
          </section>
        </div>
      </main>
    </div>
  );
}
