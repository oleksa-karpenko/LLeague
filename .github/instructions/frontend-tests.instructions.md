---
applyTo: "frontend/**/*.{test,spec}.{ts,tsx}"
---

# Frontend tests (Vitest + React Testing Library)

Test runner is **Vitest** (jsdom env, globals on) with **React Testing Library**. Setup lives in
`src/test/setup.ts` (imports `@testing-library/jest-dom`). Colocate tests next to the code as
`Name.test.tsx` / `name.test.ts`.

## Conventions

- **Query by accessible role/text**, not test IDs or class names:
  `screen.getByRole('button', { name: /sign in/i })`, `getByLabelText`, `findByText`.
- **Simulate users with `@testing-library/user-event`**, not raw `fireEvent`, for clicks/typing.
- **Assert on behavior the user sees**, not implementation details (no reaching into state/props).
- Mock the network at the `src/api/` boundary (mock `endpoints.ts` functions) rather than `fetch`.
- For components using TanStack Query, render inside a fresh `QueryClientProvider` per test.

## Example

```tsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';

describe('Login', () => {
  it('shows an error when login fails', async () => {
    // arrange: render component (wrap with providers as needed), mock endpoints
    // act: await userEvent.type(...) / userEvent.click(...)
    // assert: expect(await screen.findByText(/login failed/i)).toBeInTheDocument();
  });
});
```

## Running

```bash
cd frontend
npm run test            # one-shot
npm run test:watch      # watch mode
npm run test:coverage   # with coverage (CI gate)
```
