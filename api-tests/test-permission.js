import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testPermissionController() {
  console.log('\n==================================================');
  console.log('📌 TESTING PERMISSION CONTROLLER (/api/Permission)');
  console.log('==================================================');

  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
  if (!adminToken) {
    console.log('⚠️ Could not log in as Admin. Skipping Permission tests.');
    return;
  }

  // 1. GET List All Permissions
  {
    const res = await fetch(`${BASE_URL}/api/Permission`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult('1. GET /api/Permission (List All Permissions)', res, data);
  }

  // 2. GET Vendor Permissions
  const targetVendorId = 2;
  {
    const res = await fetch(`${BASE_URL}/api/Permission/vendor/${targetVendorId}`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`2. GET /api/Permission/vendor/${targetVendorId} (Get Vendor Permissions)`, res, data);
  }

  // 3. POST Enable Vendor Permission
  {
    const res = await fetch(`${BASE_URL}/api/Permission/vendor/${targetVendorId}/enable/CanUploadProducts`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`3. POST /api/Permission/vendor/${targetVendorId}/enable/CanUploadProducts`, res, data);
  }

  // 4. POST Disable Vendor Permission
  {
    const res = await fetch(`${BASE_URL}/api/Permission/vendor/${targetVendorId}/disable/CanUploadProducts`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`4. POST /api/Permission/vendor/${targetVendorId}/disable/CanUploadProducts`, res, data);
  }

  // 5. POST Global Enable Permission
  {
    const res = await fetch(`${BASE_URL}/api/Permission/global/enable/CanUploadProducts`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult('5. POST /api/Permission/global/enable/CanUploadProducts', res, data);
  }
}

if (process.argv[1].endsWith('test-permission.js')) {
  testPermissionController();
}
