// Notification Page JavaScript

let allNotifications = [];
let currentFilter = 'all';

$(document).ready(function () {
    console.log('Notification Page Initialized');

    // Load notifications on page load
    loadNotifications();

    // Tab change handler
    $('#notificationTabs button').on('click', function (e) {
        currentFilter = $(this).attr('id').replace('-tab', '');
        renderNotifications();
    });

    // Mark all as read
    $('#btnMarkAllRead').on('click', function () {
        markAllAsRead();
    });

    // Delete all notifications
    $('#btnDeleteAll').on('click', function () {
        deleteAllNotifications();
    });

    // Refresh every 30 seconds
    setInterval(loadNotifications, 30000);
});

// Load notifications from API
function loadNotifications() {
    $.ajax({
        url: '/api/notifications',
        type: 'GET',
        success: function (response) {
            console.log('Notifications loaded:', response);
            if (response.success && response.data) {
                allNotifications = response.data;
                renderNotifications();
                updateBadges();
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading notifications:', error);
            showError('Failed to load notifications');
        }
    });
}

// Render notifications based on current filter
function renderNotifications() {
    let filteredNotifications = allNotifications;

    if (currentFilter === 'unread') {
        filteredNotifications = allNotifications.filter(n => !n.isRead);
    } else if (currentFilter === 'read') {
        filteredNotifications = allNotifications.filter(n => n.isRead);
    }

    const containerId = currentFilter === 'all' ? '#allNotifications' : 
                        currentFilter === 'unread' ? '#unreadNotifications' : 
                        '#readNotifications';

    const container = $(containerId);
    container.empty();

    if (filteredNotifications.length === 0) {
        container.html(`
            <div class="text-center py-5">
                <i class="feather-bell text-muted" style="font-size: 48px;"></i>
                <p class="text-muted mt-3">No ${currentFilter === 'all' ? '' : currentFilter} notifications</p>
            </div>
        `);
        return;
    }

    filteredNotifications.forEach(notification => {
        const notificationHtml = createNotificationItem(notification);
        container.append(notificationHtml);
    });

    // Attach event handlers
    attachEventHandlers();
}

// Create notification item HTML
function createNotificationItem(notification) {
    const isUnread = !notification.isRead;
    const unreadClass = isUnread ? 'unread' : '';
    const iconClass = getNotificationIcon(notification.type);
    const iconBg = getNotificationIconBg(notification.type);

    return `
        <div class="list-group-item notification-item ${unreadClass}" data-id="${notification.notificationId}">
            <div class="d-flex gap-3">
                <div class="notification-icon bg-soft-${iconBg} text-${iconBg}">
                    <i class="${iconClass}"></i>
                </div>
                <div class="notification-content">
                    <div class="notification-title">${escapeHtml(notification.title)}</div>
                    <div class="notification-message">${escapeHtml(notification.message)}</div>
                    <div class="notification-time">
                        <i class="feather-clock me-1"></i>${formatTimeAgo(notification.createdAt)}
                    </div>
                </div>
                <div class="notification-actions ms-auto">
                    ${isUnread ? `
                        <button class="btn btn-sm btn-outline-success btn-mark-read" data-id="${notification.notificationId}" title="Mark as Read">
                            <i class="feather-check"></i>
                        </button>
                    ` : ''}
                    <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${notification.notificationId}" title="Delete">
                        <i class="feather-trash-2"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
}

// Get notification icon based on type
function getNotificationIcon(type) {
    const icons = {
        'info': 'feather-info',
        'success': 'feather-check-circle',
        'warning': 'feather-alert-triangle',
        'error': 'feather-alert-circle',
        'invoice': 'feather-file-text',
        'contract': 'feather-briefcase',
        'attachment': 'feather-paperclip',
        'approval': 'feather-check-square'
    };
    return icons[type?.toLowerCase()] || 'feather-bell';
}

// Get notification icon background color
function getNotificationIconBg(type) {
    const colors = {
        'info': 'info',
        'success': 'success',
        'warning': 'warning',
        'error': 'danger',
        'invoice': 'primary',
        'contract': 'info',
        'attachment': 'secondary',
        'approval': 'success'
    };
    return colors[type?.toLowerCase()] || 'primary';
}

// Update badge counts
function updateBadges() {
    const allCount = allNotifications.length;
    const unreadCount = allNotifications.filter(n => !n.isRead).length;
    const readCount = allNotifications.filter(n => n.isRead).length;

    $('#badgeAll').text(allCount);
    $('#badgeUnread').text(unreadCount);
    $('#badgeRead').text(readCount);
}

// Attach event handlers to notification items
function attachEventHandlers() {
    // Mark as read
    $('.btn-mark-read').off('click').on('click', function (e) {
        e.preventDefault();
        const notificationId = $(this).data('id');
        markAsRead(notificationId);
    });

    // Delete notification
    $('.btn-delete').off('click').on('click', function (e) {
        e.preventDefault();
        const notificationId = $(this).data('id');
        deleteNotification(notificationId);
    });

    // Click notification item to mark as read
    $('.notification-item.unread').off('click').on('click', function (e) {
        if (!$(e.target).closest('.notification-actions').length) {
            const notificationId = $(this).data('id');
            markAsRead(notificationId);
        }
    });
}

// Mark single notification as read
function markAsRead(notificationId) {
    $.ajax({
        url: `/api/notifications/${notificationId}/mark-read`,
        type: 'PUT',
        success: function (response) {
            if (response.success) {
                // Update local data
                const notification = allNotifications.find(n => n.notificationId === notificationId);
                if (notification) {
                    notification.isRead = true;
                }
                renderNotifications();
                updateBadges();
                showSuccess('Notification marked as read');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error marking as read:', error);
            showError('Failed to mark as read');
        }
    });
}

// Mark all notifications as read
function markAllAsRead() {
    Swal.fire({
        title: 'Mark All as Read?',
        text: 'This will mark all notifications as read',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#0d6efd',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, mark all',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/api/notifications/mark-all-read',
                type: 'PUT',
                success: function (response) {
                    if (response.success) {
                        // Update local data
                        allNotifications.forEach(n => n.isRead = true);
                        renderNotifications();
                        updateBadges();
                        showSuccess('All notifications marked as read');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error marking all as read:', error);
                    showError('Failed to mark all as read');
                }
            });
        }
    });
}

// Delete single notification
function deleteNotification(notificationId) {
    Swal.fire({
        title: 'Delete Notification?',
        text: 'This action cannot be undone',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, delete',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/api/notifications/${notificationId}`,
                type: 'DELETE',
                success: function (response) {
                    if (response.success) {
                        // Remove from local data
                        allNotifications = allNotifications.filter(n => n.notificationId !== notificationId);
                        renderNotifications();
                        updateBadges();
                        showSuccess('Notification deleted');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error deleting notification:', error);
                    showError('Failed to delete notification');
                }
            });
        }
    });
}

