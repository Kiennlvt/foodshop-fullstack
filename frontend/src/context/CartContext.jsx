import { createContext, useContext, useState, useCallback, useEffect } from 'react'
import { cartApi } from '../api/axios'
import { useAuth } from './AuthContext'

const CartContext = createContext(null)

export function CartProvider({ children }) {
  const { isAuthenticated } = useAuth()
  const [cart, setCart] = useState(null)
  const [loading, setLoading] = useState(false)

  const fetchCart = useCallback(async () => {
    if (!isAuthenticated) return
    try {
      setLoading(true)
      const { data } = await cartApi.getCart()
      setCart(data)
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated])

  useEffect(() => {
    fetchCart()
  }, [fetchCart])

  const addToCart = useCallback(async (productId, quantity = 1) => {
    const { data } = await cartApi.addItem({ productId, quantity })
    setCart(data)
    return data
  }, [])

  const updateItem = useCallback(async (productId, quantity) => {
    const { data } = await cartApi.updateItem(productId, quantity)
    setCart(data)
    return data
  }, [])

  const removeItem = useCallback(async (productId) => {
    const { data } = await cartApi.removeItem(productId)
    setCart(data)
    return data
  }, [])

  const clearCart = useCallback(async () => {
    await cartApi.clear()
    setCart(prev => prev ? { ...prev, items: [], totalPrice: 0, totalItems: 0 } : null)
  }, [])

  const itemCount = cart?.totalItems ?? 0

  return (
    <CartContext.Provider value={{
      cart, loading, itemCount,
      fetchCart, addToCart, updateItem, removeItem, clearCart
    }}>
      {children}
    </CartContext.Provider>
  )
}

export const useCart = () => {
  const ctx = useContext(CartContext)
  if (!ctx) throw new Error('useCart must be used within CartProvider')
  return ctx
}
