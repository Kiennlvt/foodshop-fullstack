import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import Login from '../../pages/Login'
import * as authCtx from '../../context/AuthContext'
import * as cartCtx from '../../context/CartContext'

// Mock navigate
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

function renderLogin(loginFn = vi.fn()) {
  vi.spyOn(authCtx, 'useAuth').mockReturnValue({
    login: loginFn, isAuthenticated: false, user: null,
    isAdmin: false, logout: vi.fn(), register: vi.fn()
  })
  vi.spyOn(cartCtx, 'useCart').mockReturnValue({
    cart: null, loading: false, itemCount: 0,
    fetchCart: vi.fn(), addToCart: vi.fn(),
    updateItem: vi.fn(), removeItem: vi.fn(), clearCart: vi.fn()
  })
  return render(<MemoryRouter><Login /></MemoryRouter>)
}

describe('Login Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('renders email and password inputs', () => {
    renderLogin()
    expect(screen.getByPlaceholderText('you@example.com')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('••••••••')).toBeInTheDocument()
  })

  it('renders Sign In button', () => {
    renderLogin()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('shows link to register page', () => {
    renderLogin()
    expect(screen.getByText(/create one free/i)).toBeInTheDocument()
  })

  it('calls login with correct data on submit', async () => {
    const loginFn = vi.fn().mockResolvedValue({})
    renderLogin(loginFn)

    await userEvent.type(screen.getByPlaceholderText('you@example.com'), 'kien@test.com')
    await userEvent.type(screen.getByPlaceholderText('••••••••'), 'Password123')
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(loginFn).toHaveBeenCalledWith({
        email: 'kien@test.com',
        password: 'Password123'
      })
    })
  })

  it('navigates to home after successful login', async () => {
    const loginFn = vi.fn().mockResolvedValue({ username: 'kien' })
    renderLogin(loginFn)

    await userEvent.type(screen.getByPlaceholderText('you@example.com'), 'kien@test.com')
    await userEvent.type(screen.getByPlaceholderText('••••••••'), 'pass')
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/', { replace: true })
    })
  })

  it('does not navigate when login fails', async () => {
    const loginFn = vi.fn().mockRejectedValue({
      response: { data: { message: 'Invalid credentials' } }
    })
    renderLogin(loginFn)

    await userEvent.type(screen.getByPlaceholderText('you@example.com'), 'bad@test.com')
    await userEvent.type(screen.getByPlaceholderText('••••••••'), 'wrongpass')
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(mockNavigate).not.toHaveBeenCalled()
    })
  })
})
