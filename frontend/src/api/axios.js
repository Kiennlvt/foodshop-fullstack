import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Attach JWT to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle 401 globally
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// ── Auth ──────────────────────────────────────────────────────────────────────
export const authApi = {
  register: (data) => api.post('/auth/register', data),
  login:    (data) => api.post('/auth/login', data),
}

// ── Products ──────────────────────────────────────────────────────────────────
export const productsApi = {
  getAll:    (params) => api.get('/products', { params }),
  getById:   (id)     => api.get(`/products/${id}`),
  create:    (data)   => api.post('/products', data),
  update:    (id, data) => api.put(`/products/${id}`, data),
  delete:    (id)     => api.delete(`/products/${id}`),
}

// ── Categories ────────────────────────────────────────────────────────────────
export const categoriesApi = {
  getAll:  ()        => api.get('/categories'),
  getById: (id)      => api.get(`/categories/${id}`),
  create:  (data)    => api.post('/categories', data),
  update:  (id, data) => api.put(`/categories/${id}`, data),
  delete:  (id)      => api.delete(`/categories/${id}`),
}

// ── Cart ──────────────────────────────────────────────────────────────────────
export const cartApi = {
  getCart:    ()               => api.get('/cart'),
  addItem:    (data)           => api.post('/cart/add', data),
  updateItem: (productId, qty) => api.put(`/cart/items/${productId}`, { quantity: qty }),
  removeItem: (productId)      => api.delete(`/cart/items/${productId}`),
  clear:      ()               => api.delete('/cart/clear'),
}

// ── Orders ────────────────────────────────────────────────────────────────────
export const ordersApi = {
  placeOrder:  (data) => api.post('/orders', data),
  getMyOrders: ()     => api.get('/orders/my'),
  getById:     (id)   => api.get(`/orders/${id}`),
  getAllOrders: ()     => api.get('/orders'),
  updateStatus:(id, status) => api.patch(`/orders/${id}/status`, { status }),
}

export default api
