import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import OrderHistory from '../../pages/OrderHistory'
import * as axiosModule from '../../api/axios'

const mockOrders = [
  {
    id: 1, userId: 1, username: 'kien',
    totalPrice: 10.45, status: 'Pending',
    orderDate: '2024-07-01T10:00:00Z',
    shippingAddress: '123 Main St', notes: 'Leave at door',
    items: [
      { id: 1, productId: 1, productName: 'Apple', unitPrice: 2.99, quantity: 2, subTotal: 5.98, productImage: '' },
      { id: 2, productId: 2, productName: 'Banana', unitPrice: 1.49, quantity: 3, subTotal: 4.47, productImage: '' }
    ]
  },
  {
    id: 2, userId: 1, username: 'kien',
    totalPrice: 5.00, status: 'Completed',
    orderDate: '2024-06-28T08:00:00Z',
    items: []
  }
]

describe('OrderHistory Page', () => {
  beforeEach(() => vi.clearAllMocks())

  it('shows loading spinner initially', () => {
    vi.spyOn(axiosModule.ordersApi, 'getMyOrders').mockReturnValue(new Promise(() => {}))
    render(<MemoryRouter><OrderHistory /></MemoryRouter>)
    // Spinner exists in DOM while loading
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('renders list of orders after loading', async () => {
    vi.spyOn(axiosModule.ordersApi, 'getMyOrders').mockResolvedValue({ data: mockOrders })
    render(<MemoryRouter><OrderHistory /></MemoryRouter>)

    await waitFor(() => {
      expect(screen.getByText('Order #1')).toBeInTheDocument()
      expect(screen.getByText('Order #2')).toBeInTheDocument()
    })
  })

  it('shows order count in header', async () => {
    vi.spyOn(axiosModule.ordersApi, 'getMyOrders').mockResolvedValue({ data: mockOrders })
    render(<MemoryRouter><OrderHistory /></MemoryRouter>)

    await waitFor(() => {
      expect(screen.getByText('2 orders placed')).toBeInTheDocument()
    })
  })

  it('shows Pending and Completed status badges', async () => {
    vi.spyOn(axiosModule.ordersApi, 'getMyOrders').mockResolvedValue({ data: mockOrders })
    render(<MemoryRouter><OrderHistory /></MemoryRouter>)

    await waitFor(() => {
      expect(screen.getByText(/Pending/)).toBeInTheDocument()
      expect(screen.getByText(/Completed/)).toBeInTheDocument()
    })
  })

  it('expands order details on click', async () => {
    vi.spyOn(axiosModule.ordersApi, 'getMyOrders').mockResolvedValue({ data: mockOrders })
    render(<MemoryRouter><OrderHistory /></MemoryRouter>)

    await waitFor(() => screen.getByText('Order #1'))
    fireEvent.click(screen.getByText('Order #1'))

    await waitFor(() => {
      expect(screen.getByText('Apple')).toBeInTheDocument()
      expect(screen.getByText('Banana')).toBeInTheDocument()
      expect(screen.getByText('123 Main St')).toBeInTheDocument()
    })
  })

  it('shows empty state when no orders', async () => {
    vi.spyOn(axiosModule.ordersApi, 'getMyOrders').mockResolvedValue({ data: [] })
    render(<MemoryRouter><OrderHistory /></MemoryRouter>)

    await waitFor(() => {
      expect(screen.getByText('No orders yet')).toBeInTheDocument()
    })
  })
})
