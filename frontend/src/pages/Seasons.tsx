import { useState, type FormEvent } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listSeasons, createSeason, deleteSeason } from '../api/endpoints';
import {
  Loading,
  ErrorBanner,
  EmptyState,
  IconPlus,
  IconTrash,
  IconCalendar,
} from '../components/ui';

export default function Seasons() {
  const qc = useQueryClient();
  const {
    data: seasons = [],
    isLoading,
    error,
  } = useQuery({ queryKey: ['seasons'], queryFn: listSeasons });

  const [name, setName] = useState('');
  const [year, setYear] = useState(2026);

  const create = useMutation({
    mutationFn: () => createSeason({ name: name.trim(), year }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['seasons'] });
      setName('');
    },
  });
  const remove = useMutation({
    mutationFn: (id: string) => deleteSeason(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['seasons'] }),
  });

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (name.trim()) create.mutate();
  }

  return (
    <section className="rise">
      <div className="page-head">
        <div className="titles">
          <p className="eyebrow">Catalog</p>
          <h1>Seasons</h1>
        </div>
        <span className="chip">{seasons.length} total</span>
      </div>

      <form className="toolbar" onSubmit={onSubmit}>
        <div className="field" style={{ flex: '1 1 220px' }}>
          <label className="label">Season name</label>
          <input
            className="input"
            placeholder="e.g. SUBMERGED"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="field" style={{ width: 110 }}>
          <label className="label">Year</label>
          <input
            className="input mono"
            type="number"
            value={year}
            onChange={(e) => setYear(Number(e.target.value))}
          />
        </div>
        <button
          className="btn btn-primary"
          type="submit"
          disabled={create.isPending || !name.trim()}
        >
          <IconPlus size={16} /> Add season
        </button>
      </form>
      {create.error && <ErrorBanner>{(create.error as Error).message}</ErrorBanner>}

      {isLoading ? (
        <Loading />
      ) : error ? (
        <ErrorBanner>{(error as Error).message}</ErrorBanner>
      ) : seasons.length === 0 ? (
        <div className="table-wrap">
          <EmptyState
            icon={<IconCalendar size={22} />}
            title="No seasons yet"
            hint="Create your first season to start organizing events."
          />
        </div>
      ) : (
        <div className="table-wrap">
          <table className="data">
            <thead>
              <tr>
                <th>Season</th>
                <th>Year</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {seasons.map((s) => (
                <tr key={s.id}>
                  <td className="cell-strong">{s.name}</td>
                  <td className="mono muted">{s.year}</td>
                  <td className="actions">
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => remove.mutate(s.id)}
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
