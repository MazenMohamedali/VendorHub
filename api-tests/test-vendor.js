import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testVendorController() {
  console.log('\n==================================================');
  console.log('📌 TESTING VENDOR CONTROLLER (/api/Vendor)');
  console.log('==================================================');

  const vendorEmail = `vend_prof_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  await fetch(`${BASE_URL}/api/Account/register/vendor`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Vendor",
      secondName: "User",
      email: vendorEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: "01098765432",
      storeName: `Vendor Store ${Date.now()}`
    })
  });

  const vendorToken = await getAuthToken(vendorEmail, defaultPassword);
  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);

  // 1. GET Vendor Profile
  if (vendorToken) {
    const res = await fetch(`${BASE_URL}/api/Vendor/profile`, {
      headers: { 'Authorization': `Bearer ${vendorToken}` }
    });
    const data = await safeParseJson(res);
    logResult('1. GET /api/Vendor/profile (Get Vendor Store Profile)', res, data);
  }

  // 2. PUT Update Vendor Profile
  if (vendorToken) {
    const res = await fetch(`${BASE_URL}/api/Vendor/profile`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${vendorToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        firstName: "UpdatedVendor",
        secondName: "User",
        phoneNumber: "01088887777",
        storeName: `Updated Store ${Date.now()}`
      })
    });
    const data = await safeParseJson(res);
    logResult('2. PUT /api/Vendor/profile (Update Vendor Store Profile)', res, data);
  }

  // 3. GET Admin List Vendors
  if (adminToken) {
    const res = await fetch(`${BASE_URL}/api/Vendor?page=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult('3. GET /api/Vendor (Admin List Vendors)', res, data);
  }
}

if (process.argv[1].endsWith('test-vendor.js')) {
  testVendorController();
}
