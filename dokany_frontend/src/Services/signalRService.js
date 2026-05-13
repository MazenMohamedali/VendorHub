// import * as signalR from '@microsoft/signalr';

// class SignalRService {
//   constructor() {
//     this.connection = null;
//     this.isConnected = false;
//   }

//   async startConnection(userId, role, onNewOrderCallback) {
//     if (this.connection && this.isConnected) return;

//     this.connection = new signalR.HubConnectionBuilder()
//       .withUrl('https://localhost:44342/notificationHub') // same as backend
//       .withAutomaticReconnect()
//       .configureLogging(signalR.LogLevel.Information)
//       .build();

//     try {
//       await this.connection.start();
//       this.isConnected = true;
//       console.log('SignalR connected');

//       // Join user-specific group (e.g., vendor-{vendorId})
//       if (role === 'Vendor') {
//         await this.connection.invoke('JoinVendorGroup', userId);
//         console.log(`Joined vendor group: vendor-${userId}`);
//       } else if (role === 'Customer') {
//         await this.connection.invoke('JoinCustomerGroup', userId);
//         console.log(`Joined customer group: user-${userId}`);
//       }

//       // Listen for 'ReceiveNotification' event
//       this.connection.on('ReceiveNotification', (notification) => {
//         console.log('New notification:', notification);
//         if (onNewOrderCallback) onNewOrderCallback(notification);
//       });
//     } catch (err) {
//       console.error('SignalR connection failed:', err);
//     }
//   }

//   stopConnection() {
//     if (this.connection) {
//       this.connection.stop();
//       this.isConnected = false;
//     }
//   }
// }

// export default new SignalRService();


import * as signalR from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.onNotificationCallback = null;
  }

  async startConnection(userId, role, onNotificationCallback) {
    if (this.connection && this.isConnected) {
      console.log('SignalR already connected');
      return;
    }

    this.onNotificationCallback = onNotificationCallback;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:44342/notificationHub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      this.isConnected = true;
      console.log('✅ SignalR connected successfully');

      // Join appropriate group based on role
      if (role === 'Vendor') {
        await this.connection.invoke('JoinVendorGroup', userId);
        console.log(`✅ Joined vendor group: vendor-${userId}`);
      } else if (role === 'Customer') {
        await this.connection.invoke('JoinCustomerGroup', userId);
        console.log(`✅ Joined customer group: user-${userId}`);
      }

      // Listen for 'ReceiveNotification' event
      this.connection.on('ReceiveNotification', (notification) => {
        console.log('📨 SignalR ReceiveNotification:', notification);
        if (this.onNotificationCallback) {
          this.onNotificationCallback(notification);
        }
      });

      // Listen for 'OrderStatusChanged' event (for customers)
      this.connection.on('OrderStatusChanged', (statusUpdate) => {
        console.log('📨 OrderStatusChanged:', statusUpdate);
        if (this.onNotificationCallback) {
          this.onNotificationCallback({ ...statusUpdate, type: 'OrderStatusChanged' });
        }
      });

    } catch (err) {
      console.error('❌ SignalR connection failed:', err);
      this.isConnected = false;
    }

    // Handle reconnection
    this.connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting:', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      // Re-join group after reconnection
      if (role === 'Vendor') {
        this.connection.invoke('JoinVendorGroup', userId);
      } else if (role === 'Customer') {
        this.connection.invoke('JoinCustomerGroup', userId);
      }
    });
  }

  stopConnection() {
    if (this.connection) {
      this.connection.stop();
      this.isConnected = false;
      console.log('SignalR connection stopped');
    }
  }

  // Method to manually send a test notification (for debugging)
  async sendTestNotification(vendorId, message) {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('SendTestNotification', vendorId, message);
    }
  }
}

export default new SignalRService();