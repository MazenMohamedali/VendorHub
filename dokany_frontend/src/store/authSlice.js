// // src/store/authSlice.js
// import { createSlice } from '@reduxjs/toolkit';

// const initialState = {
//   isAuthenticated: false, // افتراضياً المستخدم غير مسجل دخول
//   user: null,
// };

// const authSlice = createSlice({
//   name: 'auth',
//   initialState,
//   reducers: {
//     login(state, action) {
//       state.isAuthenticated = true;
//       state.user = action.payload; // بيانات المستخدم (مثل البريد والاسم)
//     },
//     logout(state) {
//       state.isAuthenticated = false;
//       state.user = null;
//     },
//   },
// });

// export const { login, logout } = authSlice.actions;
// export default authSlice.reducer;

// src/store/authSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  isAuthenticated: false,
  user: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    login(state, action) {
      const userData = action.payload;
      // Normalize the user object to always have 'id' and 'roles' array
      state.isAuthenticated = true;
      state.user = {
        id: userData.id ?? userData.userId ?? userData.Id,
        firstName: userData.firstName,
        secondName: userData.secondName,
        email: userData.email,
        phoneNumber: userData.phoneNumber,
        // Convert single role to array if needed
        roles: Array.isArray(userData.roles) ? userData.roles : (userData.role ? [userData.role] : []),
        storeName: userData.storeName,
        balance: userData.balance,
        accountStatus: userData.accountStatus,
        // Keep any other fields
        ...userData,
      };
    },
    logout(state) {
      state.isAuthenticated = false;
      state.user = null;
    },
  },
});

export const { login, logout } = authSlice.actions;
export default authSlice.reducer;