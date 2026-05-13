// src/layouts/MainLayout.jsx
import React from 'react';
import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';

const MainLayout = () => {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col font-sans" dir="rtl">
      <Navbar />
      
      {/* Outlet هو المكان الذي سيتم فيه عرض محتوى الصفحات المختلفة */}
      <main className="flex-1 w-full flex flex-col">
        <Outlet />
      </main>

      <Footer />
    </div>
  );
};

export default MainLayout;