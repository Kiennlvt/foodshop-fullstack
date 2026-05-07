import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { AuthProvider, useAuth } from '../../context/AuthContext'
import * as axiosModule from '../../api/axios'

// Component helper để test context
function TestComponent() {
  const { user, isAuthenticated, isAdmin, logout } = useAuth()
  return (
    <div>
      <span data-testid="auth-status">{isAuthenticated ? 'logged-in' : 'logged-out'}</span>
      <span data-testid="username">{user?.username ?? 'none'}</span>
      <span data-testid="is-admin">{isAdmin ? 'yes' : 'no'}</span>
      <button onClick={logout}>Logout</button>
    </div>
  )
}

function renderWithAuth(ui) {
  return render(<MemoryRouter><AuthProvider>{ui}</AuthProvider></MemoryRouter>)
}

describe('AuthContext', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('starts unauthenticated when no token in localStorage', () => {
    renderWithAuth(<TestComponent />)
    expect(screen.getByTestId('auth-status')).toHaveTextContent('logged-out')
    expect(screen.getByTestId('username')).toHaveTextContent('none')
  })

  it('restores session from localStorage on mount', () => {
    const userData = { id: 1, username: 'kien', email: 'k@test.com', role: 'User', token: 'tok' }
    localStorage.setItem('user',  JSON.stringify(userData))
    localStorage.setItem('token', 'tok')

    renderWithAuth(<TestComponent />)

    expect(screen.getByTestId('auth-status')).toHaveTextContent('logged-in')
    expect(screen.getByTestId('username')).toHaveTextContent('kien')
  })

  it('login sets user and saves to localStorage', async () => {
    const mockData = { id: 1, username: 'kien', role: 'User', token: 'jwt-tok' }
    vi.spyOn(axiosModule.authApi, 'login').mockResolvedValue({ data: mockData })

    function LoginTest() {
      const { login, user } = useAuth()
      return (
        <div>
          <span data-testid="username">{user?.username ?? 'none'}</span>
          <button onClick={() => login({ email: 'k@test.com', password: '123' })}>
            Login
          </button>
        </div>
      )
    }

    renderWithAuth(<LoginTest />)
    fireEvent.click(screen.getByText('Login'))

    await waitFor(() => {
      expect(screen.getByTestId('username')).toHaveTextContent('kien')
      expect(localStorage.getItem('token')).toBe('jwt-tok')
    })
  })

  it('logout clears user and localStorage', () => {
    localStorage.setItem('user',  JSON.stringify({ id: 1, username: 'kien', role: 'User' }))
    localStorage.setItem('token', 'tok')

    renderWithAuth(<TestComponent />)
    fireEvent.click(screen.getByText('Logout'))

    expect(screen.getByTestId('auth-status')).toHaveTextContent('logged-out')
    expect(localStorage.getItem('token')).toBeNull()
    expect(localStorage.getItem('user')).toBeNull()
  })

  it('isAdmin is true when role is Admin', () => {
    localStorage.setItem('user', JSON.stringify({ id: 1, username: 'admin', role: 'Admin' }))
    renderWithAuth(<TestComponent />)
    expect(screen.getByTestId('is-admin')).toHaveTextContent('yes')
  })

  it('isAdmin is false when role is User', () => {
    localStorage.setItem('user', JSON.stringify({ id: 1, username: 'kien', role: 'User' }))
    renderWithAuth(<TestComponent />)
    expect(screen.getByTestId('is-admin')).toHaveTextContent('no')
  })
})
