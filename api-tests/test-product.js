import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, createValidPngBlob, getAuthToken } from './test-helper.js';

export async function testProductController() {
  console.log('\n==================================================');
  console.log('📌 TESTING PRODUCT CONTROLLER (/api/Product)');
  console.log('==================================================');

  // Register vendor and approve them so they can upload products
  const vendorEmail = `vendor_prod_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  await fetch(`${BASE_URL}/api/Account/register/vendor`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Prod",
      secondName: "Vendor",
      email: vendorEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: "01098765432",
      storeName: `Product Store ${Date.now()}`
    })
  });

  const vendorToken = await getAuthToken(vendorEmail, defaultPassword);
  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);

  // Approve the newly registered vendor if admin token exists
  if (adminToken) {
    const meRes = await fetch(`${BASE_URL}/api/Account/me`, {
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const meData = await safeParseJson(meRes);
    if (meData?.data?.id) {
      await fetch(`${BASE_URL}/api/Account/approve-vendor/${meData.data.id}`, {
        method: 'PATCH',
        headers: { 'Authorization': `Bearer ${adminToken}` }
      });
    }
  }

  // 1. GET Public Product List
  {
    const res = await fetch(`${BASE_URL}/api/Product/list?page=1&pageSize=10`);
    const data = await safeParseJson(res);
    logResult('1. GET /api/Product/list (Public Products List)', res, data);
  }

  // 2. GET Hot Products
  {
    const res = await fetch(`${BASE_URL}/api/Product/hot-products?count=6`);
    const data = await safeParseJson(res);
    logResult('2. GET /api/Product/hot-products (Hot Products List)', res, data);
  }

  // 3. GET Search By Name
  {
    const res = await fetch(`${BASE_URL}/api/Product/search-name?name=Laptop`);
    const data = await safeParseJson(res);
    logResult('3. GET /api/Product/search-name', res, data);
  }

  // 4. GET Search By Category
  {
    const res = await fetch(`${BASE_URL}/api/Product/search-category?category=إلكترونيات`);
    const data = await safeParseJson(res);
    logResult('4. GET /api/Product/search-category', res, data);
  }

  // 5. GET Search By Price
  {
    const res = await fetch(`${BASE_URL}/api/Product/search-price?min=10&max=5000`);
    const data = await safeParseJson(res);
    logResult('5. GET /api/Product/search-price', res, data);
  }

  let createdProductId = null;

  // 6. POST Add Product (Vendor)
  if (vendorToken) {
    const formData = new FormData();
    formData.append('Name', `Test Product ${Date.now()}`);
    formData.append('Price', '299.99');
    formData.append('Quantity', '50');
    formData.append('CategoryId', '1');
    formData.append('ImageFile', createValidPngBlob(), 'test_product.png');

    const res = await fetch(`${BASE_URL}/api/Product`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${vendorToken}` },
      body: formData
    });
    const data = await safeParseJson(res);
    const passed = logResult('6. POST /api/Product (Vendor Add Product)', res, data);
    if (passed && data?.data?.id) {
      createdProductId = data.data.id;
    }
  }

  // 7. GET My Products (Vendor)
  if (vendorToken) {
    const res = await fetch(`${BASE_URL}/api/Product/my-products?page=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const data = await safeParseJson(res);
    logResult('7. GET /api/Product/my-products (Vendor Storefront List)', res, data);
  }

  // 8. GET Vendor Product Details
  if (vendorToken && createdProductId) {
    const res = await fetch(`${BASE_URL}/api/Product/${createdProductId}/vendor`, {
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`8. GET /api/Product/${createdProductId}/vendor (Vendor Product Details)`, res, data);
  }

  // 9. PUT Update Product (Vendor)
  if (vendorToken && createdProductId) {
    const formData = new FormData();
    formData.append('Name', `Updated Test Product ${Date.now()}`);
    formData.append('Price', '349.99');
    formData.append('Quantity', '45');
    formData.append('CategoryId', '1');

    const res = await fetch(`${BASE_URL}/api/Product/${createdProductId}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${vendorToken}` },
      body: formData
    });
    const data = await safeParseJson(res);
    logResult(`9. PUT /api/Product/${createdProductId} (Vendor Edit Product)`, res, data);
  }

  // 10. DELETE Product (Vendor)
  if (vendorToken && createdProductId) {
    const res = await fetch(`${BASE_URL}/api/Product/${createdProductId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`10. DELETE /api/Product/${createdProductId} (Delete Product)`, res, data);
  }
}

if (process.argv[1].endsWith('test-product.js')) {
  testProductController();
}
