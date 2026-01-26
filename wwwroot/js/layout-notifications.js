/* layout-notifications.js
   Handle notifications in dashboard layout
   Fetch notifications from API and render in dropdown
*/

;(function ($) {
    'use strict'

    const notificationsApi = '/api/notifications'
    let notifications = []

    async function fetchJson(url, options = {}) {
        const res = await fetch(url, options)
        if (!res.ok) {
            const err = await res.json().catch(() => ({ message: 'Request failed' }))
            throw err
        }
        return res.json()
    }

    function formatTimeAgo(dateStr) {
        const now = new Date()
        const date = new Date(dateStr)
        const seconds = Math.floor((now - date) / 1000)

        if (seconds < 60) return 'Just now'
        if (seconds < 3600) return `${Math.floor(seconds / 60)} minutes ago`
        if (seconds < 86400) return `${Math.floor(seconds / 3600)} hours ago`
        if (seconds < 2592000) return `${Math.floor(seconds / 86400)} days ago`
        return date.toLocaleDateString()
    }

    async function loadNotifications() {
        try {
            const response = await fetchJson(notificationsApi)
            
            if (response.success && response.data) {
                notifications = Array.isArray(response.data) ? response.data : []
                renderNotifications()
                updateBadge()
            }
        } catch (err) {
            console.error('Failed to load notifications:', err)
        }
    }

    function renderNotifications() {
        const $container = $('.notifications-item-wrapper')
        if (!$container.length) return

        $container.empty()

        if (notifications.length === 0) {
            $container.append(`
                <div class="text-center py-4 text-muted">
                    <p class="mb-0">No notifications yet</p>
                </div>
            `)
            return
        }

        // Show only first 5 notifications
        const displayNotifications = notifications.slice(0, 5)

        displayNotifications.forEach(notification => {
            const timeAgo = formatTimeAgo(notification.createdAt)
            const isReadClass = notification.isRead ? '' : 'bg-light'
            const readIndicator = notification.isRead 
                ? '<div class="wd-8 ht-8 rounded-circle bg-success"></div>' 
                : '<div class="wd-8 ht-8 rounded-circle bg-warning"></div>'

            $container.append(`
                <div class="notifications-item ${isReadClass}" data-id="${notification.notificationId}">
                    <div class="notifications-desc">
                        <a href="javascript:void(0);" class="font-body text-truncate-2-line notification-link">
                            <span class="fw-semibold text-dark">${notification.title}</span> ${notification.message}
                        </a>
                        <div class="d-flex justify-content-between align-items-center">
                            <div class="notifications-date text-muted border-bottom border-bottom-dashed">
                                ${timeAgo}
                            </div>
                            <div class="d-flex align-items-center float-end gap-2">
                                ${notification.isRead ? '' : `
                                    <a href="javascript:void(0);" class="btn-mark-read" data-id="${notification.notificationId}" 
                                       data-bs-toggle="tooltip" title="Mark as Read">
                                        ${readIndicator}
                                    </a>
                                `}
                            </div>
                        </div>
                    </div>
                </div>
            `)
        })

        // Add click handler for mark as read
        $('.btn-mark-read').on('click', function(e) {
            e.stopPropagation()
            const id = $(this).data('id')
            markAsRead(id)
        })
    }

    function updateBadge() {
        const unreadCount = notifications.filter(n => !n.isRead).length
        const $badge = $('.nxl-h-badge')
        
        if (unreadCount > 0) {
            $badge.text(unreadCount).show()
        } else {
            $badge.hide()
        }
    }

    async function markAsRead(notificationId) {
        try {
            await fetchJson(`${notificationsApi}/${notificationId}/mark-read`, {
                method: 'PUT'
            })

            // Update local state
            const notification = notifications.find(n => n.notificationId === notificationId)
            if (notification) {
                notification.isRead = true
                renderNotifications()
                updateBadge()
            }
        } catch (err) {
            console.error('Failed to mark as read:', err)
        }
    }

    async function markAllAsRead() {
        try {
            await fetchJson(`${notificationsApi}/mark-all-read`, {
                method: 'PUT'
            })

            // Update all notifications to read
            notifications.forEach(n => n.isRead = true)
            renderNotifications()
            updateBadge()

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: 'All notifications marked as read',
                    timer: 1500,
                    showConfirmButton: false
                })
            }
        } catch (err) {
            console.error('Failed to mark all as read:', err)
        }
    }

    // Initialize
    $(function() {
        // Load notifications on page load
        loadNotifications()

        // Reload notifications every 30 seconds
        setInterval(loadNotifications, 30000)

        // Mark all as read button
        $(document).on('click', '.mark-all-read', function(e) {
            e.preventDefault()
            markAllAsRead()
        })

        // Initialize tooltips
        if (typeof bootstrap !== 'undefined') {
            $('[data-bs-toggle="tooltip"]').each(function() {
                new bootstrap.Tooltip(this)
            })
        }
    })

})(jQuery)
