import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import Layout from '@/components/Layout';
import LoginPage from '@/pages/LoginPage';
import ResourcesPage from '@/pages/ResourcesPage';
import ResourceDetailsPage from '@/pages/ResourceDetailsPage';
import CreateResourcePage from '@/pages/CreateResourcePage';
import EditResourcePage from '@/pages/EditResourcePage';
import ProtectedRoute from '@/components/ProtectedRoute';

const queryClient = new QueryClient();

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Router>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<Layout />}>
              <Route path="/" element={<Navigate to="/resources" replace />} />
              <Route
                path="/resources"
                element={
                  <ProtectedRoute>
                    <ResourcesPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/resources/new"
                element={
                  <ProtectedRoute>
                    <CreateResourcePage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/resources/:id"
                element={
                  <ProtectedRoute>
                    <ResourceDetailsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/resources/:id/edit"
                element={
                  <ProtectedRoute>
                    <EditResourcePage />
                  </ProtectedRoute>
                }
              />
            </Route>
          </Routes>
        </Router>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App; 