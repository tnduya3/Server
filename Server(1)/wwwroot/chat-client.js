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

function initializeFirebase() {
    try {
        if (!firebaseConfig) {
            console.error('Firebase config not loaded');
            return false;
        }
        
        firebaseApp = firebase.initializeApp(firebaseConfig);
        messaging = firebase.messaging();
        return true;
    } catch (error) {
        console.error('Error initializing Firebase:', error);
        return false;
    }
}

// Get FCM Device Token
async function getFCMToken() {
    try {
        const permission = await Notification.requestPermission();

        if (permission !== 'granted') {
            console.log('Notification permission denied');
            addMessage('System', 'Notification permission denied. Push notifications will not work.', 'error');
            return null;
        }

        // Register service worker
        const registration = await navigator.serviceWorker.register('/firebase-messaging-sw.js');

        // Get FCM token
        const currentToken = await messaging.getToken({ 
            serviceWorkerRegistration: registration, 
            vapidKey: VAPID_KEY 
        });
        console.log('FCM Token:', currentToken);

        if (currentToken) {
            console.log('FCM Token:', currentToken);
            fcmToken = currentToken;
            return currentToken;
        } else {
            console.log('No FCM device token available');
            addMessage('System', 'No FCM device token available', 'error');
            return null;
        }
    } catch (error) {
        console.error('Error getting FCM token:', error);
        addMessage('System', 'Error getting FCM token: ' + error.message, 'error');
        return null;
    }
}

// Send FCM Token to Server
async function sendFCMTokenToServer(token, userId) {
    try {
        const response = await fetch('https://localhost:7092/api/DeviceToken/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: parseInt(userId),
                token: token
            })
        });

        if (response.ok) {
            const data = await response.json();
            return true;
        } else {
            const errorData = await response.text();
            console.error('Failed to send FCM token to server:', errorData);
            addMessage('System', 'Failed to register FCM token with server', 'error');
            return false;
        }
    } catch (error) {
        console.error('Network error sending FCM token:', error);
        addMessage('System', 'Network error registering FCM token: ' + error.message, 'error');
        return false;
    }
}

// Setup FCM message handlers
function setupFCMHandlers() {
    if (!messaging) return;

    // Handle foreground messages
    messaging.onMessage((payload) => {

        if (payload.notification) {
            // You can also show browser notification
            if (Notification.permission === 'granted') {
                new Notification(payload.notification.title, {
                    body: payload.notification.body,
                    icon: '/Server(1)/favicon.ico'
                });
            }
        }
    });
}

let connection = null;
let currentUserId;
let currentChatroomId = null;
let isTyping = false;
let typingTimeout = null;

// Khởi tạo kết nối SignalR
async function connect() {
    const serverUrl = document.getElementById('serverUrl').value;
    const userId = document.getElementById('userId').value;

    if (!serverUrl || !userId) {
        alert('Vui lòng nhập đầy đủ thông tin kết nối!');
        return;
    }

    try {
        // Load Firebase config từ server trước khi khởi tạo
        if (!firebaseConfig) {
            const configLoaded = await loadFirebaseConfig();
            if (!configLoaded) {
                addMessage('System', 'Failed to load Firebase config, continuing without FCM...', 'error');
            }
        }

        // Initialize Firebase if config is loaded
        if (firebaseConfig && !firebaseApp) {
            if (!initializeFirebase()) {
                addMessage('System', 'Firebase initialization failed, continuing without FCM...', 'error');
            } else {
                setupFCMHandlers();
            }
        }

        // Tạo connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl(serverUrl)
            .withAutomaticReconnect()
            .build();

        // Đăng ký các event handlers
        setupEventHandlers();

        // Kết nối
        await connection.start();

        // Đăng ký user
        await connection.invoke("RegisterUser", userId);

        currentUserId = userId;
        updateConnectionStatus(true);

        // Get and send FCM token after successful SignalR connection
        if (messaging) {
            const token = await getFCMToken();

            if (token) {
                await sendFCMTokenToServer(token, userId);
            }
        }

    } catch (err) {
        console.error('Lỗi kết nối:', err);
        updateConnectionStatus(false, err.message);
        addMessage('System', 'Connection failed: ' + err.message, 'error');
    }
}

// Ngắt kết nối
async function disconnect() {
    if (connection) {
        await connection.stop();
        connection = null;
        currentUserId = null;
        updateConnectionStatus(false);
        console.log('Đã ngắt kết nối');
    }
}

