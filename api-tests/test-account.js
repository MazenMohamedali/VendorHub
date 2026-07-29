import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testAccountController() {
  console.log('\n==================================================');
  console.log('📌 TESTING ACCOUNT CONTROLLER (/api/Account)');
  console.log('==================================================');

  const testEmailCustomer = `testcustomer_${Date.now()}@example.com`;
  const testEmailVendor = `testvendor_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  // 1. POST Register Customer
  {
    const res = await fetch(`${BASE_URL}/api/Account/register/customer`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        firstName: "Test",
        secondName: "Customer",
        email: testEmailCustomer,
        password: defaultPassword,
        confirmPassword: defaultPassword,
        phoneNumber: "01012345678",
        address: "123 Main Street"
      })
    });
    const data = await safeParseJson(res);
    logResult('1. POST /api/Account/register/customer', res, data);
  }

  // 2. POST Register Vendor
  {
    const res = await fetch(`${BASE_URL}/api/Account/register/vendor`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        firstName: "Test",
        secondName: "Vendor",
        email: testEmailVendor,
        password: defaultPassword,
        confirmPassword: defaultPassword,
        phoneNumber: "01098765432",
        storeName: `Test Store ${Date.now()}`
      })
    });
    const data = await safeParseJson(res);
    logResult('2. POST /api/Account/register/vendor', res, data);
  }

  // 3. POST Login Customer
  let customerToken = null;
  {
    const res = await fetch(`${BASE_URL}/api/Account/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: testEmailCustomer, password: defaultPassword })
    });
    const data = await safeParseJson(res);
    const passed = logResult('3. POST /api/Account/login (Customer Login)', res, data);
    if (passed && data?.data) customerToken = data.data;
  }

  // 4. GET /api/Account/me (Customer Session)
  if (customerToken) {
    const res = await fetch(`${BASE_URL}/api/Account/me`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('4. GET /api/Account/me (Customer Session Info)', res, data);
  }

  // 5. POST Change Password
  if (customerToken) {
    const newPassword = 'NewStrongPassword456!';
    const res = await fetch(`${BASE_URL}/api/Account/change-password`, {
      method: 'POST',
      headers: { 
        'Authorization': `Bearer ${customerToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        currentPassword: defaultPassword,
        newPassword: newPassword,
        confirmPassword: newPassword
      })
    });
    const data = await safeParseJson(res);
    logResult('5. POST /api/Account/change-password', res, data);
  }

  // 6. POST Logout
  if (customerToken) {
    const res = await fetch(`${BASE_URL}/api/Account/logout`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('6. POST /api/Account/logout', res, data);
  }

  // 7. Admin Vetting Actions
  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
  if (adminToken) {
    // Approve vendor ID 2 (if exists)
    const approveRes = await fetch(`${BASE_URL}/api/Account/approve-vendor/2`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const approveData = await safeParseJson(approveRes);
    logResult('7. PATCH /api/Account/approve-vendor/2 (Admin Approve Vendor)', approveRes, approveData);
  }
}

if (process.argv[1].endsWith('test-account.js')) {
  testAccountController();
}
