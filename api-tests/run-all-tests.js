import { testAccountController } from './test-account.js';
import { testCategoryController } from './test-category.js';
import { testProductController } from './test-product.js';
import { testAdminController } from './test-admin.js';
import { testPermissionController } from './test-permission.js';
import { testOrderController } from './test-order.js';
import { testCustomerController } from './test-customer.js';
import { testVendorController } from './test-vendor.js';
import { testFavoriteController } from './test-favorite.js';
import { testReviewController } from './test-review.js';
import { testNotificationsController } from './test-notifications.js';
import { testStatisticsController } from './test-statistics.js';

async function runAllSuites() {
  console.log('🚀 STARTING VENDORHUB COMPLETE END-TO-END API TEST SUITE');
  console.log('=========================================================\n');
  const startTime = Date.now();

  try {
    await testAccountController();
    await testCategoryController();
    await testProductController();
    await testAdminController();
    await testPermissionController();
    await testOrderController();
    await testCustomerController();
    await testVendorController();
    await testFavoriteController();
    await testReviewController();
    await testNotificationsController();
    await testStatisticsController();

    const duration = ((Date.now() - startTime) / 1000).toFixed(2);
    console.log('\n=========================================================');
    console.log(`🎉 ALL VENDORHUB CONTROLLER API TEST SUITES COMPLETED IN ${duration}s!`);
    console.log('=========================================================\n');
  } catch (error) {
    console.error('\n❌ UNHANDLED TEST EXCEPTION:', error);
  }
}

runAllSuites();