// Tham gia phòng chat
async function joinRoom() {
    if (!connection) {
        alert('Chưa kết nối tới server!');
        return;
    }

    const chatroomId = document.getElementById('chatroomId').value;
    const userId = document.getElementById('userId').value;

    if (!chatroomId) {
        alert('Vui lòng nhập Chatroom ID!');
        return;
    }

    try {
        await connection.invoke("JoinChatroom", chatroomId, userId);
        currentChatroomId = chatroomId;
        document.getElementById('chatTitle').textContent = `Chat Room #${chatroomId}`;
        document.getElementById('chatStatus').textContent = `Đã tham gia phòng #${chatroomId}`;

        // Enable message input
        document.getElementById('messageInput').disabled = false;
        document.getElementById('sendBtn').disabled = false;
        document.getElementById('leaveRoomBtn').disabled = false;
        document.getElementById('joinRoomBtn').disabled = true;

    } catch (err) {
        console.error('Lỗi tham gia phòng:', err);
        addMessage('System', 'Lỗi tham gia phòng: ' + err.message, 'error');
    }
}

// Rời phòng chat
async function leaveRoom() {
    if (!connection || !currentChatroomId) return;

    try {
        await connection.invoke("LeaveChatroom", currentChatroomId, currentUserId);
        currentChatroomId = null;

        document.getElementById('chatStatus').textContent = 'Đã rời phòng chat';
        document.getElementById('messageInput').disabled = true;
        document.getElementById('sendBtn').disabled = true;
        document.getElementById('leaveRoomBtn').disabled = true;
        document.getElementById('joinRoomBtn').disabled = false;

    } catch (err) {
        console.error('Lỗi rời phòng:', err);
    }
}

// Gửi tin nhắn
async function sendMessage() {
    const messageInput = document.getElementById('messageInput');
    const message = messageInput.value.trim();

    if (!message || !connection || !currentChatroomId) return;

    try {
        await connection.invoke("SendMessage",
            parseInt(currentUserId),
            parseInt(currentChatroomId),
            message
        );

        messageInput.value = '';

    } catch (err) {
        console.error('Lỗi gửi tin nhắn:', err);
        addMessage('System', 'Lỗi gửi tin nhắn: ' + err.message, 'error');
    }
}

// Xử lý typing
function handleTyping() {
    if (!isTyping && connection && currentChatroomId) {
        isTyping = true;
        const username = document.getElementById('username').value;

        connection.invoke("SendTyping", parseInt(currentUserId), parseInt(currentChatroomId), username)
            .then(() => {
                console.log('SendTyping successfully invoked');
            })
            .catch(err => {
                console.error('Error sending typing indicator:', err);
                addMessage('System', 'Error sending typing: ' + err.message, 'error');
            });
    }

    clearTimeout(typingTimeout);
    typingTimeout = setTimeout(stopTyping, 2000);
}

function stopTyping() {
    if (isTyping && connection && currentChatroomId) {
        isTyping = false;

        connection.invoke("StopTyping", parseInt(currentUserId), parseInt(currentChatroomId))
            .then(() => {
                console.log('StopTyping successfully invoked');
            })
            .catch(err => {
                console.error('Error sending stop typing indicator:', err);
                addMessage('System', 'Error stopping typing: ' + err.message, 'error');
            });
    }
}

