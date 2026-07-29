import { BASE_URL, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testFavoriteController() {
  console.log('\n==================================================');
  console.log('📌 TESTING FAVORITE CONTROLLER (/api/Favorite)');
  console.log('==================================================');

  const customerEmail = `fav_cust_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  await fetch(`${BASE_URL}/api/Account/register/customer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Fav",
      secondName: "User",
      email: customerEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: "01012345678",
      address: "Fav Address"
    })
  });

  const customerToken = await getAuthToken(customerEmail, defaultPassword);
  if (!customerToken) {
    console.log('⚠️ Could not log in as Customer. Skipping Favorite tests.');
    return;
  }

  let targetProductId = 3;
  const prodListRes = await fetch(`${BASE_URL}/api/Product/list?page=1&pageSize=1`);
  const prodListData = await safeParseJson(prodListRes);
  if (prodListData?.data?.items?.length > 0) {
    targetProductId = prodListData.data.items[0].id;
  }

  // 1. POST Add Favorite Product
  {
    const res = await fetch(`${BASE_URL}/api/Favorite/product/${targetProductId}`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`1. POST /api/Favorite/product/${targetProductId} (Add Product to Favorites)`, res, data);
  }

  // 2. GET Favorite Products List
  {
    const res = await fetch(`${BASE_URL}/api/Favorite`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('2. GET /api/Favorite (Customer Wishlist)', res, data);
  }

  // 3. DELETE Remove Favorite Product
  {
    const res = await fetch(`${BASE_URL}/api/Favorite/product/${targetProductId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`3. DELETE /api/Favorite/product/${targetProductId} (Remove Product from Favorites)`, res, data);
  }
}

if (process.argv[1].endsWith('test-favorite.js')) {
  testFavoriteController();
}
