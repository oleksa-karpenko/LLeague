import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listEvents, listDivisions, createDivision, deleteDivision } from '../api/endpoints';
import { Loading, ErrorBanner, EmptyState, IconPlus, IconTrash, IconLayers, IconWrench, IconClipboard, IconFlag } from '../components/ui';

export default function Divisions() {
  const qc = useQueryClient();
  const { data: events = [] } = useQuery({ queryKey: ['events', ''], queryFn: () => listEvents() });
  const [eventId, setEventId] = useState('');

  const { data: divisions = [], isLoading, error } = useQuery({
    queryKey: ['divisions', eventId],
    queryFn: () => listDivisions(eventId || undefined),
  });

  const [name, setName] = useState('');
  const [color, setColor] = useState('#ffb020');

  const create = useMutation({
    mutationFn: () => createDivision({ eventId, name: name.trim(), color }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['divisions', eventId] }); setName(''); },
  });
  const remove = useMutation({
    mutationFn: (id: string) => deleteDivision(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['divisions', eventId] }),
  });

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (!eventId) return;
    if (name.trim()) create.mutate();
  }

  return (
    <section className="rise">
      <div className="page-head">
        <div className="titles">
          <p className="eyebrow">Catalog</p>
          <h1>Divisions</h1>
        </div>
        <div className="field" style={{ minWidth: 220 }}>
          <label className="label">Filter by event</label>
          <select className="select" value={eventId} onChange={(e) => setEventId(e.target.value)}>
            <option value="">All events</option>
            {events.map((ev) => <option key={ev.id} value={ev.id}>{ev.name}</option>)}
          </select>
        </div>
      </div>

      <form className="toolbar" onSubmit={onSubmit}>
        <div className="field" style={{ flex: '1 1 220px' }}>
          <label className="label">Division name</label>
          <input className="input" placeholder="e.g. Red Division" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="field">
          <label className="label">Color</label>
          <input className="input" type="color" value={color} onChange={(e) => setColor(e.target.value)} title="Division color" />
        </div>
        <button className="btn btn-primary" type="submit" disabled={!eventId || create.isPending || !name.trim()} title={!eventId ? 'Pick an event first' : undefined}>
          <IconPlus size={16} /> Add division
        </button>
      </form>
      {!eventId && <p className="dim" style={{ marginTop: -8, marginBottom: 14, fontSize: '0.82rem' }}>Select a specific event above to add a division to it.</p>}
      {create.error && <ErrorBanner>{(create.error as Error).message}</ErrorBanner>}

      {isLoading ? <Loading />
        : error ? <ErrorBanner>{(error as Error).message}</ErrorBanner>
        : divisions.length === 0 ? (
          <div className="table-wrap"><EmptyState icon={<IconLayers size={22} />} title="No divisions" hint="Divisions group teams, tables and matches inside an event." /></div>
        ) : (
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Division</th><th>Color</th><th>Operations</th><th /></tr></thead>
              <tbody>
                {divisions.map((d) => (
                  <tr key={d.id}>
                    <td>
                      <span style={{ display: 'inline-flex', alignItems: 'center', gap: 9 }}>
                        <span className="swatch" style={{ background: d.color, width: 16, height: 16 }} />
                        <span className="cell-strong">{d.name}</span>
                      </span>
                    </td>
                    <td className="mono muted">{d.color}</td>
                    <td>
                      <div className="row" style={{ gap: 6 }}>
                        <Link className="btn btn-sm" to={`/division/${d.id}/setup`}><IconWrench size={14} /> Setup</Link>
                        <Link className="btn btn-sm" to={`/division/${d.id}/score`}><IconClipboard size={14} /> Score</Link>
                        <Link className="btn btn-sm" to={`/division/${d.id}/board`}><IconFlag size={14} /> Board</Link>
                      </div>
                    </td>
                    <td className="actions">
                      <button className="btn btn-danger btn-sm" onClick={() => remove.mutate(d.id)} disabled={remove.isPending}>
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
