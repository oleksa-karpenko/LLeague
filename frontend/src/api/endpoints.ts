import { api, setToken, clearToken } from './client';
import type { Season, Event, Division, Team } from './types';

import type {
  EnrolledTeam, Table, Match, Mission, Scoresheet, MissionValue, StandingRow
} from './types';

// division sub-resources
export const listEnrolledTeams = (divisionId: string) => api<EnrolledTeam[]>(`/divisions/${divisionId}/teams`);
export const enrollTeam   = (divisionId: string, teamId: string) =>
  api<void>(`/divisions/${divisionId}/teams`, { method: 'POST', body: JSON.stringify({ teamId }) });
export const markArrived  = (divisionId: string, teamId: string) =>
  api<void>(`/divisions/${divisionId}/teams/${teamId}/arrive`, { method: 'POST' });
export const listTables   = (divisionId: string) => api<Table[]>(`/divisions/${divisionId}/tables`);
export const createTable  = (divisionId: string, name: string) =>
  api<Table>(`/divisions/${divisionId}/tables`, { method: 'POST', body: JSON.stringify({ name }) });

// matches
export const listMatches  = (divisionId: string) => api<Match[]>(`/divisions/${divisionId}/matches`);
export const createMatch  = (divisionId: string, body: { round: number; number: number; stage: string; participants: { tableId: string; teamId: string }[] }) =>
  api<Match>(`/divisions/${divisionId}/matches`, { method: 'POST', body: JSON.stringify(body) });
export const startMatch    = (id: string) => api<void>(`/matches/${id}/start`, { method: 'POST' });
export const completeMatch = (id: string) => api<void>(`/matches/${id}/complete`, { method: 'POST' });
export const abortMatch    = (id: string) => api<void>(`/matches/${id}/abort`, { method: 'POST' });

// scoresheets
export const getMissionCatalog = () => api<Mission[]>('/missions/catalog');
export const getScoresheet     = (participantId: string) => api<Scoresheet>(`/scoresheets/${participantId}`);
export const upsertScoresheet  = (participantId: string, missions: MissionValue[]) =>
  api<Scoresheet>(`/scoresheets/${participantId}`, { method: 'PUT', body: JSON.stringify({ missions }) });
export const submitScoresheet  = (participantId: string) =>
  api<Scoresheet>(`/scoresheets/${participantId}/submit`, { method: 'POST' });

// standings
export const getStandings = (divisionId: string) => api<StandingRow[]>(`/divisions/${divisionId}/standings`);

export async function login(username: string, password: string): Promise<void> {
  const res = await api<{ token: string }>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });

  setToken(res.token);
}

export function logout(): void {
  clearToken();
}

// --- Seasons ---

export const listSeasons = () => api<Season[]>('/seasons');

export const createSeason = (body: { name: string; year: number }) =>
  api<Season>('/seasons', {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const updateSeason = (
  id: string,
  body: { name: string; year: number },
) =>
  api<Season>(`/seasons/${id}`, {
    method: 'PUT',
    body: JSON.stringify(body),
  });

export const deleteSeason = (id: string) =>
  api<void>(`/seasons/${id}`, {
    method: 'DELETE',
  });

// --- Events ---

export const listEvents = (seasonId?: string) =>
  api<Event[]>(`/events${seasonId ? `?seasonId=${seasonId}` : ''}`);

export const createEvent = (body: Omit<Event, 'id'>) =>
  api<Event>('/events', {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const deleteEvent = (id: string) =>
  api<void>(`/events/${id}`, {
    method: 'DELETE',
  });

// --- Divisions ---

export const listDivisions = (eventId?: string) =>
  api<Division[]>(`/divisions${eventId ? `?eventId=${eventId}` : ''}`);

export const createDivision = (body: Omit<Division, 'id'>) =>
  api<Division>('/divisions', {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const deleteDivision = (id: string) =>
  api<void>(`/divisions/${id}`, {
    method: 'DELETE',
  });

// --- Teams ---

export const listTeams = () => api<Team[]>('/teams');

export const createTeam = (body: Omit<Team, 'id'>) =>
  api<Team>('/teams', {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const deleteTeam = (id: string) =>
  api<void>(`/teams/${id}`, {
    method: 'DELETE',
  });