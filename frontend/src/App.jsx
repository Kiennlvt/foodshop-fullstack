import { Routes, Route } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { AuthProvider } from './context/AuthContext'
import { CartProvider } from './context/CartContext'
import Navbar from './components/Navbar'
import { PrivateRoute } from './components/index.jsx'

// Pages
import Login from './pages/Login'
import Register from './pages/Register'
import ProductList from './pages/ProductList'
import ProductDetail from './pages/ProductDetail'
import Cart from './pages/Cart'
import OrderHistory from './pages/OrderHistory'

// Simple home page
function Home() {
  return (
    <div className="min-h-[calc(100vh-64px)]">
      {/* Hero */}
      <section className="relative bg-gradient-to-br from-brand-500 to-brand-700 text-white overflow-hidden">
        <div className="absolute inset-0 opacity-10 select-none pointer-events-none">
          {['🥦','🍎','🍞','🥩','🧀','🫐','🥕','🍋','🍇','🥑'].map((e, i) => (
            <span key={i} className="absolute text-7xl"
              style={{ top:`${8+(i*9)%75}%`, left:`${3+(i*11)%90}%`, transform:`rotate(${(i%3-1)*25}deg)` }}>
              {e}
            </span>
          ))}
        </div>
        <div className="relative max-w-7xl mx-auto px-6 py-24 lg:py-32">
          <p className="text-brand-200 font-medium tracking-wider uppercase text-sm mb-4">
            Farm-fresh · Artisan · Local
          </p>
          <h1 className="font-display text-5xl lg:text-7xl font-bold leading-tight mb-6">
            Food the way<br /><em>it should be.</em>
          </h1>
          <p className="text-white/80 text-xl mb-10 max-w-lg">
            Discover premium groceries, artisan breads, fresh produce and more —
            sourced from local farms and delivered fast.
          </p>
          <div className="flex gap-4 flex-wrap">
            <a href="/products"
              className="bg-white text-brand-600 font-semibold px-8 py-4 rounded-2xl
                         hover:bg-brand-50 transition-colors shadow-warm-lg">
              Shop Now →
            </a>
            <a href="/register"
              className="border-2 border-white/30 text-white font-semibold px-8 py-4 rounded-2xl
                         hover:bg-white/10 transition-colors backdrop-blur-sm">
              Create Account
            </a>
          </div>
        </div>
      </section>

      {/* Features strip */}
      <section className="bg-white border-b border-gray-100">
        <div className="max-w-7xl mx-auto px-6 py-8 grid grid-cols-2 lg:grid-cols-4 gap-6">
          {[
            ['🚚','Fast Delivery','Same-day for orders before 2pm'],
            ['🌿','100% Fresh','Straight from local farms'],
            ['♻️','Eco Packaging','Sustainable & recyclable'],
            ['🔒','Secure Payment','JWT-protected checkout'],
          ].map(([icon, title, desc]) => (
            <div key={title} className="flex items-start gap-3">
              <span className="text-2xl">{icon}</span>
              <div>
                <p className="font-semibold text-gray-900 text-sm">{title}</p>
                <p className="text-xs text-gray-400 mt-0.5">{desc}</p>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 3000,
            style: {
              background: '#fff',
              color: '#1a1a2e',
              border: '1px solid #ffecd6',
              borderRadius: '12px',
              fontFamily: '"DM Sans", sans-serif',
            },
            success: { iconTheme: { primary: '#e8650a', secondary: '#fff' } },
          }}
        />

        <div className="min-h-screen bg-cream">
          <Navbar />
          <Routes>
            <Route path="/"         element={<Home />} />
            <Route path="/login"    element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/products" element={<ProductList />} />
            <Route path="/products/:id" element={<ProductDetail />} />

            {/* Protected routes */}
            <Route path="/cart" element={
              <PrivateRoute><Cart /></PrivateRoute>
            } />
            <Route path="/orders" element={
              <PrivateRoute><OrderHistory /></PrivateRoute>
            } />
          </Routes>
        </div>
      </CartProvider>
    </AuthProvider>
  )
}
