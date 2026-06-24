// Root lint-staged config. Hooks run from the git root, but each stack's tooling lives in its
// own folder. lint-staged passes ABSOLUTE file paths, so we `cd` into the stack (for ESLint /
// Prettier config resolution) and use `npx` to find that stack's local binaries.
//
// - Frontend TS/TSX: ESLint --fix + Prettier --write.
// - Frontend assets (css/json/md/html): Prettier --write only (ESLint only handles TS).
// - Backend C#: whitespace-only `dotnet format` (matches the CI gate; analyzer/style correctness
//   is enforced separately at build time via TreatWarningsAsErrors).
export default {
  'frontend/**/*.{ts,tsx}': (files) => {
    const list = files.join(' ');
    return [`bash -c 'cd frontend && npx eslint --fix ${list} && npx prettier --write ${list}'`];
  },
  'frontend/**/*.{css,json,md,html}': (files) =>
    `bash -c 'cd frontend && npx prettier --write ${files.join(' ')}'`,
  'backend/**/*.cs': (files) =>
    `dotnet format whitespace backend/LLeague.slnx --include ${files.join(' ')}`,
};
