import { useState, type FormEvent } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listTeams, createTeam, deleteTeam } from '../api/endpoints';
import { Loading, ErrorBanner, EmptyState, IconPlus, IconTrash, IconUsers } from '../components/ui';

const REGION_RE = /^[A-Za-z]{2}$/; // exactly 2 letters

export default function Teams() {
  const qc = useQueryClient();
  const {
    data: teams = [],
    isLoading,
    error,
  } = useQuery({ queryKey: ['teams'], queryFn: listTeams });

  const [number, setNumber] = useState(1000);
  const [name, setName] = useState('');
  const [affiliation, setAffiliation] = useState('');
  const [city, setCity] = useState('');
  const [region, setRegion] = useState('IL');
  const [formError, setFormError] = useState<string | null>(null);

  const create = useMutation({
    mutationFn: () =>
      createTeam({
        number,
        name: name.trim(),
        affiliation: affiliation.trim(),
        city: city.trim(),
        region: region.trim().toUpperCase(),
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['teams'] });
      setName('');
      setAffiliation('');
      setCity('');
      setNumber((n) => n + 1);
    },
  });
  const remove = useMutation({
    mutationFn: (id: string) => deleteTeam(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['teams'] }),
  });

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    setFormError(null);
    if (!REGION_RE.test(region.trim())) {
      setFormError('Region must be a 2-letter ISO country code (e.g. IL, US, PL)');
      return;
    }
    if (name.trim()) create.mutate();
  }

  return (
    <section className="rise">
      <div className="page-head">
        <div className="titles">
          <p className="eyebrow">Catalog</p>
          <h1>Teams</h1>
        </div>
        <span className="chip">{teams.length} registered</span>
      </div>

      <form className="toolbar" onSubmit={onSubmit}>
        <div className="field" style={{ width: 96 }}>
          <label className="label">Number</label>
          <input
            className="input mono"
            type="number"
            placeholder="1000"
            value={number}
            onChange={(e) => setNumber(Number(e.target.value))}
          />
        </div>
        <div className="field" style={{ flex: '1 1 180px' }}>
          <label className="label">Team name</label>
          <input
            className="input"
            placeholder="Robo Wizards"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="field" style={{ flex: '1 1 160px' }}>
          <label className="label">Affiliation</label>
          <input
            className="input"
            placeholder="School / club"
            value={affiliation}
            onChange={(e) => setAffiliation(e.target.value)}
          />
        </div>
        <div className="field" style={{ flex: '1 1 120px' }}>
          <label className="label">City</label>
          <input
            className="input"
            placeholder="City"
            value={city}
            onChange={(e) => setCity(e.target.value)}
          />
        </div>
        <div className="field" style={{ width: 76 }}>
          <label className="label">Region</label>
          <input
            className="input mono"
            placeholder="IL"
            value={region}
            maxLength={2}
            onChange={(e) => setRegion(e.target.value)}
          />
        </div>
        <button
          className="btn btn-primary"
          type="submit"
          disabled={create.isPending || !name.trim()}
        >
          <IconPlus size={16} /> Add team
        </button>
      </form>
      {formError && <ErrorBanner>{formError}</ErrorBanner>}
      {create.error && <ErrorBanner>{(create.error as Error).message}</ErrorBanner>}

      {isLoading ? (
        <Loading />
      ) : error ? (
        <ErrorBanner>{(error as Error).message}</ErrorBanner>
      ) : teams.length === 0 ? (
        <div className="table-wrap">
          <EmptyState
            icon={<IconUsers size={22} />}
            title="No teams yet"
            hint="Add competing teams here, then enroll them into a division on its Setup page."
          />
        </div>
      ) : (
        <div className="table-wrap">
          <table className="data">
            <thead>
              <tr>
                <th>No.</th>
                <th>Team</th>
                <th>Affiliation</th>
                <th>City</th>
                <th>Region</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {teams.map((t) => (
                <tr key={t.id}>
                  <td className="mono cell-strong" style={{ color: 'var(--primary)' }}>
                    #{t.number}
                  </td>
                  <td className="cell-strong">{t.name}</td>
                  <td className="muted">{t.affiliation || '—'}</td>
                  <td className="muted">{t.city || '—'}</td>
                  <td className="mono muted">{t.region}</td>
                  <td className="actions">
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => remove.mutate(t.id)}
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
