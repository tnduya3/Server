// SignalR Chat Client Example
// Sử dụng file này để tích hợp vào ứng dụng web/mobile của bạn

class ChatClient {
    constructor(serverUrl, options = {}) {
        this.serverUrl = serverUrl;
        this.connection = null;
        this.isConnected = false;
        this.currentUserId = null;
        this.currentChatroomId = null;
        this.options = {
            autoReconnect: true,
            logLevel: signalR.LogLevel.Information,
            ...options
        };

        // Event callbacks
        this.onConnected = null;
        this.onDisconnected = null;
        this.onMessageReceived = null;
        this.onUserTyping = null;
        this.onUserStoppedTyping = null;
        this.onUserJoinedChatroom = null;
        this.onUserLeftChatroom = null;
        this.onError = null;
    }

    // Kết nối đến SignalR Hub
    async connect(userId, username) {
        try {
            // Tạo connection builder
            const connectionBuilder = new signalR.HubConnectionBuilder()
                .withUrl(this.serverUrl)
                .configureLogging(this.options.logLevel);

            if (this.options.autoReconnect) {
                connectionBuilder.withAutomaticReconnect();
            }

            this.connection = connectionBuilder.build();

            // Đăng ký event handlers
            this._setupEventHandlers();

            // Kết nối
            await this.connection.start();
            this.isConnected = true;
            this.currentUserId = userId;

            // Đăng ký user với hub
            await this.connection.invoke("RegisterUser", userId.toString());

            console.log('Connected to SignalR Hub');
            if (this.onConnected) {
                this.onConnected();
            }

            return true;
        } catch (error) {
            console.error('Failed to connect:', error);
            if (this.onError) {
                this.onError('Connection failed', error);
            }
            return false;
        }
    }

    // Ngắt kết nối
    async disconnect() {
        if (this.connection && this.isConnected) {
            await this.connection.stop();
            this.isConnected = false;
            this.currentUserId = null;
            this.currentChatroomId = null;
            console.log('Disconnected from SignalR Hub');
            
            if (this.onDisconnected) {
                this.onDisconnected();
            }
        }
    }

    // Tham gia phòng chat
    async joinChatroom(chatroomId) {
        if (!this._checkConnection()) return false;

        try {
            await this.connection.invoke("JoinChatroom");
            this.currentChatroomId = chatroomId;
            console.log(`Joined chatroom: ${chatroomId}`);
            return true;
        } catch (error) {
            console.error('Failed to join chatroom:', error);
            if (this.onError) {
                this.onError('Join chatroom failed', error);
            }
            return false;
        }
    }

    // Rời phòng chat
    async leaveChatroom() {
        if (!this._checkConnection() || !this.currentChatroomId) return false;

        try {
            await this.connection.invoke("LeaveChatroom", this.currentChatroomId.toString(), this.currentUserId.toString());
            const leftChatroomId = this.currentChatroomId;
            this.currentChatroomId = null;
            console.log(`Left chatroom: ${leftChatroomId}`);
            return true;
        } catch (error) {
            console.error('Failed to leave chatroom:', error);
            if (this.onError) {
                this.onError('Leave chatroom failed', error);
            }
            return false;
        }
    }

    // Gửi tin nhắn
    async sendMessage(message) {
        if (!this._checkConnection() || !this.currentChatroomId) {
            console.error('Not connected or not in a chatroom');
            return false;
        }

        try {
            await this.connection.invoke("SendMessage", 
                parseInt(this.currentUserId), 
                parseInt(this.currentChatroomId), 
                message
            );
            return true;
        } catch (error) {
            console.error('Failed to send message:', error);
            if (this.onError) {
                this.onError('Send message failed', error);
            }
            return false;
        }
    }

    // Gửi typing indicator
    async sendTyping(senderName) {
        if (!this._checkConnection() || !this.currentChatroomId) return false;

        try {
            await this.connection.invoke("SendTyping", 
                parseInt(this.currentUserId), 
                parseInt(this.currentChatroomId), 
                senderName
            );
            return true;
        } catch (error) {
            console.error('Failed to send typing:', error);
            return false;
        }
    }

    // Ngừng typing
    async stopTyping() {
        if (!this._checkConnection() || !this.currentChatroomId) return false;

        try {
            await this.connection.invoke("StopTyping", 
                parseInt(this.currentUserId), 
                parseInt(this.currentChatroomId)
            );
            return true;
        } catch (error) {
            console.error('Failed to stop typing:', error);
            return false;
        }
    }

    // Đánh dấu tin nhắn đã đọc
    async markMessageAsRead(messageId) {
        if (!this._checkConnection() || !this.currentChatroomId) return false;

        try {
            await this.connection.invoke("MarkMessageAsRead", 
                parseInt(messageId), 
                parseInt(this.currentUserId), 
                parseInt(this.currentChatroomId)
            );
            return true;
        } catch (error) {
            console.error('Failed to mark message as read:', error);
            return false;
        }
    }

