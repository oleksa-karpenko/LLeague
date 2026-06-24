import { Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Layout from './components/Layout';
import Seasons from './pages/Seasons';
import Events from './pages/Events';
import Divisions from './pages/Divisions';
import Teams from './pages/Teams';
import { ProtectedRoute } from './auth/ProtectedRoute';
import Setup from './pages/Setup';
import Score from './pages/Score';
import Board from './pages/Board';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        path="/admin"
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Navigate to="seasons" replace />} />
        <Route path="seasons" element={<Seasons />} />
        <Route path="events" element={<Events />} />
        <Route path="divisions" element={<Divisions />} />
        <Route path="teams" element={<Teams />} />
      </Route>
      <Route
        path="/division/:divisionId/setup"
        element={
          <ProtectedRoute>
            <Setup />
          </ProtectedRoute>
        }
      />
      <Route
        path="/division/:divisionId/score"
        element={
          <ProtectedRoute>
            <Score />
          </ProtectedRoute>
        }
      />
      <Route
        path="/division/:divisionId/board"
        element={
          <ProtectedRoute>
            <Board />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/admin" replace />} />
    </Routes>
  );
}
