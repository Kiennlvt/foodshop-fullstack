import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function PrivateRoute({ children, adminOnly = false }) {
  const { isAuthenticated, isAdmin } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }
  if (adminOnly && !isAdmin) {
    return <Navigate to="/" replace />
  }
  return children
}

// ── Loading Spinner ───────────────────────────────────────────────────────────
export function Spinner({ size = 'md' }) {
  const sizes = { sm: 'w-4 h-4', md: 'w-8 h-8', lg: 'w-12 h-12' }
  return (
    <div className={`${sizes[size]} border-2 border-brand-200 border-t-brand-500 rounded-full animate-spin`} />
  )
}

// ── Page Wrapper ──────────────────────────────────────────────────────────────
export function PageWrapper({ children, className = '' }) {
  return (
    <main className={`min-h-[calc(100vh-64px)] max-w-7xl mx-auto px-4 sm:px-6 py-8 page-enter ${className}`}>
      {children}
    </main>
  )
}

// ── Empty State ───────────────────────────────────────────────────────────────
export function EmptyState({ icon, title, description, action }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center">
      <div className="text-6xl mb-4">{icon}</div>
      <h3 className="text-xl font-display font-semibold text-gray-800 mb-2">{title}</h3>
      <p className="text-gray-500 mb-6 max-w-md">{description}</p>
      {action}
    </div>
  )
}

// ── Order Status Badge ────────────────────────────────────────────────────────
export function StatusBadge({ status }) {
  const styles = {
    Pending:    'bg-yellow-100 text-yellow-700',
    Processing: 'bg-blue-100 text-blue-700',
    Completed:  'bg-green-100 text-green-700',
    Cancelled:  'bg-red-100 text-red-700',
  }
  const icons = {
    Pending:    '⏳',
    Processing: '🔄',
    Completed:  '✅',
    Cancelled:  '❌',
  }
  return (
    <span className={`badge ${styles[status] ?? 'bg-gray-100 text-gray-700'} py-1 px-3`}>
      {icons[status]} {status}
    </span>
  )
}