    // Ping server để kiểm tra kết nối
    async ping() {
        if (!this._checkConnection()) return false;

        try {
            await this.connection.invoke("Ping");
            return true;
        } catch (error) {
            console.error('Ping failed:', error);
            return false;
        }
    }

    // Private methods
    _checkConnection() {
        if (!this.connection || !this.isConnected) {
            console.error('Not connected to SignalR Hub');
            return false;
        }
        return true;
    }

    _setupEventHandlers() {
        // Nhận tin nhắn
        this.connection.on("ReceiveMessage", (messageData) => {
            console.log('Message received:', messageData);
            if (this.onMessageReceived) {
                this.onMessageReceived(messageData);
            }
        });

        // Kết nối thành công
        this.connection.on("Connected", (data) => {
            console.log('Connection confirmed:', data);
        });

        // Xác nhận tham gia phòng
        this.connection.on("JoinConfirmation", (data) => {
            console.log('Join confirmation:', data);
        });

        // Xác nhận rời phòng
        this.connection.on("LeaveConfirmation", (data) => {
            console.log('Leave confirmation:', data);
        });

        // User tham gia phòng
        this.connection.on("UserJoinedChatroom", (data) => {
            console.log('User joined:', data);
            if (this.onUserJoinedChatroom) {
                this.onUserJoinedChatroom(data);
            }
        });

        // User rời phòng
        this.connection.on("UserLeftChatroom", (data) => {
            console.log('User left:', data);
            if (this.onUserLeftChatroom) {
                this.onUserLeftChatroom(data);
            }
        });

        // Typing indicators
        this.connection.on("UserTyping", (data) => {
            console.log('User typing:', data);
            if (this.onUserTyping) {
                this.onUserTyping(data);
            }
        });

        this.connection.on("UserStoppedTyping", (data) => {
            console.log('User stopped typing:', data);
            if (this.onUserStoppedTyping) {
                this.onUserStoppedTyping(data);
            }
        });

        // Tin nhắn đã đọc
        this.connection.on("MessageRead", (data) => {
            console.log('Message read:', data);
        });

        // Tin nhắn đã chỉnh sửa
        this.connection.on("MessageEdited", (data) => {
            console.log('Message edited:', data);
        });

        // Tin nhắn đã xóa
        this.connection.on("MessageDeleted", (data) => {
            console.log('Message deleted:', data);
        });

        // Lỗi
        this.connection.on("ReceiveError", (error) => {
            console.error('SignalR Error:', error);
            if (this.onError) {
                this.onError('SignalR Error', error);
            }
        });

        // Xác nhận tin nhắn đã gửi
        this.connection.on("MessageSent", (data) => {
            console.log('Message sent confirmation:', data);
        });

        // User online/offline
        this.connection.on("UserOnline", (userId) => {
            console.log(`User ${userId} is online`);
        });

        this.connection.on("UserOffline", (userId) => {
            console.log(`User ${userId} is offline`);
        });

        // Pong response
        this.connection.on("Pong", (timestamp) => {
            console.log('Pong received at:', timestamp);
        });

        // Connection state changes
        this.connection.onreconnecting(() => {
            console.log('Reconnecting...');
            this.isConnected = false;
        });

        this.connection.onreconnected(() => {
            console.log('Reconnected!');
            this.isConnected = true;
            // Re-register user and rejoin chatroom if needed
            if (this.currentUserId) {
                this.connection.invoke("RegisterUser", this.currentUserId.toString());
                if (this.currentChatroomId) {
                    this.connection.invoke("JoinChatroom", this.currentChatroomId.toString(), this.currentUserId.toString());
                }
            }
        });

        this.connection.onclose(() => {
            console.log('Connection closed');
            this.isConnected = false;
            if (this.onDisconnected) {
                this.onDisconnected();
            }
        });
    }
}

// Export cho sử dụng
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ChatClient;
}

// Usage example:
/*
const chatClient = new ChatClient('https://localhost:7092/chathub');

// Set up event handlers
chatClient.onMessageReceived = (messageData) => {
    console.log('New message:', messageData);
    // Update UI with new message
};

chatClient.onUserTyping = (data) => {
    console.log(`${data.SenderName} is typing...`);
    // Show typing indicator
};

chatClient.onError = (type, error) => {
    console.error(`${type}:`, error);
    // Handle error
};

// Connect and join chatroom
async function startChat() {
    await chatClient.connect(1, 'User1');
    await chatClient.joinChatroom(1);
    
    // Send a message
    await chatClient.sendMessage('Hello everyone!');
}

startChat();
*/
