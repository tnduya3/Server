// Cookie utility functions
function getCookie(name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return decodeURIComponent(c.substring(nameEQ.length, c.length));
    }
    return null;
}

// // Set cookie utility function
// function setCookie(name, value, days) {
//     let expires = "";
//     if (days) {
//         const date = new Date();
//         date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
//         expires = "; expires=" + date.toUTCString();
//     }
//     document.cookie = name + "=" + encodeURIComponent(value) + expires + "; path=/; SameSite=Strict; Secure";
// }

// Global user data object
let currentUser = null;

// Global variables for chatroom functionality
let allChatrooms = [];
let isDropdownOpen = false;
let selectedChatroom = null;

// Method to decrypt Firebase token and get user information
async function decryptFirebaseToken() {
    try {
        const firebaseToken = getCookie('firebaseToken');
        const userId = getCookie('userId');
        const userDisplayName = getCookie('userDisplayName');
        const userEmail = getCookie('userEmail');
        const userAvatar = getCookie('userAvatar');

        if (!firebaseToken) {
            console.warn('No Firebase token found in cookies');
            redirectToLogin();
            return null;
        }

        // Decode JWT token to get user information
        const tokenPayload = parseJWT(firebaseToken);

        if (!tokenPayload) {
            console.error('Invalid Firebase token');
            redirectToLogin();
            return null;
        }

        // Check if token is expired
        const currentTime = Math.floor(Date.now() / 1000);
        if (tokenPayload.exp && tokenPayload.exp < currentTime) {
            console.warn('Firebase token has expired');
            await refreshToken();
            return null;
        }

        // Set current user data
        currentUser = {
            firebaseToken: firebaseToken,
            userId: userId,
            displayName: userDisplayName,
            email: userEmail,
            avatar: userAvatar,
            tokenPayload: tokenPayload
        };

        console.log('User authenticated successfully:', currentUser);
        updateUIWithUserInfo();
        return currentUser;

    } catch (error) {
        console.error('Error decrypting Firebase token:', error);
        redirectToLogin();
        return null;
    }
}

// Parse JWT token
function parseJWT(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error parsing JWT token:', error);
        return null;
    }
}

// Refresh token if expired
async function refreshToken() {
    try {
        const refreshToken = getCookie('refreshToken');
        if (!refreshToken) {
            redirectToLogin();
            return;
        }

        const response = await fetch('https://localhost:7092/api/Auth/refresh', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                refreshToken: refreshToken
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            // Update cookies with new tokens
            setCookie('firebaseToken', data.accessToken, 7);
            setCookie('refreshToken', data.refreshToken, 30);

            // Retry decryption with new token
            await decryptFirebaseToken();
        } else {
            console.error('Token refresh failed:', data);
            redirectToLogin();
        }
    } catch (error) {
        console.error('Error refreshing token:', error);
        redirectToLogin();
    }
}

// Redirect to login if not authenticated
function redirectToLogin() {
    alert('Please log in to continue');
    window.location.href = '/Server(1)/wwwroot/login.html';
}

// Logout function
function logout() {
    // Clear all authentication cookies
    document.cookie = 'firebaseToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'refreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userId=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userDisplayName=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userEmail=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userAvatar=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';

    // Redirect to login page
    window.location.href = '/Server(1)/wwwroot/login.html';
}

// Update UI with user information
function updateUIWithUserInfo() {
    if (!currentUser) return;

    // Update user fields
    document.getElementById('userId').value = currentUser.userId;
    document.getElementById('username').value = currentUser.displayName;

    // Update chat title with user info
    document.getElementById('chatTitle').textContent = `🤖 ${currentUser.displayName}`;

    // Show user avatar if available
    if (currentUser.avatar) {
        addUserAvatar(currentUser.avatar);
    }

    console.log('UI updated with user information');
}

