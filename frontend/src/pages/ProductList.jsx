import { useState, useEffect, useCallback } from 'react'
import { productsApi, categoriesApi } from '../api/axios'
import ProductCard from '../components/ProductCard'
import { PageWrapper, Spinner } from '../components/index.jsx'

const SORT_OPTIONS = [
  { value: '',          label: 'Newest First' },
  { value: 'price_asc', label: 'Price: Low → High' },
  { value: 'price_desc',label: 'Price: High → Low' },
  { value: 'name',      label: 'Name A–Z' },
]

export default function ProductList() {
  const [products, setProducts] = useState([])
  const [categories, setCategories] = useState([])
  const [meta, setMeta] = useState({ totalCount: 0, totalPages: 1, page: 1 })
  const [loading, setLoading] = useState(true)

  const [params, setParams] = useState({
    page: 1, pageSize: 12,
    categoryId: '', search: '', sortBy: '',
    minPrice: '', maxPrice: ''
  })
  const [searchInput, setSearchInput] = useState('')

  const fetchCategories = useCallback(async () => {
    try {
      const { data } = await categoriesApi.getAll()
      setCategories(data)
    } catch {}
  }, [])

  const fetchProducts = useCallback(async () => {
    setLoading(true)
    try {
      const cleanParams = Object.fromEntries(
        Object.entries(params).filter(([, v]) => v !== '' && v !== null)
      )
      const { data } = await productsApi.getAll(cleanParams)
      setProducts(data.items)
      setMeta({ totalCount: data.totalCount, totalPages: data.totalPages, page: data.page })
    } catch {
      setProducts([])
    } finally {
      setLoading(false)
    }
  }, [params])

  useEffect(() => { fetchCategories() }, [fetchCategories])
  useEffect(() => { fetchProducts() },  [fetchProducts])

  const updateParam = (key, val) => setParams(p => ({ ...p, [key]: val, page: 1 }))

  // Debounced search
  useEffect(() => {
    const t = setTimeout(() => updateParam('search', searchInput), 500)
    return () => clearTimeout(t)
  }, [searchInput])

  return (
    <PageWrapper>
      {/* Header */}
      <div className="mb-8">
        <h1 className="font-display text-4xl font-bold text-gray-900 mb-2">
          Our Products
        </h1>
        <p className="text-gray-500">
          {meta.totalCount} fresh items — sourced locally, delivered to your door.
        </p>
      </div>

      <div className="flex flex-col lg:flex-row gap-8">
        {/* Sidebar filters */}
        <aside className="lg:w-64 flex-shrink-0">
          <div className="card p-5 space-y-6 sticky top-24">
            <h2 className="font-semibold text-gray-900">Filters</h2>

            {/* Search */}
            <div>
              <label className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-2 block">
                Search
              </label>
              <div className="relative">
                <input
                  type="text"
                  placeholder="Search products…"
                  value={searchInput}
                  onChange={e => setSearchInput(e.target.value)}
                  className="input pl-9 text-sm"
                />
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">🔍</span>
              </div>
            </div>

            {/* Categories */}
            <div>
              <label className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-2 block">
                Category
              </label>
              <div className="space-y-1">
                <button
                  onClick={() => updateParam('categoryId', '')}
                  className={`w-full text-left text-sm px-3 py-2 rounded-lg transition-colors ${
                    params.categoryId === '' ? 'bg-brand-50 text-brand-600 font-medium' : 'text-gray-600 hover:bg-gray-50'
                  }`}>
                  All Categories
                </button>
                {categories.map(cat => (
                  <button key={cat.id}
                    onClick={() => updateParam('categoryId', cat.id)}
                    className={`w-full text-left text-sm px-3 py-2 rounded-lg transition-colors flex justify-between items-center ${
                      params.categoryId === cat.id ? 'bg-brand-50 text-brand-600 font-medium' : 'text-gray-600 hover:bg-gray-50'
                    }`}>
                    <span>{cat.name}</span>
                    <span className="text-xs text-gray-400">{cat.productCount}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Price range */}
            <div>
              <label className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-2 block">
                Price Range
              </label>
              <div className="flex gap-2">
                <input type="number" placeholder="Min" min="0"
                  value={params.minPrice} onChange={e => updateParam('minPrice', e.target.value)}
                  className="input text-sm" />
                <input type="number" placeholder="Max" min="0"
                  value={params.maxPrice} onChange={e => updateParam('maxPrice', e.target.value)}
                  className="input text-sm" />
              </div>
            </div>

            {/* Sort */}
            <div>
              <label className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-2 block">
                Sort By
              </label>
              <select
                value={params.sortBy}
                onChange={e => updateParam('sortBy', e.target.value)}
                className="input text-sm bg-white">
                {SORT_OPTIONS.map(o => (
                  <option key={o.value} value={o.value}>{o.label}</option>
                ))}
              </select>
            </div>

            {/* Clear */}
            <button
              onClick={() => { setSearchInput(''); setParams({ page:1,pageSize:12,categoryId:'',search:'',sortBy:'',minPrice:'',maxPrice:'' }) }}
              className="btn-secondary w-full text-sm">
              Clear Filters
            </button>
          </div>
        </aside>

        {/* Products grid */}
        <div className="flex-1">
          {loading ? (
            <div className="flex items-center justify-center py-32">
              <Spinner size="lg" />
            </div>
          ) : products.length === 0 ? (
            <div className="text-center py-32">
              <div className="text-6xl mb-4">🔍</div>
              <h3 className="text-xl font-display font-semibold text-gray-700 mb-2">No products found</h3>
              <p className="text-gray-400">Try adjusting your filters or search term.</p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-5">
                {products.map(p => <ProductCard key={p.id} product={p} />)}
              </div>

              {/* Pagination */}
              {meta.totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 mt-10">
                  <button
                    onClick={() => setParams(p => ({ ...p, page: p.page - 1 }))}
                    disabled={meta.page === 1}
                    className="btn-secondary text-sm disabled:opacity-40">
                    ← Prev
                  </button>
                  <div className="flex gap-1">
                    {Array.from({ length: meta.totalPages }, (_, i) => i + 1)
                      .filter(pg => Math.abs(pg - meta.page) <= 2)
                      .map(pg => (
                        <button key={pg}
                          onClick={() => setParams(p => ({ ...p, page: pg }))}
                          className={`w-9 h-9 rounded-lg text-sm font-medium transition-colors ${
                            pg === meta.page ? 'bg-brand-500 text-white' : 'bg-white text-gray-600 hover:bg-brand-50 border border-gray-200'
                          }`}>
                          {pg}
                        </button>
                      ))}
                  </div>
                  <button
                    onClick={() => setParams(p => ({ ...p, page: p.page + 1 }))}
                    disabled={meta.page === meta.totalPages}
                    className="btn-secondary text-sm disabled:opacity-40">
                    Next →
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </PageWrapper>
  )
}
