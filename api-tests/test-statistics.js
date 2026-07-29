import { BASE_URL, ADMIN_CREDENTIALS, logResult, safeParseJson, getAuthToken } from './test-helper.js';

export async function testStatisticsController() {
  console.log('\n==================================================');
  console.log('📌 TESTING STATISTICS CONTROLLER (/api/Statistics)');
  console.log('==================================================');

  const adminToken = await getAuthToken(ADMIN_CREDENTIALS.email, ADMIN_CREDENTIALS.password);
  if (!adminToken) {
    console.log('⚠️ Could not log in as Admin. Skipping Statistics tests.');
    return;
  }

  const targetVendorId = 2;

  // 1. GET Vendor Statistics
  {
    const res = await fetch(`${BASE_URL}/api/Statistics/vendor/${targetVendorId}`, {
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const data = await safeParseJson(res);
    logResult(`1. GET /api/Statistics/vendor/${targetVendorId} (Vendor Analytics & Store Metrics)`, res, data);
  }
}

if (process.argv[1].endsWith('test-statistics.js')) {
  testStatisticsController();
}
