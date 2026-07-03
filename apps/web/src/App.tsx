import { Routes, Route } from 'react-router-dom';
import { AuthProvider } from '@/hooks/useAuth';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { AppLayout } from '@/components/layout/AppLayout';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from '@/pages/RegisterPage';
import DashboardPage from '@/pages/DashboardPage';
import WorkflowsPage from '@/pages/WorkflowsPage';
import WorkflowDetailPage from '@/pages/WorkflowDetailPage';
import ProvidersPage from '@/pages/ProvidersPage';
import DocumentsPage from '@/pages/DocumentsPage';

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/" element={<ProtectedRoute><AppLayout><DashboardPage /></AppLayout></ProtectedRoute>} />
        <Route path="/dashboard" element={<ProtectedRoute><AppLayout><DashboardPage /></AppLayout></ProtectedRoute>} />
        <Route path="/workflows" element={<ProtectedRoute><AppLayout><WorkflowsPage /></AppLayout></ProtectedRoute>} />
        <Route path="/workflows/:id" element={<ProtectedRoute><AppLayout><WorkflowDetailPage /></AppLayout></ProtectedRoute>} />
        <Route path="/providers" element={<ProtectedRoute><AppLayout><ProvidersPage /></AppLayout></ProtectedRoute>} />
        <Route path="/documents" element={<ProtectedRoute><AppLayout><DocumentsPage /></AppLayout></ProtectedRoute>} />
      </Routes>
    </AuthProvider>
  );
}
