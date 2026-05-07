import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import toast from 'react-hot-toast'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()

  const [form, setForm] = useState({ username: '', email: '', password: '', confirmPassword: '' })
  const [loading, setLoading] = useState(false)

  const handleChange = (e) => setForm(prev => ({ ...prev, [e.target.name]: e.target.value }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (form.password !== form.confirmPassword) {
      toast.error('Passwords do not match')
      return
    }
    if (form.password.length < 6) {
      toast.error('Password must be at least 6 characters')
      return
    }
    setLoading(true)
    try {
      await register({ username: form.username, email: form.email, password: form.password })
      toast.success('Account created! Welcome to FoodShop 🎉')
      navigate('/')
    } catch (err) {
      toast.error(err.response?.data?.message || 'Registration failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-cream flex items-center justify-center p-8">
      <div className="w-full max-w-md page-enter">
        <div className="text-center mb-8">
          <Link to="/" className="text-2xl font-display font-bold text-gray-900">
            Food<span className="text-brand-500">Shop</span>
          </Link>
          <h1 className="text-3xl font-display font-semibold text-gray-900 mt-6 mb-2">
            Create your account
          </h1>
          <p className="text-gray-500">Join thousands of happy food lovers.</p>
        </div>

        <div className="card p-8">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Username</label>
              <input
                type="text" name="username" required
                value={form.username} onChange={handleChange}
                placeholder="johndoe"
                className="input"
                minLength={3}
              />
            </div>
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
                placeholder="Min. 6 characters"
                className="input"
                minLength={6}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Confirm Password</label>
              <input
                type="password" name="confirmPassword" required
                value={form.confirmPassword} onChange={handleChange}
                placeholder="Repeat password"
                className="input"
              />
            </div>

            <button type="submit" disabled={loading} className="btn-primary w-full mt-2 justify-center">
              {loading ? (
                <span className="flex items-center justify-center gap-2">
                  <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                  Creating account…
                </span>
              ) : 'Create Account'}
            </button>
          </form>
        </div>

        <p className="text-center text-gray-500 text-sm mt-6">
          Already have an account?{' '}
          <Link to="/login" className="text-brand-500 font-medium hover:underline">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  )
}
