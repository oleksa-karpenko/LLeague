import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api/endpoints';
import { ApiError } from '../api/client';
import { ErrorBanner, IconLock, Spinner } from '../components/ui';

export default function Login() {
  const navigate = useNavigate();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      await login(username, password);
      navigate('/admin');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Login failed');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ minHeight: '100svh', display: 'grid', placeItems: 'center', padding: 24 }}>
      <div className="rise" style={{ width: '100%', maxWidth: 400 }}>
        <div
          style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 14,
            marginBottom: 26,
          }}
        >
          <span className="brand-mark" style={{ width: 56, height: 56, borderRadius: 16 }}>
            <svg
              width="30"
              height="30"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.4"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M7 6v12h5M14 6v12h5" />
            </svg>
          </span>
          <div style={{ textAlign: 'center' }}>
            <h1 style={{ fontSize: '1.7rem' }}>LLeague</h1>
            <p className="eyebrow" style={{ marginTop: 6 }}>
              Tournament Control
            </p>
          </div>
        </div>

        <form className="card card-pad stack" onSubmit={onSubmit}>
          <div className="field">
            <label className="label" htmlFor="u">
              Username
            </label>
            <input
              id="u"
              className="input"
              value={username}
              autoComplete="username"
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>
          <div className="field">
            <label className="label" htmlFor="p">
              Password
            </label>
            <input
              id="p"
              className="input"
              type="password"
              value={password}
              autoComplete="current-password"
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
          {error && <ErrorBanner>{error}</ErrorBanner>}
          <button className="btn btn-primary btn-lg btn-block" type="submit" disabled={busy}>
            {busy ? (
              <>
                <Spinner /> Signing in…
              </>
            ) : (
              <>
                <IconLock size={17} /> Sign in
              </>
            )}
          </button>
        </form>

        <p className="dim" style={{ textAlign: 'center', marginTop: 16, fontSize: '0.8rem' }}>
          Referees & organizers · authorized access only
        </p>
      </div>
    </div>
  );
}
