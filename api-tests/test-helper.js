// Disable SSL verification for local HTTPS/HTTP development
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

export const BASE_URL = process.env.BASE_URL || 'https://localhost:44342';

// Super Admin Credentials
export const ADMIN_CREDENTIALS = {
  email: "admin@gmail.com",
  password: "P@ssw0rd123!"
};

// Helper to log test status with clean formatting
export function logResult(stepName, response, data) {
  const status = response.status;
  const isOk = response.ok && (data?.success !== false);
  const icon = isOk ? '✅ PASS' : '❌ FAIL';
  
  console.log(`\n${icon} [${status}] ${stepName}`);
  if (data?.message) console.log(`   Message: ${data.message}`);
  if (!isOk) {
    console.log('   Error details:', JSON.stringify(data?.errors || data, null, 2));
  }
  return isOk;
}

// Helper to safely parse JSON responses without breaking on empty bodies
export async function safeParseJson(response) {
  const text = await response.text();
  if (!text) return { message: 'Empty response body' };
  try {
    return JSON.parse(text);
  } catch {
    return { rawResponse: text };
  }
}

// Creates a valid 1x1 PNG image blob so backend image processors don't fail
export function createValidPngBlob() {
  const pngBytes = new Uint8Array([
    137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82,
    0, 0, 0, 1, 0, 0, 0, 1, 8, 6, 0, 0, 0, 31, 213, 196, 200,
    0, 0, 0, 13, 73, 68, 65, 84, 120, 156, 98, 96, 0, 0, 0, 2,
    0, 1, 229, 39, 221, 250, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130
  ]);
  return new Blob([pngBytes], { type: 'image/png' });
}

// Helper to perform login and return token
export async function getAuthToken(email, password) {
  const res = await fetch(`${BASE_URL}/api/Account/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  const data = await safeParseJson(res);
  return data?.data?.token || data?.data?.Token || (typeof data?.data === 'string' ? data.data : null);
}

export function generateEgyptianPhone() {
  const prefixes = ['010', '011', '012', '015'];
  const prefix = prefixes[Math.floor(Math.random() * prefixes.length)];
  const randomDigits = Math.floor(10000000 + Math.random() * 90000000).toString();
  return `${prefix}${randomDigits}`;
}
