export interface Season {
  id: string;
  name: string;
  year: number;
}

export interface Event {
  id: string;
  seasonId: string;
  name: string;
  slug: string;
  startDate: string;
  endDate: string;
  location: string;
}

export interface Division {
  id: string;
  eventId: string;
  name: string;
  color: string;
}

export interface Team {
  id: string;
  number: number;
  name: string;
  affiliation: string;
  city: string;
  region: string;
}

export type MatchStage = 'Practice' | 'Ranking' | 'Test';
export type MatchStatus = 'NotStarted' | 'InProgress' | 'Completed';
export type ScoresheetStatus = 'Empty' | 'Draft' | 'Completed';
export type ClauseType = 'boolean' | 'number' | 'enum';

export interface EnrolledTeam {
  teamId: string;
  number: number;
  name: string;
  affiliation: string;
  city: string;
  region: string;
  arrived: boolean;
  arrivedAt: string | null;
}
export interface Table {
  id: string;
  name: string;
}

export interface MatchParticipant {
  id: string;
  tableId: string;
  tableName: string;
  teamId: string;
  teamNumber: number;
  teamName: string;
  ready: boolean;
  scoresheetStatus: ScoresheetStatus;
  score: number;
}
export interface Match {
  id: string;
  round: number;
  number: number;
  stage: MatchStage;
  status: MatchStatus;
  scheduledTime: string | null;
  startTime: string | null;
  participants: MatchParticipant[];
}

export interface ClauseOption {
  label: string;
  value: string;
  points: number;
}
export interface MissionClause {
  index: number;
  type: ClauseType;
  label: string;
  points?: number | null;
  perUnit?: number | null;
  max?: number | null;
  options?: ClauseOption[] | null;
}
export interface Mission {
  missionId: string;
  title: string;
  clauses: MissionClause[];
}

export interface MissionValue {
  missionId: string;
  clauseIndex: number;
  valueType: string;
  value: string | null;
}
export interface Scoresheet {
  participantId: string;
  status: ScoresheetStatus;
  score: number;
  missions: MissionValue[];
}

export interface StandingRow {
  rank: number;
  teamId: string;
  teamNumber: number;
  teamName: string;
  bestScore: number;
  totalScore: number;
  matchesPlayed: number;
}
