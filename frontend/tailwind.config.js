/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          50:  '#fff8f0',
          100: '#ffecd6',
          200: '#ffd4a8',
          300: '#ffb370',
          400: '#ff8c38',
          500: '#e8650a',
          600: '#c94f06',
          700: '#a53b08',
          800: '#872f0e',
          900: '#6f280f',
        },
        forest: {
          50:  '#f0faf4',
          100: '#d9f2e3',
          200: '#b3e4c8',
          300: '#7dcfa5',
          400: '#46b47e',
          500: '#2d6a4f',
          600: '#256040',
          700: '#1e4d33',
          800: '#183d28',
          900: '#133021',
        },
        cream: '#fff8f0',
      },
      fontFamily: {
        display: ['"Playfair Display"', 'Georgia', 'serif'],
        body:    ['"DM Sans"', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        'warm': '0 4px 24px -4px rgba(232, 101, 10, 0.15)',
        'warm-lg': '0 8px 40px -8px rgba(232, 101, 10, 0.25)',
      }
    },
  },
  plugins: [],
}
