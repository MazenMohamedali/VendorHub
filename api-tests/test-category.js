import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, createValidPngBlob, getAuthToken } from './test-helper.js';

export async function testCategoryController() {
  console.log('\n==================================================');
  console.log('📌 TESTING CATEGORY CONTROLLER (/api/Category)');
  console.log('==================================================');

  let adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
  let createdCategoryId = null;

  // 1. GET Active Categories (Public)
  {
    const res = await fetch(`${BASE_URL}/api/Category/active`);
    const data = await safeParseJson(res);
    logResult('1. GET /api/Category/active (Public)', res, data);
  }

  // 2. POST Create Category (Admin)
  if (adminToken) {
    const formData = new FormData();
    formData.append('Name', `Test Category ${Date.now()}`);
    formData.append('ImageFile', createValidPngBlob(), 'test_category.png');

    const res = await fetch(`${BASE_URL}/api/Category`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${adminToken}` },
      body: formData
    });
    const data = await safeParseJson(res);
    const passed = logResult('2. POST /api/Category (Admin Create Category)', res, data);
    if (passed && data?.data?.id) {
      createdCategoryId = data.data.id;
    }
  }

  // 3. GET Category By ID
  if (createdCategoryId) {
    const res = await fetch(`${BASE_URL}/api/Category/${createdCategoryId}`);
    const data = await safeParseJson(res);
    logResult(`3. GET /api/Category/${createdCategoryId} (Details By ID)`, res, data);
  }

  // 4. GET Search Category
  {
    const res = await fetch(`${BASE_URL}/api/Category/search?searchTerm=Test`);
    const data = await safeParseJson(res);
    logResult('4. GET /api/Category/search (Search By Term)', res, data);
  }

  // 5. GET Admin All Categories
  if (adminToken) {
    const res = await fetch(`${BASE_URL}/api/Category/admin/all?pageNumber=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult('5. GET /api/Category/admin/all (Admin List Categories)', res, data);
  }

  // 6. PUT Update Category (Admin)
  if (adminToken && createdCategoryId) {
    const formData = new FormData();
    formData.append('Name', `Updated Category ${Date.now()}`);
    formData.append('IsActive', 'true');

    const res = await fetch(`${BASE_URL}/api/Category/${createdCategoryId}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${adminToken}` },
      body: formData
    });
    const data = await safeParseJson(res);
    logResult(`6. PUT /api/Category/${createdCategoryId} (Admin Update Category)`, res, data);
  }

  // 7. DELETE Soft Delete Category (Admin)
  if (adminToken && createdCategoryId) {
    const res = await fetch(`${BASE_URL}/api/Category/${createdCategoryId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`7. DELETE /api/Category/${createdCategoryId} (Soft Delete Category)`, res, data);
  }

  // 8. DELETE Hard Delete Category (Admin)
  if (adminToken && createdCategoryId) {
    const res = await fetch(`${BASE_URL}/api/Category/${createdCategoryId}/hard`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`8. DELETE /api/Category/${createdCategoryId}/hard (Hard Delete Category)`, res, data);
  }
}

if (process.argv[1].endsWith('test-category.js')) {
  testCategoryController();
}