// Setup event handlers cho SignalR
function setupEventHandlers() {

    // Nhận tin nhắn
    connection.on("ReceiveMessage", function (messageData) {
        const isOwn = messageData.senderId == currentUserId;
        addMessage(
            messageData.senderName,
            messageData.content,
            isOwn ? 'sent' : 'received',
            messageData.CreatedAt
        );

        // Lưu ID tin nhắn cuối cùng để test mark as read
        lastMessageId = messageData.MessageId;
    });

    connection.on("ReceiveChatroomUsers", function (users) {
        const userList = document.getElementById('onlineUsersList');
        userList.innerHTML = ''; // Clear current list

        if (users.length === 0) {
            userList.textContent = 'Chưa có ai online';
            return;
        }

        users.forEach(user => {
            const userItem = document.createElement('div');
            userItem.textContent = `${user.UserId} - ${user.Username}`;
            userList.appendChild(userItem);
        });
    });

    connection.on("ReceiveNotification", function (notification) {
        // addMessage('System', notification.Message, 'system');
    });

    // Thông báo kết nối thành công
    connection.on("Connected", function (data) {
        addMessage('System', `Kết nối thành công! Connection ID: ${data.Message}`, 'system');
    });

    // Xác nhận tham gia phòng
    connection.on("JoinConfirmation", function (data) {
        // addMessage('System', data.Message, 'system');
    });

    // Xác nhận rời phòng
    connection.on("LeaveConfirmation", function (data) {
        // addMessage('System', data.Message, 'system');
    });

    // User tham gia phòng
    connection.on("UserJoinedChatroom", function (data) {
        addMessage('System', `${data.userId} đã tham gia phòng chat`, 'system');
    });

    // User rời phòng
    connection.on("UserLeftChatroom", function (data) {
        addMessage('System', `${data.Username} đã rời phòng chat`, 'system');
    });
    // Typing indicators
    connection.on("UserTyping", function (data) {
        showTypingIndicator(`${data.senderName} đang nhập...`);
    });

    connection.on("UserStoppedTyping", function (data) {
        hideTypingIndicator();
    });

    // Tin nhắn đã được chỉnh sửa
    connection.on("MessageEdited", function (data) {
        addMessage('System', `Tin nhắn #${data.MessageId} đã được chỉnh sửa`, 'system');
    });

    // Tin nhắn đã được xóa
    connection.on("MessageDeleted", function (data) {
        addMessage('System', `Tin nhắn #${data.MessageId} đã được xóa`, 'system');
    });

    // Lỗi
    connection.on("ReceiveError", function (error) {
        addMessage('System', `Lỗi: ${error.Message || error}`, 'error');
    });

    // User online/offline
    connection.on("UserOnline", function (userId) {
        console.log(`User ${userId} đã online`);
    });

    connection.on("UserOffline", function (userId) {
        console.log(`User ${userId} đã offline`);
    });

    // Message read receipts
    connection.on("MessageRead", function (data) {
        console.log(`Tin nhắn #${data.MessageId} đã được đọc bởi User ${data.ReadBy}`);
    });

    // Broadcast notifications
    connection.on("BroadcastNotification", function (data) {
        addMessage('📢 Thông báo', data.Body, 'system');
    });            // Ping/Pong for connection health
    connection.on("Pong", function (timestamp) {
        console.log('Pong received at:', timestamp);
        addMessage('System', `Pong received at ${new Date(timestamp).toLocaleTimeString()}`, 'system');
    });

    // Online users in chatroom response
    connection.on("OnlineUsersInChatroom", function (data) {
        const userList = document.getElementById('onlineUsersList');
        userList.innerHTML = '';

        if (data.OnlineUsers.length === 0) {
            userList.textContent = 'Chưa có ai online';
        } else {
            data.OnlineUsers.forEach(user => {
                const userItem = document.createElement('div');
                userItem.textContent = `${user.UserId} - ${user.Username}`;
                userList.appendChild(userItem);
            });
        }

        addMessage('System', `${data.Count} users online in chatroom`, 'system');
    });
}

// Utility functions
function updateConnectionStatus(connected, error = null) {
    const statusEl = document.getElementById('connectionStatus');
    const connectBtn = document.getElementById('connectBtn');
    const disconnectBtn = document.getElementById('disconnectBtn');
    const joinRoomBtn = document.getElementById('joinRoomBtn');

    if (connected) {
        statusEl.textContent = `Đã kết nối (User ID: ${currentUserId})`;
        statusEl.className = 'status connected';
        connectBtn.disabled = true;
        disconnectBtn.disabled = false;
        joinRoomBtn.disabled = false;
    } else {
        statusEl.textContent = error ? `Lỗi kết nối: ${error}` : 'Chưa kết nối';
        statusEl.className = error ? 'status error' : 'status disconnected';
        connectBtn.disabled = false;
        disconnectBtn.disabled = true;
        joinRoomBtn.disabled = true;
        document.getElementById('leaveRoomBtn').disabled = true;
        document.getElementById('messageInput').disabled = true;
        document.getElementById('sendBtn').disabled = true;
    }
}

function addMessage(sender, message, type = 'received', timestamp = null) {
    const container = document.getElementById('messagesContainer');
    const messageEl = document.createElement('div');
    messageEl.className = `message ${type}`;

    const time = timestamp ? new Date(timestamp).toLocaleTimeString() : new Date().toLocaleTimeString();

    messageEl.innerHTML = `
                <div><strong>${sender}:</strong> ${message}</div>
                <div class="message-info">${time}</div>
            `;

    container.appendChild(messageEl);
    container.scrollTop = container.scrollHeight;
}

