import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    coverage: {
      provider: 'v8',
      reportsDirectory: './coverage',
      reporter: ['text', 'html'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/**/*.test.{ts,tsx}', 'src/test/**', 'src/main.tsx', 'src/api/types.ts'],
      // Modest starting floor that today's single sample test clears — a real, blocking
      // ratchet to raise as tests grow (see Copilot.md "Part 6" and docs/ai-config-log.md).
      thresholds: {
        lines: 6,
        functions: 2,
        statements: 6,
        branches: 2,
      },
    },
  },
});
