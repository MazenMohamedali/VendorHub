import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, createValidPngBlob, getAuthToken, generateEgyptianPhone } from './test-helper.js';

export async function testOrderController() {
  console.log('\n==================================================');
  console.log('📌 TESTING ORDER CONTROLLER (/api/Order)');
  console.log('==================================================');

  const customerEmail = `order_cust_${Date.now()}@example.com`;
  const vendorEmail = `order_vend_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  // Register Customer
  const custRegRes = await fetch(`${BASE_URL}/api/Account/register/customer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Order",
      secondName: "Customer",
      email: customerEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: generateEgyptianPhone(),
      address: "123 Order Lane"
    })
  });
  const custRegData = await safeParseJson(custRegRes);
  const customerToken = custRegData?.data?.token || custRegData?.data?.Token || (typeof custRegData?.data === 'string' ? custRegData.data : null) || await getAuthToken(customerEmail, defaultPassword);

  // Register Vendor & Approve
  const regRes = await fetch(`${BASE_URL}/api/Account/register/vendor`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Order",
      secondName: "Vendor",
      email: vendorEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: generateEgyptianPhone(),
      storeName: `Order Store ${Date.now()}`
    })
  });
  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);

  // Approve Vendor via Admin
  let vendorToken = null;
  if (adminToken) {
    const listRes = await fetch(`${BASE_URL}/api/Vendor?page=1&pageSize=100`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const listData = await safeParseJson(listRes);
    const newVendor = listData?.data?.items?.find(v => v.email === vendorEmail);
    if (newVendor?.id) {
      await fetch(`${BASE_URL}/api/Account/approve-vendor/${newVendor.id}`, {
        method: 'PATCH',
        headers: { 'Authorization': `Bearer ${adminToken}` }
      });
      await fetch(`${BASE_URL}/api/Permission/vendor/${newVendor.id}/enable/CanUploadProducts`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${adminToken}` }
      });
    }
  }

  vendorToken = await getAuthToken(vendorEmail, defaultPassword);

  let activeCategoryId = 1;
  const catRes = await fetch(`${BASE_URL}/api/Category/active`);
  const catData = await safeParseJson(catRes);
  if (catData?.data?.length > 0) {
    activeCategoryId = catData.data[0].id;
  }

  let validProductId = 2;
  if (vendorToken) {
    const formData = new FormData();
    formData.append('Name', `Order Test Prod ${Date.now()}`);
    formData.append('Price', '149.99');
    formData.append('Quantity', '50');
    formData.append('CategoryId', activeCategoryId.toString());
    formData.append('ImageFile', createValidPngBlob(), 'test_product.png');

    const addRes = await fetch(`${BASE_URL}/api/Product`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${vendorToken}` },
      body: formData
    });
    const addData = await safeParseJson(addRes);
    const newId = addData?.data?.id || addData?.data?.Id;
    if (newId) {
      validProductId = newId;
      if (adminToken) {
        await fetch(`${BASE_URL}/api/Admin/${validProductId}/approve`, {
          method: 'PATCH',
          headers: { 'Authorization': `Bearer ${adminToken}` }
        });
      }
    }
  }

  let createdOrderId = null;

  // 1. POST Checkout Order (Customer)
  if (customerToken) {
    const res = await fetch(`${BASE_URL}/api/Order`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${customerToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        deliveryAddress: "45 Nile Corniche, Maadi, Cairo",
        phoneNumber: "01112223334",
        cartItems: [
          { productId: validProductId, productName: "Wireless Headphones", imageUrl: "headphones.jpg", quantity: 2, unitPrice: 149.99 }
        ]
      })
    });
    const data = await safeParseJson(res);
    const passed = logResult('1. POST /api/Order (Customer Place Order)', res, data);
    if (passed && data?.data?.orderId) {
      createdOrderId = data.data.orderId;
    }
  }

  // 2. GET My Orders (Customer)
  if (customerToken) {
    const res = await fetch(`${BASE_URL}/api/Order/my-orders?pageNumber=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('2. GET /api/Order/my-orders (Customer Order History)', res, data);
  }

  // 3. GET Order Details (Customer)
  if (customerToken && createdOrderId) {
    const res = await fetch(`${BASE_URL}/api/Order/${createdOrderId}`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`3. GET /api/Order/${createdOrderId} (Customer Order Details)`, res, data);
  }

  // 4. GET Vendor Orders (Vendor)
  if (vendorToken) {
    const res = await fetch(`${BASE_URL}/api/Order/vendor-orders?page=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const data = await safeParseJson(res);
    logResult('4. GET /api/Order/vendor-orders (Vendor Orders List)', res, data);
  }

  // 5. GET Vendor Orders Stats (Vendor)
  if (vendorToken) {
    const res = await fetch(`${BASE_URL}/api/Order/vendor-orders-stats`, {
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const data = await safeParseJson(res);
    logResult('5. GET /api/Order/vendor-orders-stats (Vendor Order Metrics)', res, data);
  }

  // 6. PATCH Update Order Status (Vendor)
  if (vendorToken && createdOrderId) {
    const res = await fetch(`${BASE_URL}/api/Order/${createdOrderId}/status`, {
      method: 'PATCH',
      headers: {
        'Authorization': `Bearer ${vendorToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ newStatus: "Shipped" })
    });
    const data = await safeParseJson(res);
    logResult(`6. PATCH /api/Order/${createdOrderId}/status (Vendor Update Status)`, res, data);
  }
}

if (process.argv[1].endsWith('test-order.js')) {
  testOrderController();
}
