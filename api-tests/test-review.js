import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, createValidPngBlob, getAuthToken, generateEgyptianPhone } from './test-helper.js';

export async function testReviewController() {
  console.log('\n==================================================');
  console.log('📌 TESTING REVIEW CONTROLLER (/api/Review)');
  console.log('==================================================');

  const customerEmail = `rev_cust_${Date.now()}@example.com`;
  const vendorEmail = `rev_vend_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  // Register Customer
  const custRegRes = await fetch(`${BASE_URL}/api/Account/register/customer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Review",
      secondName: "User",
      email: customerEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: generateEgyptianPhone(),
      address: "Review Address"
    })
  });
  const custRegData = await safeParseJson(custRegRes);
  const customerToken = custRegData?.data?.token || custRegData?.data?.Token || (typeof custRegData?.data === 'string' ? custRegData.data : null) || await getAuthToken(customerEmail, defaultPassword);

  // Register Vendor & Approve
  const regRes = await fetch(`${BASE_URL}/api/Account/register/vendor`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Review",
      secondName: "Vendor",
      email: vendorEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: generateEgyptianPhone(),
      storeName: `Review Store ${Date.now()}`
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

  let targetProductId = 2;
  if (vendorToken) {
    const formData = new FormData();
    formData.append('Name', `Review Test Prod ${Date.now()}`);
    formData.append('Price', '99.99');
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
      targetProductId = newId;
      if (adminToken) {
        await fetch(`${BASE_URL}/api/Admin/${targetProductId}/approve`, {
          method: 'PATCH',
          headers: { 'Authorization': `Bearer ${adminToken}` }
        });
      }
    }
  }

  // 1. GET Product Reviews (Public)
  {
    const res = await fetch(`${BASE_URL}/api/Review/${targetProductId}?page=1&pageSize=10`);
    const data = await safeParseJson(res);
    logResult(`1. GET /api/Review/${targetProductId} (Public Product Reviews)`, res, data);
  }

  // Place order for product first to satisfy review eligibility rule
  if (customerToken) {
    await fetch(`${BASE_URL}/api/Order`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${customerToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        deliveryAddress: "45 Nile Corniche, Maadi, Cairo",
        phoneNumber: "01112223334",
        cartItems: [
          { productId: targetProductId, productName: "Ordered Item", imageUrl: "item.jpg", quantity: 1, unitPrice: 99.99 }
        ]
      })
    });
  }

  // 2. POST Submit Review (Customer)
  if (customerToken) {
    const res = await fetch(`${BASE_URL}/api/Review/${targetProductId}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${customerToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        rating: 5,
        comment: "Excellent product quality and very fast shipping!"
      })
    });
    const data = await safeParseJson(res);
    logResult(`2. POST /api/Review/${targetProductId} (Customer Submit Review)`, res, data);
  }
}

if (process.argv[1].endsWith('test-review.js')) {
  testReviewController();
}
