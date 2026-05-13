/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        dokany: {
          light: '#ecfdf5',   // لون الخلفية ورا المنتجات (أخضر فاتح جداً)
          DEFAULT: '#059669', // اللون الأخضر الأساسي للأزرار
          dark: '#047857',    // لون الزرار لما الماوس يجي عليه (Hover)
        }
      }
    },
  },
  plugins: [],
}