import { http, HttpResponse } from 'msw'

// ── Mock data ─────────────────────────────────────────────────────────────────
export const mockUser = {
  id: 1, username: 'kien', email: 'kien@test.com',
  role: 'User', token: 'mock-jwt-token-abc123'
}

export const mockProduct = {
  id: 1, name: 'Apple', description: 'Fresh apple', price: 2.99,
  stockQuantity: 50, categoryId: 1, categoryName: 'Fruits',
  imageUrl: 'https://example.com/apple.jpg', isActive: true
}

export const mockCart = {
  id: 1, userId: 1, totalPrice: 5.98, totalItems: 2,
  items: [{
    id: 1, productId: 1, productName: 'Apple',
    unitPrice: 2.99, quantity: 2, subTotal: 5.98,
    productImage: 'https://example.com/apple.jpg', availableStock: 50
  }]
}

export const mockOrder = {
  id: 1, userId: 1, username: 'kien',
  totalPrice: 5.98, status: 'Pending',
  orderDate: new Date().toISOString(),
  items: [{ id: 1, productId: 1, productName: 'Apple', quantity: 2, unitPrice: 2.99, subTotal: 5.98 }]
}

// ── Handlers ──────────────────────────────────────────────────────────────────
export const handlers = [
  // Auth
  http.post('/api/auth/login', () => HttpResponse.json(mockUser)),
  http.post('/api/auth/register', () => HttpResponse.json(mockUser)),

  // Products
  http.get('/api/products', () => HttpResponse.json({
    items: [mockProduct], totalCount: 1, page: 1, pageSize: 12, totalPages: 1
  })),
  http.get('/api/products/:id', () => HttpResponse.json(mockProduct)),

  // Categories
  http.get('/api/categories', () => HttpResponse.json([
    { id: 1, name: 'Fruits', productCount: 3 },
    { id: 2, name: 'Bakery', productCount: 2 }
  ])),

  // Cart
  http.get('/api/cart',      () => HttpResponse.json(mockCart)),
  http.post('/api/cart/add', () => HttpResponse.json(mockCart)),
  http.delete('/api/cart/items/:productId', () => HttpResponse.json({
    ...mockCart, items: [], totalItems: 0, totalPrice: 0
  })),

  // Orders
  http.get('/api/orders/my', () => HttpResponse.json([mockOrder])),
  http.post('/api/orders',   () => HttpResponse.json(mockOrder)),
]
