import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { ordersApi } from '../api/axios'
import { PageWrapper, EmptyState, Spinner, StatusBadge } from '../components/index.jsx'

export default function OrderHistory() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [expanded, setExpanded] = useState(null)

  useEffect(() => {
    ordersApi.getMyOrders()
      .then(({ data }) => setOrders(data))
      .catch(() => setOrders([]))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return (
    <PageWrapper>
      <div className="flex justify-center py-32"><Spinner size="lg" /></div>
    </PageWrapper>
  )

  return (
    <PageWrapper>
      <div className="mb-8">
        <h1 className="font-display text-4xl font-bold text-gray-900 mb-2">My Orders</h1>
        <p className="text-gray-500">{orders.length} order{orders.length !== 1 ? 's' : ''} placed</p>
      </div>

      {orders.length === 0 ? (
        <EmptyState
          icon="📦"
          title="No orders yet"
          description="You haven't placed any orders. Start shopping to see them here!"
          action={<Link to="/products" className="btn-primary">Browse Products</Link>}
        />
      ) : (
        <div className="space-y-4">
          {orders.map(order => (
            <div key={order.id} className="card overflow-hidden">
              {/* Order header */}
              <button
                className="w-full p-5 text-left flex flex-wrap items-center gap-4"
                onClick={() => setExpanded(expanded === order.id ? null : order.id)}>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-3 mb-1">
                    <span className="font-semibold text-gray-900">Order #{order.id}</span>
                    <StatusBadge status={order.status} />
                  </div>
                  <p className="text-sm text-gray-400">
                    {new Date(order.orderDate).toLocaleDateString('en-US', {
                      year: 'numeric', month: 'long', day: 'numeric',
                      hour: '2-digit', minute: '2-digit'
                    })}
                  </p>
                </div>

                <div className="text-right">
                  <p className="text-xs text-gray-400 mb-0.5">{order.items?.length ?? 0} items</p>
                  <p className="text-xl font-display font-bold text-brand-500">
                    ${order.totalPrice.toFixed(2)}
                  </p>
                </div>

                <span className={`text-gray-400 transition-transform ${expanded === order.id ? 'rotate-180' : ''}`}>
                  ▾
                </span>
              </button>

              {/* Expanded details */}
              {expanded === order.id && (
                <div className="border-t border-gray-100 p-5 bg-gray-50/50">
                  {order.shippingAddress && (
                    <p className="text-sm text-gray-500 mb-4">
                      📍 <span className="text-gray-700">{order.shippingAddress}</span>
                    </p>
                  )}
                  {order.notes && (
                    <p className="text-sm text-gray-500 mb-4">
                      📝 <span className="text-gray-700">{order.notes}</span>
                    </p>
                  )}

                  <div className="space-y-3">
                    {order.items?.map(item => (
                      <div key={item.id} className="flex items-center gap-4 bg-white rounded-xl p-3">
                        <div className="w-12 h-12 rounded-lg overflow-hidden flex-shrink-0 bg-gray-100">
                          <img src={item.productImage} alt={item.productName}
                            className="w-full h-full object-cover"
                            onError={(e) => { e.target.src = 'https://placehold.co/48?text=?' }} />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="font-medium text-gray-900 text-sm truncate">{item.productName}</p>
                          <p className="text-xs text-gray-400">${item.unitPrice.toFixed(2)} × {item.quantity}</p>
                        </div>
                        <span className="font-semibold text-gray-700 text-sm">${item.subTotal.toFixed(2)}</span>
                      </div>
                    ))}
                  </div>

                  <div className="flex justify-between items-center mt-4 pt-4 border-t border-gray-100">
                    <span className="font-semibold text-gray-700">Total</span>
                    <span className="text-xl font-display font-bold text-brand-500">
                      ${order.totalPrice.toFixed(2)}
                    </span>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </PageWrapper>
  )
}
