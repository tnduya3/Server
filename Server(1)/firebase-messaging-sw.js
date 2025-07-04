// firebase-messaging-sw.js
// Đây là Service Worker cần thiết cho Firebase Cloud Messaging trên Web.
// Nó phải nằm ở thư mục gốc của domain để hoạt động đúng.

importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging-compat.js');

// Thay thế bằng cấu hình Firebase của dự án của bạn
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
// Firebase Configuration - sẽ được load từ server
let firebaseConfig = null;
let VAPID_KEY = null;

// Initialize Firebase
let firebaseApp = null;
let messaging = null;
let fcmToken = null;

// Load Firebase config từ server
async function loadFirebaseConfig() {
  try {
    const response = await fetch('https://localhost:7092/api/config/firebase');
    if (response.ok) {
      const config = await response.json();
      firebaseConfig = {
        apiKey: config.apiKey,
        authDomain: config.authDomain,
        projectId: config.projectId,
        storageBucket: config.storageBucket,
        messagingSenderId: config.messagingSenderId,
        appId: config.appId,
        measurementId: config.measurementId
      };
      VAPID_KEY = config.vapidKey;
      console.log('Firebase config loaded from server');
      return true;
    } else {
      console.error('Failed to load Firebase config from server');
      return false;
    }
  } catch (error) {
    console.error('Error loading Firebase config:', error);
    return false;
  }
}


// Initialize Firebase
function initializeFirebase() {
  try {
    if (!firebaseConfig) {
      console.error('Firebase config not loaded');
      return false;
    }

    firebaseApp = firebase.initializeApp(firebaseConfig);
    messaging = firebase.messaging();
    console.log('Firebase initialized successfully');
    return true;
  } catch (error) {
    console.error('Error initializing Firebase:', error);
    return false;
  }
}

// Xử lý thông báo nền (background messages)
messaging.onBackgroundMessage((payload) => {
  console.log('[firebase-messaging-sw.js] Received background message ', payload);
  // Customize notification here
  const notificationTitle = payload.notification.title;
  const notificationOptions = {
    body: payload.notification.body,
    icon: '/favicon.ico' // Icon cho thông báo
    // Thêm các tùy chọn khác như actions, data, v.v.
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});

console.log('[firebase-messaging-sw.js] Service Worker registered and running.');