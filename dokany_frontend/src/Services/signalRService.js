// src/services/signalRService.js
import * as signalR from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.onNotificationCallback = null;
  }

  async startConnection(userId, role, onNotificationCallback) {
    if (this.connection && this.isConnected) return;

    this.onNotificationCallback = onNotificationCallback;
    const token = localStorage.getItem('token');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:44342/notificationHub', {
        transport: signalR.HttpTransportType.LongPolling,   // ← Use LongPolling only
        accessTokenFactory: () => token,                   // ← Pass JWT token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      this.isConnected = true;
      console.log('✅ SignalR connected (LongPolling)');

      if (role === 'Vendor') {
        await this.connection.invoke('JoinVendorGroup', userId);
        console.log(`✅ Joined vendor group: vendor-${userId}`);
      } else if (role === 'Customer') {
        await this.connection.invoke('JoinCustomerGroup', userId);
        console.log(`✅ Joined customer group: user-${userId}`);
      }

      this.connection.on('ReceiveNotification', (notification) => {
        console.log('📨 Notification received:', notification);
        if (this.onNotificationCallback) this.onNotificationCallback(notification);
      });
    } catch (err) {
      console.error('❌ SignalR connection failed:', err);
    }
  }

  stopConnection() {
    this.connection?.stop();
    this.isConnected = false;
  }
}

export default new SignalRService();