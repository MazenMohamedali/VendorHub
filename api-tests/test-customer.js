import { BASE_URL, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testCustomerController() {
  console.log('\n==================================================');
  console.log('📌 TESTING CUSTOMER CONTROLLER (/api/Customer)');
  console.log('==================================================');

  const customerEmail = `cust_prof_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  await fetch(`${BASE_URL}/api/Account/register/customer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Customer",
      secondName: "User",
      email: customerEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: "01012345678",
      address: "Original Address"
    })
  });

  const customerToken = await getAuthToken(customerEmail, defaultPassword);
  if (!customerToken) {
    console.log('⚠️ Could not log in as Customer. Skipping Customer Profile tests.');
    return;
  }

  // 1. GET Customer Profile
  {
    const res = await fetch(`${BASE_URL}/api/Customer/profile`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('1. GET /api/Customer/profile (Get Customer Profile)', res, data);
  }

  // 2. PUT Update Customer Profile
  {
    const res = await fetch(`${BASE_URL}/api/Customer/profile`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${customerToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        firstName: "UpdatedCustomer",
        secondName: "User",
        phoneNumber: "01099998888",
        address: "789 New Oasis Street, Cairo"
      })
    });
    const data = await safeParseJson(res);
    logResult('2. PUT /api/Customer/profile (Update Customer Profile)', res, data);
  }
}

if (process.argv[1].endsWith('test-customer.js')) {
  testCustomerController();
}
