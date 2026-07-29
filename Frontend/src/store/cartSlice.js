// // src/store/cartSlice.js
// import { createSlice } from '@reduxjs/toolkit';

// const initialState = {
//   cartItems: [],
//   cartTotalQuantity: 0,
//   cartTotalAmount: 0,
// };

// // دالة مساعدة لحساب الإجمالي والكمية بأمان
// const calculateTotals = (state) => {
//   let total = 0;
//   let quantity = 0;
//   state.cartItems.forEach((item) => {
//     total += item.price * item.cartQuantity;
//     quantity += item.cartQuantity;
//   });
//   state.cartTotalQuantity = quantity;
//   state.cartTotalAmount = total;
// };

// const cartSlice = createSlice({
//   name: 'cart',
//   initialState,
//   reducers: {
//     addToCart(state, action) {
//       const itemIndex = state.cartItems.findIndex(item => item.id === action.payload.id);
//       if (itemIndex >= 0) {
//         state.cartItems[itemIndex].cartQuantity += 1;
//       } else {
//         state.cartItems.push({ ...action.payload, cartQuantity: 1 });
//       }
//       calculateTotals(state); // تحديث الإجمالي
//     },
    
//     removeFromCart(state, action) {
//       state.cartItems = state.cartItems.filter(item => item.id !== action.payload.id);
//       calculateTotals(state);
//     },

//     increaseQuantity(state, action) {
//       const itemIndex = state.cartItems.findIndex(item => item.id === action.payload.id);
//       if (itemIndex >= 0) {
//         state.cartItems[itemIndex].cartQuantity += 1;
//       }
//       calculateTotals(state);
//     },

//     decreaseQuantity(state, action) {
//       const itemIndex = state.cartItems.findIndex(item => item.id === action.payload.id);
//       if (state.cartItems[itemIndex].cartQuantity > 1) {
//         state.cartItems[itemIndex].cartQuantity -= 1;
//       } else {
//         state.cartItems = state.cartItems.filter(item => item.id !== action.payload.id);
//       }
//       calculateTotals(state);
//     },

//     clearCart(state) {
//       state.cartItems = [];
//       calculateTotals(state);
//     },
//   },
// });

// export const { addToCart, removeFromCart, increaseQuantity, decreaseQuantity, clearCart } = cartSlice.actions;
// export default cartSlice.reducer;

// src/store/cartSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  cartItems: [],
  cartTotalQuantity: 0,
  cartTotalAmount: 0,
};

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
      const newItem = { ...action.payload, cartQuantity: action.payload.cartQuantity || 1 };
      const existingIndex = state.cartItems.findIndex(
        item => Number(item.id) === Number(newItem.id)
      );

      if (existingIndex >= 0) {
        const currentQty = state.cartItems[existingIndex].cartQuantity;
        const stockAvailable = state.cartItems[existingIndex].stockQuantity;
        
        // التحقق: هل الكمية المطلوبة + الموجودة في السلة تتخطى المخزون؟
        if (currentQty + newItem.cartQuantity <= stockAvailable) {
          state.cartItems[existingIndex].cartQuantity += newItem.cartQuantity;
        } else {
          // إذا تخطى، اجعلها تساوي أقصى كمية متاحة فقط
          state.cartItems[existingIndex].cartQuantity = stockAvailable;
          alert(`عذراً، لا يوجد سوى ${stockAvailable} قطع فقط من هذا المنتج.`);
        }
      } else {
        state.cartItems.push(newItem);
      }
      calculateTotals(state);
    },
    
    removeFromCart(state, action) {
      state.cartItems = state.cartItems.filter(item => Number(item.id) !== Number(action.payload.id));
      calculateTotals(state);
    },

    increaseQuantity(state, action) {
      const itemIndex = state.cartItems.findIndex(item => Number(item.id) === Number(action.payload.id));
      if (itemIndex >= 0) {
        const item = state.cartItems[itemIndex];
        if (item.cartQuantity < item.stockQuantity) {
          state.cartItems[itemIndex].cartQuantity += 1;
          calculateTotals(state);
        } else {
          alert("لقد وصلت للحد الأقصى المتاح في المخزون لهذا المنتج");
        }
      }
    },

    decreaseQuantity(state, action) {
      const itemIndex = state.cartItems.findIndex(item => Number(item.id) === Number(action.payload.id));
      if (itemIndex >= 0) {
        if (state.cartItems[itemIndex].cartQuantity > 1) {
          state.cartItems[itemIndex].cartQuantity -= 1;
        } else {
          state.cartItems.splice(itemIndex, 1);
        }
        calculateTotals(state);
      }
    },

    clearCart(state) {
      state.cartItems = [];
      calculateTotals(state);
    },
  },
});

export const { addToCart, removeFromCart, increaseQuantity, decreaseQuantity, clearCart } = cartSlice.actions;
export default cartSlice.reducer;