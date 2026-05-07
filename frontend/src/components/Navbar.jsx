import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useCart } from '../context/CartContext'

export default function Navbar() {
  const { user, logout, isAdmin, isAuthenticated } = useAuth()
  const { itemCount } = useCart()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  return (
    <nav className="sticky top-0 z-50 bg-white/90 backdrop-blur-sm border-b border-orange-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 group">
            <div className="w-8 h-8 bg-brand-500 rounded-lg flex items-center justify-center
                            group-hover:bg-brand-600 transition-colors">
              <span className="text-white text-lg leading-none">🌿</span>
            </div>
            <span className="font-display font-bold text-xl text-gray-900 tracking-tight">
              Food<span className="text-brand-500">Shop</span>
            </span>
          </Link>

          {/* Center nav */}
          <div className="hidden md:flex items-center gap-6">
            <NavLink to="/" end
              className={({ isActive }) =>
                `nav-link text-sm font-medium transition-colors pb-1 ${isActive ? 'active text-brand-500' : 'text-gray-600 hover:text-gray-900'}`
              }>
              Home
            </NavLink>
            <NavLink to="/products"
              className={({ isActive }) =>
                `nav-link text-sm font-medium transition-colors pb-1 ${isActive ? 'active text-brand-500' : 'text-gray-600 hover:text-gray-900'}`
              }>
              Products
            </NavLink>
            {isAdmin && (
              <NavLink to="/admin"
                className={({ isActive }) =>
                  `nav-link text-sm font-medium transition-colors pb-1 ${isActive ? 'active text-brand-500' : 'text-gray-600 hover:text-gray-900'}`
                }>
                Admin
              </NavLink>
            )}
          </div>

          {/* Right side */}
          <div className="flex items-center gap-3">
            {isAuthenticated ? (
              <>
                {/* Cart */}
                <Link to="/cart" className="relative p-2 text-gray-600 hover:text-brand-500 transition-colors">
                  <CartIcon />
                  {itemCount > 0 && (
                    <span className="absolute -top-0.5 -right-0.5 bg-brand-500 text-white text-xs
                                     w-5 h-5 rounded-full flex items-center justify-center font-medium
                                     animate-pulse">
                      {itemCount > 9 ? '9+' : itemCount}
                    </span>
                  )}
                </Link>

                {/* Orders */}
                <Link to="/orders" className="hidden md:block btn-ghost text-sm">
                  My Orders
                </Link>

                {/* User menu */}
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 bg-gradient-to-br from-brand-400 to-brand-600 rounded-full
                                  flex items-center justify-center text-white text-sm font-semibold">
                    {user?.username?.[0]?.toUpperCase()}
                  </div>
                  <button onClick={handleLogout}
                    className="hidden md:block text-sm text-gray-500 hover:text-red-500 transition-colors">
                    Logout
                  </button>
                </div>
              </>
            ) : (
              <>
                <Link to="/login" className="btn-ghost text-sm">Sign In</Link>
                <Link to="/register" className="btn-primary text-sm">Get Started</Link>
              </>
            )}
          </div>
        </div>
      </div>
    </nav>
  )
}

function CartIcon() {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.8}
        d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
    </svg>
  )
}
