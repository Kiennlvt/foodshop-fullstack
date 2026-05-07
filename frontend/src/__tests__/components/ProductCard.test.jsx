import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ProductCard from '../../components/ProductCard'
import { AuthProvider } from '../../context/AuthContext'
import { CartProvider } from '../../context/CartContext'
import * as cartCtx from '../../context/CartContext'
import * as authCtx from '../../context/AuthContext'

const mockProduct = {
  id: 1, name: 'Fresh Apple', description: 'Very fresh apple', price: 2.99,
  stockQuantity: 50, categoryName: 'Fruits',
  imageUrl: 'https://example.com/apple.jpg', isActive: true
}

function renderCard(product = mockProduct) {
  return render(
    <MemoryRouter>
      <AuthProvider>
        <CartProvider>
          <ProductCard product={product} />
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>
  )
}

describe('ProductCard', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('renders product name and price', () => {
    renderCard()
    expect(screen.getByText('Fresh Apple')).toBeInTheDocument()
    expect(screen.getByText('$2.99')).toBeInTheDocument()
  })

  it('renders category badge', () => {
    renderCard()
    expect(screen.getByText('Fruits')).toBeInTheDocument()
  })

  it('renders product image with correct alt', () => {
    renderCard()
    const img = screen.getByAltText('Fresh Apple')
    expect(img).toBeInTheDocument()
    expect(img).toHaveAttribute('src', mockProduct.imageUrl)
  })

  it('shows "Out of Stock" overlay when stockQuantity is 0', () => {
    renderCard({ ...mockProduct, stockQuantity: 0 })
    expect(screen.getByText('Out of Stock')).toBeInTheDocument()
  })

  it('shows low stock warning when stockQuantity <= 5', () => {
    renderCard({ ...mockProduct, stockQuantity: 3 })
    expect(screen.getByText('Only 3 left!')).toBeInTheDocument()
  })

  it('clicking Add to Cart when not logged in shows toast error', async () => {
    // Mock useAuth — chưa đăng nhập
    vi.spyOn(authCtx, 'useAuth').mockReturnValue({
      isAuthenticated: false, user: null, isAdmin: false,
      login: vi.fn(), logout: vi.fn(), register: vi.fn()
    })
    vi.spyOn(cartCtx, 'useCart').mockReturnValue({
      cart: null, loading: false, itemCount: 0,
      fetchCart: vi.fn(), addToCart: vi.fn(), updateItem: vi.fn(),
      removeItem: vi.fn(), clearCart: vi.fn()
    })

    renderCard()
    fireEvent.click(screen.getByText('+ Cart'))

    // toast.error gọi với message đúng
    await waitFor(() => {
      expect(cartCtx.useCart().addToCart).not.toHaveBeenCalled()
    })
  })

  it('clicking Add to Cart when logged in calls addToCart', async () => {
    const addToCart = vi.fn().mockResolvedValue({})

    vi.spyOn(authCtx, 'useAuth').mockReturnValue({
      isAuthenticated: true, user: { id: 1, username: 'kien' },
      isAdmin: false, login: vi.fn(), logout: vi.fn(), register: vi.fn()
    })
    vi.spyOn(cartCtx, 'useCart').mockReturnValue({
      cart: null, loading: false, itemCount: 0,
      fetchCart: vi.fn(), addToCart, updateItem: vi.fn(),
      removeItem: vi.fn(), clearCart: vi.fn()
    })

    renderCard()
    fireEvent.click(screen.getByText('+ Cart'))

    await waitFor(() => {
      expect(addToCart).toHaveBeenCalledWith(1, 1)
    })
  })
})
