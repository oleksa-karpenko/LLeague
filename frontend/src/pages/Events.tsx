import { useState, type FormEvent } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listSeasons, listEvents, createEvent, deleteEvent } from '../api/endpoints';
import { Loading, ErrorBanner, EmptyState, IconPlus, IconTrash, IconFlag } from '../components/ui';

export default function Events() {
  const qc = useQueryClient();
  const { data: seasons = [] } = useQuery({ queryKey: ['seasons'], queryFn: listSeasons });
  const [seasonId, setSeasonId] = useState('');

  const {
    data: events = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['events', seasonId],
    queryFn: () => listEvents(seasonId || undefined),
  });

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [startDate, setStartDate] = useState('2026-03-14');
  const [endDate, setEndDate] = useState('2026-03-14');

  const create = useMutation({
    mutationFn: () =>
      createEvent({
        seasonId,
        name: name.trim(),
        slug: '',
        startDate,
        endDate,
        location: location.trim(),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['events', seasonId] });
      setName('');
      setLocation('');
    },
  });
  const remove = useMutation({
    mutationFn: (id: string) => deleteEvent(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['events', seasonId] }),
  });

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (!seasonId) return;
    if (name.trim()) create.mutate();
  }

  return (
    <section className="rise">
      <div className="page-head">
        <div className="titles">
          <p className="eyebrow">Catalog</p>
          <h1>Events</h1>
        </div>
        <div className="field" style={{ minWidth: 220 }}>
          <label className="label">Filter by season</label>
          <select className="select" value={seasonId} onChange={(e) => setSeasonId(e.target.value)}>
            <option value="">All seasons</option>
            {seasons.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name} ({s.year})
              </option>
            ))}
          </select>
        </div>
      </div>

      <form className="toolbar" onSubmit={onSubmit}>
        <div className="field" style={{ flex: '1 1 200px' }}>
          <label className="label">Event name</label>
          <input
            className="input"
            placeholder="Regional Championship"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="field" style={{ flex: '1 1 160px' }}>
          <label className="label">Location</label>
          <input
            className="input"
            placeholder="City, venue"
            value={location}
            onChange={(e) => setLocation(e.target.value)}
          />
        </div>
        <div className="field">
          <label className="label">Start</label>
          <input
            className="input mono"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
        </div>
        <div className="field">
          <label className="label">End</label>
          <input
            className="input mono"
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
          />
        </div>
        <button
          className="btn btn-primary"
          type="submit"
          disabled={!seasonId || create.isPending || !name.trim()}
          title={!seasonId ? 'Pick a season first' : undefined}
        >
          <IconPlus size={16} /> Add event
        </button>
      </form>
      {!seasonId && (
        <p className="dim" style={{ marginTop: -8, marginBottom: 14, fontSize: '0.82rem' }}>
          Select a specific season above to add an event to it.
        </p>
      )}
      {create.error && <ErrorBanner>{(create.error as Error).message}</ErrorBanner>}

      {isLoading ? (
        <Loading />
      ) : error ? (
        <ErrorBanner>{(error as Error).message}</ErrorBanner>
      ) : events.length === 0 ? (
        <div className="table-wrap">
          <EmptyState
            icon={<IconFlag size={22} />}
            title="No events"
            hint="Events belong to a season — pick one above and add it."
          />
        </div>
      ) : (
        <div className="table-wrap">
          <table className="data">
            <thead>
              <tr>
                <th>Event</th>
                <th>Dates</th>
                <th>Location</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {events.map((ev) => (
                <tr key={ev.id}>
                  <td className="cell-strong">{ev.name}</td>
                  <td className="mono muted">
                    {ev.startDate}
                    <span className="dim"> → </span>
                    {ev.endDate}
                  </td>
                  <td className="muted">{ev.location || '—'}</td>
                  <td className="actions">
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => remove.mutate(ev.id)}
                      disabled={remove.isPending}
                    >
                      <IconTrash size={15} /> Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
