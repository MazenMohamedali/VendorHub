// src/store/cartSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  cartItems: [],
  cartTotalQuantity: 0,
  cartTotalAmount: 0,
};

// دالة مساعدة لحساب الإجمالي والكمية بأمان
const calculateTotals = (state) => {
  let total = 0;
  let quantity = 0;
  state.cartItems.forEach((item) => {
    total += item.price * item.cartQuantity;
    quantity += item.cartQuantity;
  });
  state.cartTotalQuantity = quantity;
  state.cartTotalAmount = total;
};

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    addToCart(state, action) {
      const itemIndex = state.cartItems.findIndex(item => item.id === action.payload.id);
      if (itemIndex >= 0) {
        state.cartItems[itemIndex].cartQuantity += 1;
      } else {
        state.cartItems.push({ ...action.payload, cartQuantity: 1 });
      }
      calculateTotals(state); // تحديث الإجمالي
    },
    
    removeFromCart(state, action) {
      state.cartItems = state.cartItems.filter(item => item.id !== action.payload.id);
      calculateTotals(state);
    },

    increaseQuantity(state, action) {
      const itemIndex = state.cartItems.findIndex(item => item.id === action.payload.id);
      if (itemIndex >= 0) {
        state.cartItems[itemIndex].cartQuantity += 1;
      }
      calculateTotals(state);
    },

    decreaseQuantity(state, action) {
      const itemIndex = state.cartItems.findIndex(item => item.id === action.payload.id);
      if (state.cartItems[itemIndex].cartQuantity > 1) {
        state.cartItems[itemIndex].cartQuantity -= 1;
      } else {
        state.cartItems = state.cartItems.filter(item => item.id !== action.payload.id);
      }
      calculateTotals(state);
    },

    clearCart(state) {
      state.cartItems = [];
      calculateTotals(state);
    },
  },
});

export const { addToCart, removeFromCart, increaseQuantity, decreaseQuantity, clearCart } = cartSlice.actions;
export default cartSlice.reducer;