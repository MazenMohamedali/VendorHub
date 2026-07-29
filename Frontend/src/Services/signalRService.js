import * as signalR from "@microsoft/signalr";

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnecting = false;
    this.listeners = [];
  }

  onNotification(callback) {
    this.listeners.push(callback);
    return () => {
      this.listeners = this.listeners.filter(cb => cb !== callback);
    };
  }

  notifyListeners(data) {
    this.listeners.forEach(cb => {
      try {
        cb(data);
      } catch (err) {
        console.error("Error executing notification listener:", err);
      }
    });
  }

  getSignalRUrl() {
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5131';
    const cleanUrl = baseUrl.replace(/\/+$/, "");
    return `${cleanUrl}/notificationHub`;
  }

  async startConnection() {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.isConnecting) {
      return;
    }

    try {
      this.isConnecting = true;
      const token = localStorage.getItem("token");

      if (!token) {
        this.isConnecting = false;
        return;
      }

      const url = this.getSignalRUrl();

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(url, {
          accessTokenFactory: () => token,
          skipNegotiation: false,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      this.connection.onreconnecting((error) => {
        console.log("🔄 SignalR Reconnecting...", error);
      });

      this.connection.onreconnected((connectionId) => {
        console.log("✅ SignalR Reconnected! ID:", connectionId);
      });

      this.connection.onclose((error) => {
        console.log("⏹️ SignalR disconnected", error);
      });

      this.connection.on("ReceiveNotification", (notification) => {
        console.log("📢 SignalR ReceiveNotification:", notification);
        this.handleNotification(notification);
        this.notifyListeners(notification);
      });

      this.connection.on("ReceiveNewPurchaseNotification", (data) => {
        console.log("🛒 SignalR ReceiveNewPurchaseNotification:", data);
        this.handleNotification(data);
        this.notifyListeners(data);
      });

      this.connection.on("ReceiveOrderStatusNotification", (data) => {
        console.log("📦 SignalR ReceiveOrderStatusNotification:", data);
        this.handleNotification(data);
        this.notifyListeners(data);
      });

      await this.connection.start();
      console.log("✅ SignalR connected successfully!");
      this.isConnecting = false;
    } catch (error) {
      console.warn("⚠️ SignalR connection error (server may be offline):", error.message);
      this.isConnecting = false;
    }
  }

  stopConnection() {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
      console.log("⏹️ SignalR connection stopped");
    }
  }

  handleNotification(notification) {
    if (typeof window !== "undefined" && "Notification" in window && Notification.permission === "granted") {
      new Notification(notification.title || notification.Title || "New Notification", {
        body: notification.message || notification.Message || "You have a new update.",
        icon: "/logo.png",
      });
    }
  }

  async sendMessage(method, ...args) {
    try {
      if (this.connection?.state === signalR.HubConnectionState.Connected) {
        await this.connection.invoke(method, ...args);
      }
    } catch (error) {
      console.error("❌ Error sending message:", error);
    }
  }

  isConnected() {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

const signalRService = new SignalRService();
export default signalRService;