function showTypingIndicator(text) {
    const indicator = document.getElementById('typingIndicator');
    const textEl = document.getElementById('typingText');
    textEl.textContent = text;
    indicator.style.display = 'block';
}

function hideTypingIndicator() {
    document.getElementById('typingIndicator').style.display = 'none';
}

// Event listeners
document.getElementById('messageInput').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        sendMessage();
    } else {
        handleTyping();
    }
});

// Thêm event listener cho input để bắt typing
document.getElementById('messageInput').addEventListener('input', function (e) {
    handleTyping();
});

// Test functions
async function testPing() {
    if (connection) {
        try {
            await connection.invoke("Ping");
            addMessage('System', 'Ping sent to server', 'system');
        } catch (err) {
            addMessage('System', 'Ping failed: ' + err.message, 'error');
        }
    }
}

let lastMessageId = null;
async function markMessageAsRead() {
    if (connection && currentChatroomId && lastMessageId) {
        try {
            await connection.invoke("MarkMessageAsRead", lastMessageId, parseInt(currentUserId), parseInt(currentChatroomId));
            addMessage('System', `Marked message #${lastMessageId} as read`, 'system');
        } catch (err) {
            addMessage('System', 'Mark as read failed: ' + err.message, 'error');
        }
    } else {
        addMessage('System', 'No message to mark as read', 'system');
    }
}

async function getOnlineUsers() {
    if (connection && currentChatroomId) {
        try {
            await connection.invoke("GetOnlineUsersInChatroom", currentChatroomId);
            addMessage('System', 'Requested online users list', 'system');
        } catch (err) {
            addMessage('System', 'Get online users failed: ' + err.message, 'error');
        }
    }
}

// Test functions cho typing
async function testTypingMethod() {
    if (connection) {
        try {
            await connection.invoke("TestTyping");
            addMessage('System', 'TestTyping method called', 'system');
        } catch (err) {
            addMessage('System', 'TestTyping failed: ' + err.message, 'error');
        }
    } else {
        addMessage('System', 'Not connected', 'error');
    }
}

async function testSendTyping() {
    if (connection && currentChatroomId) {
        try {
            const username = document.getElementById('username').value;
            console.log('Testing SendTyping with params:', parseInt(currentUserId), parseInt(currentChatroomId), username);

            await connection.invoke("SendTyping", parseInt(currentUserId), parseInt(currentChatroomId), username);
            addMessage('System', 'SendTyping test called successfully', 'system');
        } catch (err) {
            console.error('SendTyping test error:', err);
            addMessage('System', 'SendTyping test failed: ' + err.message, 'error');
        }
    } else {
        addMessage('System', 'Not connected or not in chatroom', 'error');
    }
}

// Test function cho FCM
async function testFCMToken() {
    if (!messaging) {
        addMessage('System', 'Firebase not initialized', 'error');
        return;
    }

    addMessage('System', 'Testing FCM token retrieval...', 'system');
    const token = await getFCMToken();

    if (token && currentUserId) {
        addMessage('System', `FCM Token: ${token.substring(0, 50)}...`, 'system');
        await sendFCMTokenToServer(token, currentUserId);
    }
}

// Test function cho Firebase config
async function testFirebaseConfig() {
    addMessage('System', 'Testing Firebase config loading...', 'system');
    
    const configLoaded = await loadFirebaseConfig();
    
    if (configLoaded && firebaseConfig) {
        addMessage('System', `Firebase config loaded successfully!`, 'system');
        addMessage('System', `Project ID: ${firebaseConfig.projectId}`, 'system');
        addMessage('System', `VAPID Key: ${VAPID_KEY ? VAPID_KEY.substring(0, 20) + '...' : 'Not found'}`, 'system');
        
        // Thử khởi tạo Firebase
        if (!firebaseApp) {
            if (initializeFirebase()) {
                addMessage('System', 'Firebase initialized successfully!', 'system');
                setupFCMHandlers();
            } else {
                addMessage('System', 'Firebase initialization failed!', 'error');
            }
        } else {
            addMessage('System', 'Firebase already initialized', 'system');
        }
    } else {
        addMessage('System', 'Failed to load Firebase config', 'error');
    }
}

// Tự động kết nối khi trang load (để demo)
window.addEventListener('load', function () {
    // Uncomment dòng dưới để tự động kết nối
    // setTimeout(connect, 1000);
});