// Delete all notifications
function deleteAllNotifications() {
    if (allNotifications.length === 0) {
        showWarning('No notifications to delete');
        return;
    }

    Swal.fire({
        title: 'Delete All Notifications?',
        text: 'This will delete all your notifications permanently',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, delete all',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            // Delete all notifications one by one
            const deletePromises = allNotifications.map(notification => {
                return $.ajax({
                    url: `/api/notifications/${notification.notificationId}`,
                    type: 'DELETE'
                });
            });

            Promise.all(deletePromises)
                .then(() => {
                    allNotifications = [];
                    renderNotifications();
                    updateBadges();
                    showSuccess('All notifications deleted');
                })
                .catch((error) => {
                    console.error('Error deleting all notifications:', error);
                    showError('Failed to delete all notifications');
                    loadNotifications(); // Reload to get current state
                });
        }
    });
}

// Format time ago
function formatTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const seconds = Math.floor((now - date) / 1000);

    if (seconds < 60) return 'Just now';
    if (seconds < 3600) return Math.floor(seconds / 60) + ' minutes ago';
    if (seconds < 86400) return Math.floor(seconds / 3600) + ' hours ago';
    if (seconds < 604800) return Math.floor(seconds / 86400) + ' days ago';
    if (seconds < 2592000) return Math.floor(seconds / 604800) + ' weeks ago';
    return Math.floor(seconds / 2592000) + ' months ago';
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Show success message
function showSuccess(message) {
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: message,
        timer: 2000,
        showConfirmButton: false
    });
}

// Show error message
function showError(message) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message,
        confirmButtonColor: '#dc3545'
    });
}

// Show warning message
function showWarning(message) {
    Swal.fire({
        icon: 'warning',
        title: 'Warning',
        text: message,
        confirmButtonColor: '#ffc107'
    });
}

// ========================================
// EMAIL SMTP TESTING FUNCTIONS
// ========================================

