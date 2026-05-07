import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { productsApi } from '../api/axios'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import { PageWrapper, Spinner } from '../components/index.jsx'
import toast from 'react-hot-toast'

export default function ProductDetail() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { addToCart } = useCart()
  const { isAuthenticated } = useAuth()

  const [product, setProduct] = useState(null)
  const [loading, setLoading] = useState(true)
  const [quantity, setQuantity] = useState(1)
  const [adding, setAdding] = useState(false)

  useEffect(() => {
    productsApi.getById(id)
      .then(({ data }) => setProduct(data))
      .catch(() => navigate('/products'))
      .finally(() => setLoading(false))
  }, [id, navigate])

  const handleAddToCart = async () => {
    if (!isAuthenticated) {
      toast.error('Please login first')
      navigate('/login')
      return
    }
    setAdding(true)
    try {
      await addToCart(product.id, quantity)
      toast.success(`${product.name} × ${quantity} added to cart!`)
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to add to cart')
    } finally {
      setAdding(false)
    }
  }

  if (loading) return (
    <PageWrapper>
      <div className="flex justify-center py-32"><Spinner size="lg" /></div>
    </PageWrapper>
  )

  if (!product) return null

  const isOutOfStock = product.stockQuantity === 0
  const maxQty = Math.min(product.stockQuantity, 20)

  return (
    <PageWrapper>
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-8">
        <Link to="/products" className="hover:text-brand-500 transition-colors">Products</Link>
        <span>/</span>
        <span className="text-gray-600">{product.categoryName}</span>
        <span>/</span>
        <span className="text-gray-900 font-medium">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
        {/* Image */}
        <div className="relative rounded-3xl overflow-hidden bg-gray-50 aspect-square">
          <img
            src={product.imageUrl}
            alt={product.name}
            className="w-full h-full object-cover"
            onError={(e) => { e.target.src = 'https://placehold.co/600x600?text=No+Image' }}
          />
          {isOutOfStock && (
            <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
              <span className="bg-white text-red-600 font-bold text-lg px-6 py-3 rounded-2xl">
                Out of Stock
              </span>
            </div>
          )}
        </div>

        {/* Info */}
        <div className="flex flex-col">
          <span className="badge bg-brand-50 text-brand-600 mb-4 w-fit">
            {product.categoryName}
          </span>

          <h1 className="font-display text-4xl font-bold text-gray-900 mb-4 leading-tight">
            {product.name}
          </h1>

          <p className="text-gray-500 leading-relaxed mb-8">
            {product.description}
          </p>

          {/* Price */}
          <div className="flex items-baseline gap-3 mb-8">
            <span className="text-5xl font-display font-bold text-brand-500">
              ${product.price.toFixed(2)}
            </span>
            <span className="text-gray-400 text-sm">per unit</span>
          </div>

          {/* Stock indicator */}
          <div className="mb-8">
            {isOutOfStock ? (
              <div className="flex items-center gap-2 text-red-500">
                <span className="w-2 h-2 rounded-full bg-red-500" />
                <span className="text-sm font-medium">Out of stock</span>
              </div>
            ) : product.stockQuantity <= 5 ? (
              <div className="flex items-center gap-2 text-orange-500">
                <span className="w-2 h-2 rounded-full bg-orange-500 animate-pulse" />
                <span className="text-sm font-medium">Only {product.stockQuantity} left in stock!</span>
              </div>
            ) : (
              <div className="flex items-center gap-2 text-green-600">
                <span className="w-2 h-2 rounded-full bg-green-500" />
                <span className="text-sm font-medium">In stock ({product.stockQuantity} available)</span>
              </div>
            )}
          </div>

          {/* Quantity + Add to cart */}
          {!isOutOfStock && (
            <div className="flex items-center gap-4 mb-6">
              <div className="flex items-center border border-gray-200 rounded-xl overflow-hidden">
                <button
                  onClick={() => setQuantity(q => Math.max(1, q - 1))}
                  className="px-4 py-3 text-gray-600 hover:bg-gray-50 transition-colors text-lg font-medium">
                  −
                </button>
                <span className="px-5 py-3 text-gray-900 font-semibold min-w-[3rem] text-center">
                  {quantity}
                </span>
                <button
                  onClick={() => setQuantity(q => Math.min(maxQty, q + 1))}
                  className="px-4 py-3 text-gray-600 hover:bg-gray-50 transition-colors text-lg font-medium">
                  +
                </button>
              </div>

              <button
                onClick={handleAddToCart}
                disabled={adding}
                className="btn-primary flex-1 text-base py-3 justify-center flex items-center gap-2">
                {adding ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                    Adding…
                  </>
                ) : (
                  <>🛒 Add to Cart — ${(product.price * quantity).toFixed(2)}</>
                )}
              </button>
            </div>
          )}

          <Link to="/products" className="btn-secondary text-sm mt-auto w-fit">
            ← Back to Products
          </Link>
        </div>
      </div>
    </PageWrapper>
  )
}
