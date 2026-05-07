import { Link } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import { useState } from 'react'
import toast from 'react-hot-toast'

export default function ProductCard({ product }) {
  const { addToCart } = useCart()
  const { isAuthenticated } = useAuth()
  const [adding, setAdding] = useState(false)

  const handleAddToCart = async (e) => {
    e.preventDefault()
    if (!isAuthenticated) {
      toast.error('Please login to add items to cart')
      return
    }
    setAdding(true)
    try {
      await addToCart(product.id, 1)
      toast.success(`${product.name} added to cart!`)
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to add to cart')
    } finally {
      setAdding(false)
    }
  }

  const isOutOfStock = product.stockQuantity === 0

  return (
    <Link to={`/products/${product.id}`}
      className="card group overflow-hidden flex flex-col cursor-pointer">
      {/* Image */}
      <div className="relative overflow-hidden h-48 bg-gray-50">
        <img
          src={product.imageUrl}
          alt={product.name}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
          onError={(e) => { e.target.src = 'https://placehold.co/400x300?text=No+Image' }}
        />
        {/* Category badge */}
        <span className="absolute top-3 left-3 badge bg-white/90 text-gray-700 backdrop-blur-sm shadow-sm">
          {product.categoryName}
        </span>
        {isOutOfStock && (
          <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
            <span className="badge bg-white text-red-600 font-semibold text-sm px-3 py-1">
              Out of Stock
            </span>
          </div>
        )}
      </div>

      {/* Content */}
      <div className="p-4 flex flex-col flex-1">
        <h3 className="font-semibold text-gray-900 mb-1 group-hover:text-brand-600 transition-colors line-clamp-1">
          {product.name}
        </h3>
        <p className="text-sm text-gray-500 line-clamp-2 flex-1 mb-3">
          {product.description}
        </p>

        <div className="flex items-center justify-between">
          <div>
            <span className="text-xl font-bold text-brand-500">
              ${product.price.toFixed(2)}
            </span>
            {product.stockQuantity > 0 && product.stockQuantity <= 5 && (
              <p className="text-xs text-orange-600 font-medium mt-0.5">
                Only {product.stockQuantity} left!
              </p>
            )}
          </div>

          <button
            onClick={handleAddToCart}
            disabled={adding || isOutOfStock}
            className="btn-primary text-sm py-2 px-3 !rounded-lg"
          >
            {adding ? (
              <span className="flex items-center gap-1">
                <span className="w-3 h-3 border-2 border-white/50 border-t-white rounded-full animate-spin" />
                Adding…
              </span>
            ) : (
              '+ Cart'
            )}
          </button>
        </div>
      </div>
    </Link>
  )
}