// Add user avatar to UI
function addUserAvatar(avatarUrl) {
    const chatHeader = document.querySelector('.chat-header');
    const existingAvatar = chatHeader.querySelector('.user-avatar');

    if (existingAvatar) {
        existingAvatar.remove();
    }

    const avatarImg = document.createElement('img');
    avatarImg.src = avatarUrl;
    avatarImg.alt = 'User Avatar';
    avatarImg.className = 'user-avatar';
    avatarImg.style.cssText = `
        width: 40px;
        height: 40px;
        border-radius: 50%;
        margin-left: 10px;
        border: 2px solid white;
        vertical-align: middle;
    `;

    chatHeader.appendChild(avatarImg);
}

// Add logout button to UI
function addLogoutButton() {
    const connectionPanel = document.querySelector('.connection-panel');

    // Check if logout button already exists
    if (connectionPanel.querySelector('.logout-btn')) {
        return;
    }

    const logoutBtn = document.createElement('button');
    logoutBtn.textContent = 'Logout';
    logoutBtn.className = 'logout-btn';
    logoutBtn.onclick = logout;
    logoutBtn.style.cssText = `
        background: #dc3545;
        margin-top: 10px;
        width: 100%;
    `;
    connectionPanel.appendChild(logoutBtn);
}

// Chatroom dropdown functionality
async function loadChatrooms() {
    if (!currentUser || !currentUser.userId) {
        console.error('User not authenticated');
        return;
    }

    const dropdown = document.getElementById('chatroomDropdown');
    if (!dropdown) {
        console.error('Chatroom dropdown not found');
        return;
    }

    dropdown.innerHTML = '<div class="loading-spinner">Loading chatrooms...</div>';
    dropdown.style.display = 'block';

    try {
        const response = await fetch(`https://localhost:7092/api/Chatrooms/user/${currentUser.userId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${currentUser.firebaseToken}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const chatrooms = await response.json();
            allChatrooms = chatrooms;
            displayChatrooms(chatrooms);
            console.log('Chatrooms loaded:', chatrooms);
        } else {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
    } catch (error) {
        console.error('Error loading chatrooms:', error);
        dropdown.innerHTML = '<div class="chatroom-item" style="color: red;">Error loading chatrooms. Click to retry.</div>';
    }
}

function displayChatrooms(chatrooms) {
    const dropdown = document.getElementById('chatroomDropdown');
    if (!dropdown) return;

    if (!chatrooms || chatrooms.length === 0) {
        dropdown.innerHTML = '<div class="chatroom-item">No chatrooms found</div>';
        return;
    }

    dropdown.innerHTML = '';

    chatrooms.forEach(chatroom => {
        const item = document.createElement('div');
        item.className = 'chatroom-item';
        item.onclick = () => selectChatroom(chatroom);

        const isPrivate = chatroom.isPrivate ? '🔒' : '👥';
        const lastActivity = chatroom.lastActivity ?
            new Date(chatroom.lastActivity).toLocaleDateString() : 'No activity';

        item.innerHTML = `
            <div class="chatroom-name">${isPrivate} ${chatroom.name}</div>
            <div class="chatroom-description">${chatroom.description || 'No description'}</div>
            <div class="chatroom-info">ID: ${chatroom.chatRoomId} • Last activity: ${lastActivity}</div>
        `;

        dropdown.appendChild(item);
    });
}

function selectChatroom(chatroom) {
    selectedChatroom = chatroom;
    const selectedInput = document.getElementById('selectedChatroomId');
    const chatroomInput = document.getElementById('chatroomInput');
    const chatTitle = document.getElementById('chatTitle');

    if (selectedInput) selectedInput.value = chatroom.chatRoomId;
    if (chatroomInput) chatroomInput.value = `${chatroom.name} (ID: ${chatroom.chatRoomId})`;
    if (chatTitle) chatTitle.textContent = `${chatroom.name}`;

    // Close dropdown
    toggleChatroomDropdown(false);

    console.log('Selected chatroom:', chatroom);
}

function toggleChatroomDropdown(forceState = null) {
    const dropdown = document.getElementById('chatroomDropdown');
    const input = document.getElementById('chatroomInput');

    if (!dropdown || !input) return;

    if (forceState !== null) {
        isDropdownOpen = forceState;
    } else {
        isDropdownOpen = !isDropdownOpen;
    }

    if (isDropdownOpen) {
        dropdown.style.display = 'block';
        input.removeAttribute('readonly');
        input.focus();

        // Load chatrooms if not loaded yet
        if (allChatrooms.length === 0) {
            loadChatrooms();
        }
    } else {
        dropdown.style.display = 'none';
        input.setAttribute('readonly', 'true');
    }
}

function searchChatrooms(searchTerm) {
    if (!searchTerm || searchTerm.length === 0) {
        displayChatrooms(allChatrooms);
        return;
    }

    const filteredChatrooms = allChatrooms.filter(chatroom =>
        chatroom.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        chatroom.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        chatroom.chatRoomId.toString().includes(searchTerm)
    );

    displayChatrooms(filteredChatrooms);
}

// Helper function to get selected chatroom ID
function getSelectedChatroomId() {
    const selectedInput = document.getElementById('selectedChatroomId');
    return selectedInput ? selectedInput.value : (selectedChatroom?.chatRoomId || null);
}

// Expose functions globally for HTML access
window.toggleChatroomDropdown = toggleChatroomDropdown;
window.searchChatrooms = searchChatrooms;
window.loadChatrooms = loadChatrooms;
window.getCurrentUser = () => currentUser;
window.getSelectedChatroom = () => selectedChatroom;

// Refresh token if expired
async function refreshToken() {
    try {
        const refreshToken = getCookie('refreshToken');
        if (!refreshToken) {
            redirectToLogin();
            return;
        }

        const response = await fetch('/api/Auth/refresh', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                refreshToken: refreshToken
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            // Update cookies with new tokens
            setCookie('firebaseToken', data.accessToken, 7);
            setCookie('refreshToken', data.refreshToken, 30);

            // Retry decryption with new token
            await decryptFirebaseToken();
        } else {
            console.error('Token refresh failed:', data);
            redirectToLogin();
        }
    } catch (error) {
        console.error('Error refreshing token:', error);
        redirectToLogin();
    }
}

// // Set cookie (utility function)
// function setCookie(name, value, days) {
//     let expires = "";
//     if (days) {
//         const date = new Date();
//         date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
//         expires = "; expires=" + date.toUTCString();
//     }
//     document.cookie = name + "=" + encodeURIComponent(value) + expires + "; path=/; SameSite=Strict; Secure";
// }

// Redirect to login if not authenticated
function redirectToLogin() {
    alert('Please log in to continue');
    window.location.href = '/Server(1)/wwwroot/login.html';
}

// Logout function
function logout() {
    // Clear all authentication cookies
    document.cookie = 'firebaseToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'refreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userId=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userDisplayName=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userEmail=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
    document.cookie = 'userAvatar=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';

    // Redirect to login page
    window.location.href = '/Server(1)/wwwroot/login.html';
}

// Initialize authentication when page loads
async function initializeAuth() {
    console.log('Chat client loading...');

    // Decrypt Firebase token and authenticate user
    const user = await decryptFirebaseToken();

    if (user) {
        console.log('User authenticated, ready to chat!');

        // Add logout button to UI
        addLogoutButton();

        // Setup dropdown click outside handler
        document.addEventListener('click', function (event) {
            const dropdown = document.querySelector('.chatroom-dropdown');
            if (dropdown && !dropdown.contains(event.target)) {
                toggleChatroomDropdown(false);
            }
        });

        return true;
    } else {
        console.log('User not authenticated, redirecting to login...');
        return false;
    }
}


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
    const serverUrl = "https://localhost:7092/chathub";
    const userId = document.getElementById('userId').value;
    const username = document.getElementById('username');

    if (!userId) {
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
        
        // Add debugging event handlers
        addConnectionDebugging();

        // Kết nối
        await connection.start();
        console.log("SignalR connection started successfully");

        // Đăng ký user
        await connection.invoke("RegisterUser", userId);
        console.log("User registered successfully with ID:", userId);

        // Call GetAllOnlineUsers after setting up the event handler
        try {
            await connection.invoke("GetAllOnlineUsers");
            console.log("GetAllOnlineUsers invoked successfully");
        } catch (error) {
            console.error("Error calling GetAllOnlineUsers:", error);
        }

        currentUserId = userId;
        updateConnectionStatus(true);
        
        // Add refresh button to online users section
        addRefreshButton();

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

    // Get chatroom ID from dropdown selection or fallback to manual input
    let chatroomId = getSelectedChatroomId();
    if (!chatroomId) {
        // Fallback to manual input if dropdown not used
        const chatroomInput = document.getElementById('chatroomId');
        if (chatroomInput) {
            chatroomId = chatroomInput.value;
        }
    }

    const userId = document.getElementById('userId').value;

    if (!chatroomId) {
        alert('Vui lòng chọn hoặc nhập Chatroom ID!');
        return;
    }

    try {
        await connection.invoke("JoinChatroom", chatroomId, userId);
        currentChatroomId = chatroomId;

        // Update UI with selected chatroom info
        if (selectedChatroom) {
            document.getElementById('chatTitle').textContent = `${selectedChatroom.name}`;
            document.getElementById('chatStatus').textContent = `Đã tham gia phòng: ${selectedChatroom.name} (#${chatroomId})`;
        } else {
            document.getElementById('chatTitle').textContent = `Chat Room #${chatroomId}`;
            document.getElementById('chatStatus').textContent = `Đã tham gia phòng #${chatroomId}`;
        }

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

// Function to display online users in the UI
function displayOnlineUsers(users) {
    const userList = document.getElementById('onlineUsersList');
    if (!userList) {
        console.error("onlineUsersList element not found in HTML");
        return;
    }

    userList.innerHTML = ''; // Clear current list

    if (!users || users.length === 0) {
        userList.innerHTML = `
            <div style="text-align: center; color: #6c757d; font-style: italic; padding: 20px;">
                <div style="font-size: 2rem; margin-bottom: 8px;">👥</div>
                <div>Chưa có ai online</div>
            </div>
        `;
        return;
    }

    // Add a summary at the top
    const summary = document.createElement('div');
    summary.style.cssText = `
        padding: 12px 0;
        font-weight: 600;
        color: #00b894;
        font-size: 14px;
        border-bottom: 2px solid #e9ecef;
        margin-bottom: 16px;
        text-align: center;
    `;
    summary.innerHTML = `
        <div style="font-size: 1.2rem; margin-bottom: 4px;">🟢</div>
        <div>${users.length} user${users.length !== 1 ? 's' : ''} online</div>
    `;
    userList.appendChild(summary);

    // Create a container for the users
    const usersContainer = document.createElement('div');
    usersContainer.className = 'users-container';

    users.forEach(user => {
        const userDiv = document.createElement('div');
        userDiv.className = 'user-item';
        
        // Get user initials for avatar
        const displayName = user.username || `User ${user.UserId}`;
        const initials = displayName.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
        
        const avatar = user.avatar ? 
            `<img src="${user.avatar}" alt="Avatar" class="user-avatar" style="width: 32px; height: 32px; border-radius: 50%; object-fit: cover;">` : 
            `<div class="user-avatar">${initials}</div>`;
        
        const connectionCount = user.connectionCount || 1;
        
        console.log(`Processing user: ID=${user.userId}, Username=${user.username}`);
        
        userDiv.innerHTML = `
            ${avatar}
            <div class="user-info">
                <div class="user-name">${displayName}</div>
                <div class="user-status">ID: ${user.userId} • ${user.onlineStatus}</div>
            </div>
            <div class="online-indicator"></div>
        `;
        
        usersContainer.appendChild(userDiv);
    });

    userList.appendChild(usersContainer);
    console.log(`Displayed ${users.length} online users`);
}

// Add a test function to manually get online users
function getOnlineUsers() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        console.log("Requesting online users...");
        connection.invoke("GetAllOnlineUsers")
            .then(() => {
                console.log("GetAllOnlineUsers called successfully");
                addMessage('System', 'Requested online users list', 'system');
            })
            .catch(err => {
                console.error("Error calling GetAllOnlineUsers:", err);
                addMessage('System', 'Error getting online users: ' + err.message, 'error');
            });
    } else {
        console.log("No connection available or connection not ready");
        addMessage('System', 'No connection available to get online users', 'error');
    }
}

// Add refresh button to online users section
function addRefreshButton() {
    const onlineUsersDiv = document.querySelector('.online-users');
    if (onlineUsersDiv) {
        // Check if button already exists
        if (!onlineUsersDiv.querySelector('.refresh-users-btn')) {
            const refreshBtn = document.createElement('button');
            refreshBtn.textContent = 'Refresh Users';
            refreshBtn.className = 'refresh-users-btn';
            refreshBtn.onclick = getOnlineUsers;
            refreshBtn.style.cssText = `
                background: #007bff;
                color: white;
                border: none;
                padding: 5px 10px;
                border-radius: 4px;
                cursor: pointer;
                margin-top: 10px;
                font-size: 12px;
            `;
            onlineUsersDiv.appendChild(refreshBtn);
        }
    }
}

// Expose the function globally
window.getOnlineUsers = getOnlineUsers;

// Add debugging for connection events
function addConnectionDebugging() {
    if (!connection) return;
    
    // Add event handlers for debugging
    connection.on("OnlineUsers", (data) => {
        console.log("=== OnlineUsers Event Received ===");
        console.log("Raw data:", data);
        console.log("Data type:", typeof data);
        console.log("Data.Users:", data ? data.Users : "data is null/undefined");
        console.log("Data.TotalCount:", data ? data.totalCount : "data is null/undefined");
        console.log("=====================================");
        
        if (data) {
            console.log("Users array:", data.users);
            console.log("Total count:", data.totalCount);
            
            // Display online users in UI
            displayOnlineUsers(data.users);
        } else {
            console.log("No users data received or data is undefined");
            console.log("Data object:", JSON.stringify(data, null, 2));
        }
    });

    connection.on("ReceiveError", (error) => {
        console.error("SignalR Error received:", error);
        addMessage('System', 'Error: ' + error, 'error');
    });

    // Add reconnection debugging
    connection.onreconnecting(() => {
        console.log("SignalR reconnecting...");
        addMessage('System', 'Reconnecting to server...', 'system');
    });

    connection.onreconnected(() => {
        console.log("SignalR reconnected successfully");
        addMessage('System', 'Reconnected to server', 'system');
        // Re-register user and get online users after reconnection
        if (currentUserId) {
            connection.invoke("RegisterUser", currentUserId)
                .then(() => {
                    return connection.invoke("GetAllOnlineUsers");
                })
                .catch(err => {
                    console.error("Error re-registering after reconnection:", err);
                });
        }
    });

    connection.onclose(() => {
        console.log("SignalR connection closed");
        addMessage('System', 'Connection closed', 'system');
    });
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
        addMessage('System', `${data.userId} đã rời phòng chat`, 'system');
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
    });

    // Ping/Pong for connection health
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

// Tự động kết nối khi trang load (để demo)
window.addEventListener('load', async function () {
    // Initialize authentication first
    const isAuthenticated = await initializeAuth();

    if (isAuthenticated) {
        // Uncomment dòng dưới để tự động kết nối
        setTimeout(connect, 1000);
    }
});

// Add debugging functions for connection events
addConnectionDebugging();