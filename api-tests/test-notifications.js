import { BASE_URL, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testNotificationsController() {
  console.log('\n==================================================');
  console.log('📌 TESTING NOTIFICATIONS CONTROLLER (/api/Notifications)');
  console.log('==================================================');

  const customerEmail = `notif_cust_${Date.now()}@example.com`;
  const defaultPassword = 'StrongPassword123!';

  await fetch(`${BASE_URL}/api/Account/register/customer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      firstName: "Notif",
      secondName: "User",
      email: customerEmail,
      password: defaultPassword,
      confirmPassword: defaultPassword,
      phoneNumber: "01012345678",
      address: "Notif Address"
    })
  });

  const customerToken = await getAuthToken(customerEmail, defaultPassword);
  if (!customerToken) {
    console.log('⚠️ Could not log in as Customer. Skipping Notifications tests.');
    return;
  }

  let notificationId = null;

  // 1. GET Notification History
  {
    const res = await fetch(`${BASE_URL}/api/Notifications?pageNumber=1&pageSize=10`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    const passed = logResult('1. GET /api/Notifications (User Notification History)', res, data);
    if (passed && data?.data?.items?.length > 0) {
      notificationId = data.data.items[0].id;
    }
  }

  // 2. GET Unread Notifications
  {
    const res = await fetch(`${BASE_URL}/api/Notifications/unread`, {
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('2. GET /api/Notifications/unread (Unread Notifications)', res, data);
  }

  // 3. PUT Mark All Read
  {
    const res = await fetch(`${BASE_URL}/api/Notifications/mark-all-read`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult('3. PUT /api/Notifications/mark-all-read (Mark All Read)', res, data);
  }

  // 4. PUT Mark Single Read
  if (notificationId) {
    const res = await fetch(`${BASE_URL}/api/Notifications/${notificationId}/mark-read`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`4. PUT /api/Notifications/${notificationId}/mark-read (Mark Single Read)`, res, data);
  }

  // 5. DELETE Notification
  if (notificationId) {
    const res = await fetch(`${BASE_URL}/api/Notifications/${notificationId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${customerToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`5. DELETE /api/Notifications/${notificationId} (Delete Notification)`, res, data);
  }
}

if (process.argv[1].endsWith('test-notifications.js')) {
  testNotificationsController();
}
