import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, ProtectedRoute, useAuth } from './context/AuthContext'
import { ThemeProvider } from './context/ThemeContext'
import { AppShell } from './layouts/AppShell'
import { DashboardPage } from './pages/DashboardPage'
import { EscoposPage } from './pages/EscoposPage'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage'
import { LoginPage } from './pages/LoginPage'
import { ParceirosPage } from './pages/ParceirosPage'
import { PerfisPage } from './pages/PerfisPage'
import { ResetPasswordPage } from './pages/ResetPasswordPage'
import { UsuariosPage } from './pages/UsuariosPage'

function HomeRedirect() {
  const { isAuthenticated } = useAuth()

  return <Navigate to={isAuthenticated ? '/app' : '/login'} replace />
}

function App() {
  return (
    <ThemeProvider>
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/" element={<HomeRedirect />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/auth/esqueceu-senha" element={<ForgotPasswordPage />} />
            <Route path="/auth/trocar-senha" element={<ResetPasswordPage />} />
            <Route
              path="/app"
              element={
                <ProtectedRoute>
                  <AppShell />
                </ProtectedRoute>
              }
            >
              <Route index element={<DashboardPage />} />
              <Route
                path="escopos"
                element={
                  <ProtectedRoute requireMaster>
                    <EscoposPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="parceiros"
                element={
                  <ProtectedRoute requireMaster>
                    <ParceirosPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="perfis"
                element={
                  <ProtectedRoute requireMaster>
                    <PerfisPage />
                  </ProtectedRoute>
                }
              />
              <Route path="usuarios" element={<UsuariosPage />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  )
}

export default App