$(document).ready(function () {
    // Send test email
    $('#emailTestForm').on('submit', function (e) {
        e.preventDefault();
        sendTestEmail();
    });

    // Check SMTP configuration
    $('#btnCheckConfig').on('click', function () {
        checkSmtpConfig();
    });
});

// Send test email
function sendTestEmail() {
    const emailAddress = $('#testEmailAddress').val();
    const template = $('#testEmailTemplate').val();

    if (!emailAddress) {
        showError('Please enter an email address');
        return;
    }

    // Disable button and show loading
    const btnSendTest = $('#btnSendTest');
    const originalText = btnSendTest.html();
    btnSendTest.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Sending...');

    // Hide previous result
    $('#emailTestResult').addClass('d-none');

    let endpoint = '/api/test/email/send';
    let requestData = { email: emailAddress };

    // Change endpoint based on template
    if (template === 'invoice') {
        endpoint = '/api/test/email/send-invoice';
        requestData = { recipientEmail: emailAddress };
    } else if (template === 'contract') {
        endpoint = '/api/test/email/send-contract';
        requestData = { recipientEmail: emailAddress };
    } else if (template === 'welcome') {
        endpoint = '/api/test/email/send-welcome';
        requestData = {
            email: emailAddress,
            name: "Test User"
        };
    }

    $.ajax({
        url: endpoint,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function (response) {
            console.log('Test email response:', response);
            showTestResult(true, 'Success!', response.message || 'Test email sent successfully. Please check your inbox.');
            
            // Reset form
            $('#testEmailAddress').val('');
        },
        error: function (xhr, status, error) {
            console.error('Error sending test email:', error);
            let errorMessage = 'Failed to send test email. Please check your SMTP configuration.';
            
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            } else if (xhr.responseText) {
                try {
                    const errorData = JSON.parse(xhr.responseText);
                    errorMessage = errorData.message || errorMessage;
                } catch (e) {
                    // Keep default error message
                }
            }
            
            showTestResult(false, 'Failed', errorMessage);
        },
        complete: function () {
            // Re-enable button
            btnSendTest.prop('disabled', false).html(originalText);
        }
    });
}

// Show test result
function showTestResult(success, title, message) {
    const resultDiv = $('#emailTestResult');
    const alertDiv = $('#testResultAlert');
    const iconElement = $('#testResultIcon');
    const titleElement = $('#testResultTitle');
    const messageElement = $('#testResultMessage');

    // Set alert class
    alertDiv.removeClass('alert-success alert-danger');
    alertDiv.addClass(success ? 'alert-success' : 'alert-danger');

    // Set icon
    iconElement.removeClass();
    iconElement.addClass('feather me-2');
    iconElement.addClass(success ? 'feather-check-circle' : 'feather-alert-circle');

    // Set content
    titleElement.text(title);
    messageElement.text(message);

    // Show result
    resultDiv.removeClass('d-none');

    // Scroll to result
    resultDiv[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

// Check SMTP configuration
function checkSmtpConfig() {
    const btnCheckConfig = $('#btnCheckConfig');
    const originalText = btnCheckConfig.html();
    btnCheckConfig.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Checking...');

    // Hide previous result
    $('#emailTestResult').addClass('d-none');

    $.ajax({
        url: '/api/test/email/config',
        type: 'GET',
        success: function (response) {
            console.log('SMTP config:', response);
            if (response.success && response.data) {
                displaySmtpConfig(response.data);
                showTestResult(true, 'Configuration Retrieved', 'SMTP configuration loaded successfully.');
            } else {
                showTestResult(false, 'Failed', 'Could not retrieve SMTP configuration.');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error checking SMTP config:', error);
            showTestResult(false, 'Error', 'Failed to check SMTP configuration. Endpoint might not be available.');
        },
        complete: function () {
            btnCheckConfig.prop('disabled', false).html(originalText);
        }
    });
}

// Display SMTP configuration
function displaySmtpConfig(config) {
    $('#smtpProvider').text(config.provider || '-');
    $('#smtpHost').text(config.host || '-');
    $('#smtpPort').text(config.port || '-');
    $('#smtpFromEmail').text(config.fromEmail || '-');
    $('#smtpFromName').text(config.fromName || '-');
    $('#smtpUseSsl').text(config.useSsl ? 'Yes' : 'No');

    $('#smtpConfigInfo').removeClass('d-none');
    $('#smtpConfigInfo')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}
