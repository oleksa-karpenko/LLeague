import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { ErrorBanner, EmptyState, MatchStatusBadge } from './ui';

// Sample test proving the Vitest + React Testing Library harness works.
// Query by role/accessible text and assert on user-visible output — see
// .github/instructions/frontend-tests.instructions.md.
describe('ui components', () => {
  it('ErrorBanner renders its message in an alert region', () => {
    render(<ErrorBanner>Something went wrong</ErrorBanner>);
    const alert = screen.getByRole('alert');
    expect(alert).toHaveTextContent('Something went wrong');
  });

  it('EmptyState shows the title and optional hint', () => {
    render(<EmptyState title="No teams yet" hint="Add your first team" />);
    expect(screen.getByRole('heading', { name: 'No teams yet' })).toBeInTheDocument();
    expect(screen.getByText('Add your first team')).toBeInTheDocument();
  });

  it('MatchStatusBadge maps status to a human label', () => {
    render(<MatchStatusBadge status="InProgress" />);
    expect(screen.getByText('Live')).toBeInTheDocument();
  });
});
