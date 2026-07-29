import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useSelector } from 'react-redux';

const ProtectedRoute = ({ allowedRoles = [] }) => {
  const { isAuthenticated, user } = useSelector((state) => state.auth);
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles.length > 0) {
    const userRoles = Array.isArray(user?.roles)
      ? user.roles
      : user?.role
      ? [user.role]
      : [];

    const hasPermission = allowedRoles.some((role) => userRoles.includes(role));

    if (!hasPermission) {
      return (
        <div className="min-h-[70vh] flex flex-col items-center justify-center p-6 text-center" dir="rtl">
          <div className="bg-red-50 text-red-500 w-20 h-20 rounded-full flex items-center justify-center text-4xl mb-4 shadow-sm">
            🚫
          </div>
          <h2 className="text-2xl font-black text-gray-800 mb-2">غير مصرح بالوصول</h2>
          <p className="text-gray-500 mb-6 max-w-md">
            عذراً، ليس لديك الصلاحيات الكافية لعرض هذه الصفحة. هذه الصفحة مخصصة لـ ({allowedRoles.join(' / ')}).
          </p>
          <button
            onClick={() => window.history.back()}
            className="bg-dokany text-white px-6 py-2.5 rounded-xl font-bold hover:bg-dokany-dark transition-all"
          >
            العودة للخلف
          </button>
        </div>
      );
    }
  }

  return <Outlet />;
};

export default ProtectedRoute;
