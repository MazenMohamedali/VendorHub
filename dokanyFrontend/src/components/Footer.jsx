// src/components/Footer.jsx
import React from 'react';

const Footer = () => {
  return (
    <footer className="bg-white border-t border-gray-100 py-10 mt-auto">
      <div className="max-w-7xl mx-auto px-4 text-center">
        <p className="text-gray-400 text-sm">
          &copy; {new Date().getFullYear()} مشروع دكاني - جميع الحقوق محفوظة لفريق العمل
        </p>
      </div>
    </footer>
  );
};

export default Footer;