// src/store/favoriteSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  favoriteItems: [],
};

const favoriteSlice = createSlice({
  name: 'favorite',
  initialState,
  reducers: {
    toggleFavorite(state, action) {
      // نبحث هل المنتج موجود بالفعل في المفضلة؟
      const itemIndex = state.favoriteItems.findIndex(
        (item) => item.id === action.payload.id
      );

      if (itemIndex >= 0) {
        // إذا كان موجوداً، نقوم بحذفه (Toggle Off)
        state.favoriteItems.splice(itemIndex, 1);
      } else {
        // إذا لم يكن موجوداً، نضيفه (Toggle On)
        state.favoriteItems.push(action.payload);
      }
    },
  },
});

export const { toggleFavorite } = favoriteSlice.actions;
export default favoriteSlice.reducer;