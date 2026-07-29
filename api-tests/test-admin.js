import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, createValidPngBlob, getAuthToken } from './test-helper.js';

export async function testAdminController() {
  console.log('\n==================================================');
  console.log('📌 TESTING ADMIN CONTROLLER (/api/Admin)');
  console.log('==================================================');

  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
  if (!adminToken) {
    console.log('⚠️ Could not log in as Admin. Skipping Admin Controller tests.');
    return;
  }

  // 1. GET Admin All Products
  let targetProductId = null;
  {
    const res = await fetch(`${BASE_URL}/api/Admin/admin/all?page=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    const passed = logResult('1. GET /api/Admin/admin/all (Admin Product Moderation List)', res, data);
    if (passed && data?.data?.items?.length > 0) {
      targetProductId = data.data.items[0].id;
    }
  }

  // 2. GET Admin Product Details
  if (targetProductId) {
    const res = await fetch(`${BASE_URL}/api/Admin/${targetProductId}/admin`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`2. GET /api/Admin/${targetProductId}/admin (Admin Product Details)`, res, data);
  }

  // 3. PATCH Approve Product
  if (targetProductId) {
    const res = await fetch(`${BASE_URL}/api/Admin/${targetProductId}/approve`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`3. PATCH /api/Admin/${targetProductId}/approve (Approve Product)`, res, data);
  }

  // 4. PATCH Reject Product
  if (targetProductId) {
    const res = await fetch(`${BASE_URL}/api/Admin/${targetProductId}/reject`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`4. PATCH /api/Admin/${targetProductId}/reject (Reject Product)`, res, data);
  }
}

if (process.argv[1].endsWith('test-admin.js')) {
  testAdminController();
}
