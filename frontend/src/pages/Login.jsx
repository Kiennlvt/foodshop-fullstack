import { useState } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import toast from 'react-hot-toast'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = location.state?.from?.pathname || '/'

  const [form, setForm] = useState({ email: '', password: '' })
  const [loading, setLoading] = useState(false)

  const handleChange = (e) => setForm(prev => ({ ...prev, [e.target.name]: e.target.value }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      await login(form)
      toast.success('Welcome back!')
      navigate(from, { replace: true })
    } catch (err) {
      toast.error(err.response?.data?.message || 'Invalid credentials')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-cream flex">
      {/* Left panel */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-brand-500 to-brand-700 
                      p-12 flex-col justify-between relative overflow-hidden">
        <div className="absolute inset-0 opacity-10">
          {['🥦','🍎','🍞','🥩','🧀','🫐','🥕','🍋'].map((emoji, i) => (
            <span key={i} className="absolute text-6xl select-none"
              style={{
                top: `${10 + (i * 11) % 80}%`,
                left: `${5 + (i * 13) % 85}%`,
                transform: `rotate(${(i % 3 - 1) * 20}deg)`
              }}>
              {emoji}
            </span>
          ))}
        </div>
        <div className="relative">
          <div className="text-white font-display text-4xl font-bold leading-tight mb-4">
            Fresh, local,<br />
            <em>delivered fast.</em>
          </div>
          <p className="text-white/70 text-lg">
            The finest produce and artisan goods from local farms to your table.
          </p>
        </div>
        <div className="relative flex gap-4">
          {[['14+', 'Product Categories'], ['200+', 'Artisan Products'], ['4.9★', 'Customer Rating']].map(([val, label]) => (
            <div key={label} className="bg-white/10 backdrop-blur-sm rounded-2xl p-4 text-white">
              <div className="text-2xl font-bold font-display">{val}</div>
              <div className="text-white/70 text-xs mt-1">{label}</div>
            </div>
          ))}
        </div>
      </div>

      {/* Right panel */}
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="w-full max-w-md page-enter">
          <div className="mb-8">
            <Link to="/" className="text-2xl font-display font-bold text-gray-900">
              Food<span className="text-brand-500">Shop</span>
            </Link>
            <h1 className="text-3xl font-display font-semibold text-gray-900 mt-6 mb-2">
              Welcome back
            </h1>
            <p className="text-gray-500">Sign in to your account to continue shopping.</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Email</label>
              <input
                type="email" name="email" required
                value={form.email} onChange={handleChange}
                placeholder="you@example.com"
                className="input"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Password</label>
              <input
                type="password" name="password" required
                value={form.password} onChange={handleChange}
                placeholder="••••••••"
                className="input"
              />
            </div>

            {/* Demo credentials hint */}
            <div className="bg-brand-50 border border-brand-100 rounded-xl p-3 text-xs text-gray-600">
              <strong className="text-brand-600">Demo:</strong>{' '}
              admin@foodshop.com / Admin@123 &nbsp;·&nbsp; john@example.com / User@123
            </div>

            <button type="submit" disabled={loading} className="btn-primary w-full justify-center">
              {loading ? (
                <span className="flex items-center justify-center gap-2">
                  <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                  Signing in…
                </span>
              ) : 'Sign In'}
            </button>
          </form>

          <p className="text-center text-gray-500 text-sm mt-8">
            Don't have an account?{' '}
            <Link to="/register" className="text-brand-500 font-medium hover:underline">
              Create one free
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}
