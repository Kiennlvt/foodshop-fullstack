import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { ordersApi } from '../api/axios'
import { PageWrapper, EmptyState, Spinner } from '../components/index.jsx'
import toast from 'react-hot-toast'

export default function Cart() {
  const { cart, loading, updateItem, removeItem } = useCart()
  const navigate = useNavigate()
  const [placingOrder, setPlacingOrder] = useState(false)
  const [orderForm, setOrderForm] = useState({ shippingAddress: '', notes: '' })
  const [showOrderForm, setShowOrderForm] = useState(false)

  const handleUpdateQty = async (productId, qty) => {
    try { await updateItem(productId, qty) }
    catch (err) { toast.error(err.response?.data?.message || 'Failed to update') }
  }

  const handleRemove = async (productId, name) => {
    try {
      await removeItem(productId)
      toast.success(`${name} removed from cart`)
    } catch { toast.error('Failed to remove item') }
  }

  const handlePlaceOrder = async (e) => {
    e.preventDefault()
    setPlacingOrder(true)
    try {
      const { data } = await ordersApi.placeOrder({
        shippingAddress: orderForm.shippingAddress || null,
        notes: orderForm.notes || null
      })
      toast.success('Order placed successfully! 🎉')
      navigate(`/orders`)
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to place order')
    } finally {
      setPlacingOrder(false)
    }
  }

  if (loading) return (
    <PageWrapper>
      <div className="flex justify-center py-32"><Spinner size="lg" /></div>
    </PageWrapper>
  )

  const items = cart?.items ?? []

  if (items.length === 0) return (
    <PageWrapper>
      <EmptyState
        icon="🛒"
        title="Your cart is empty"
        description="Looks like you haven't added anything yet. Discover our fresh products!"
        action={<Link to="/products" className="btn-primary">Browse Products</Link>}
      />
    </PageWrapper>
  )

  return (
    <PageWrapper>
      <h1 className="font-display text-4xl font-bold text-gray-900 mb-8">Your Cart</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Items list */}
        <div className="lg:col-span-2 space-y-4">
          {items.map(item => (
            <div key={item.id} className="card p-5 flex gap-5 items-center">
              {/* Image */}
              <Link to={`/products/${item.productId}`}
                className="w-20 h-20 rounded-xl overflow-hidden flex-shrink-0 bg-gray-50">
                <img src={item.productImage} alt={item.productName}
                  className="w-full h-full object-cover hover:scale-105 transition-transform"
                  onError={(e) => { e.target.src = 'https://placehold.co/80?text=?' }} />
              </Link>

              {/* Info */}
              <div className="flex-1 min-w-0">
                <Link to={`/products/${item.productId}`}
                  className="font-semibold text-gray-900 hover:text-brand-500 transition-colors block mb-1 truncate">
                  {item.productName}
                </Link>
                <p className="text-sm text-gray-400">${item.unitPrice.toFixed(2)} each</p>
              </div>

              {/* Qty controls */}
              <div className="flex items-center border border-gray-200 rounded-xl overflow-hidden">
                <button onClick={() => handleUpdateQty(item.productId, item.quantity - 1)}
                  disabled={item.quantity <= 1}
                  className="px-3 py-2 text-gray-500 hover:bg-gray-50 disabled:opacity-40 transition-colors">
                  −
                </button>
                <span className="px-3 py-2 text-sm font-semibold min-w-[2rem] text-center">
                  {item.quantity}
                </span>
                <button onClick={() => handleUpdateQty(item.productId, item.quantity + 1)}
                  disabled={item.quantity >= item.availableStock}
                  className="px-3 py-2 text-gray-500 hover:bg-gray-50 disabled:opacity-40 transition-colors">
                  +
                </button>
              </div>

              {/* Subtotal */}
              <div className="text-right w-24 flex-shrink-0">
                <span className="font-bold text-gray-900">${item.subTotal.toFixed(2)}</span>
              </div>

              {/* Remove */}
              <button onClick={() => handleRemove(item.productId, item.productName)}
                className="text-gray-300 hover:text-red-400 transition-colors p-1">
                <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          ))}
        </div>

        {/* Order summary */}
        <div className="lg:col-span-1">
          <div className="card p-6 sticky top-24">
            <h2 className="font-display text-xl font-semibold text-gray-900 mb-5">Order Summary</h2>

            <div className="space-y-3 mb-5">
              {items.map(item => (
                <div key={item.id} className="flex justify-between text-sm">
                  <span className="text-gray-500 truncate mr-2">{item.productName} × {item.quantity}</span>
                  <span className="font-medium text-gray-700">${item.subTotal.toFixed(2)}</span>
                </div>
              ))}
            </div>

            <div className="border-t border-gray-100 pt-4 mb-6">
              <div className="flex justify-between items-baseline">
                <span className="font-semibold text-gray-900">Total</span>
                <span className="text-2xl font-display font-bold text-brand-500">
                  ${cart?.totalPrice?.toFixed(2) ?? '0.00'}
                </span>
              </div>
            </div>

            {/* Order form */}
            {showOrderForm ? (
              <form onSubmit={handlePlaceOrder} className="space-y-3">
                <input
                  type="text"
                  placeholder="Shipping address (optional)"
                  value={orderForm.shippingAddress}
                  onChange={e => setOrderForm(p => ({ ...p, shippingAddress: e.target.value }))}
                  className="input text-sm"
                />
                <textarea
                  placeholder="Notes for your order (optional)"
                  value={orderForm.notes}
                  onChange={e => setOrderForm(p => ({ ...p, notes: e.target.value }))}
                  rows={2}
                  className="input text-sm resize-none"
                />
                <button type="submit" disabled={placingOrder} className="btn-primary w-full justify-center">
                  {placingOrder ? (
                    <span className="flex items-center justify-center gap-2">
                      <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                      Placing order…
                    </span>
                  ) : '✓ Confirm Order'}
                </button>
                <button type="button" onClick={() => setShowOrderForm(false)} className="btn-ghost w-full text-sm">
                  Cancel
                </button>
              </form>
            ) : (
              <button onClick={() => setShowOrderForm(true)} className="btn-primary w-full justify-center">
                Proceed to Checkout →
              </button>
            )}

            <Link to="/products" className="block text-center text-sm text-gray-400 hover:text-brand-500 mt-4 transition-colors">
              ← Continue Shopping
            </Link>
          </div>
        </div>
      </div>
    </PageWrapper>
  )
}
