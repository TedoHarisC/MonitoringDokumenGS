# Notification Management Page Guide

## Overview

Halaman Notification Management memungkinkan user untuk melihat, mengelola, dan menghapus notifikasi mereka sendiri.

## Access Points

### 1. Dari Menu Settings

```
Dashboard → Settings → Notification
```

### 2. Dari Avatar Dropdown

```
Click Avatar (Top Right) → Notifications
```

## Features

### 📋 View Notifications

- **All Tab**: Menampilkan semua notifikasi (read + unread)
- **Unread Tab**: Menampilkan hanya notifikasi yang belum dibaca
- **Read Tab**: Menampilkan hanya notifikasi yang sudah dibaca

### ✅ Mark as Read

1. **Single Notification**: Click tombol ✓ (check) pada notification item
2. **All Notifications**: Click tombol "Mark All as Read" di header
3. **Auto Mark**: Click pada unread notification item akan otomatis mark as read

### 🗑️ Delete Notifications

1. **Single Delete**: Click tombol 🗑️ (trash) pada notification item
2. **Delete All**: Click tombol "Delete All" di header
3. **Confirmation**: Sistem akan meminta konfirmasi sebelum delete

### 🔄 Auto Refresh

- Halaman akan auto-refresh setiap **30 detik** untuk load notifikasi terbaru
- Badge count akan update secara real-time

## Notification Types & Icons

| Type       | Icon | Color  | Description        |
| ---------- | ---- | ------ | ------------------ |
| Info       | ℹ️   | Blue   | Informasi umum     |
| Success    | ✓    | Green  | Operasi berhasil   |
| Warning    | ⚠️   | Yellow | Peringatan         |
| Error      | ⚠️   | Red    | Error atau gagal   |
| Invoice    | 📄   | Blue   | Invoice related    |
| Contract   | 💼   | Blue   | Contract related   |
| Attachment | 📎   | Gray   | Attachment related |
| Approval   | ✓    | Green  | Approval related   |

## UI Elements

### Notification Item Structure

```
┌─────────────────────────────────────────────┐
│ [Icon] Title                           [✓][🗑️] │
│        Message text here...                  │
│        🕐 2 hours ago                        │
└─────────────────────────────────────────────┘
```

### Visual Indicators

- **Unread**: Light blue background + blue left border
- **Read**: White background
- **Hover**: Light gray background

### Badge Counters

- **All Badge**: Total semua notifikasi
- **Unread Badge**: Total unread (Red)
- **Read Badge**: Total read (Green)

## API Endpoints Used

```javascript
GET / api / notifications; // Get all notifications for current user
GET / api / notifications / { id }; // Get single notification
PUT / api / notifications / { id } / mark - read; // Mark as read
PUT / api / notifications / mark - all - read; // Mark all as read
DELETE / api / notifications / { id }; // Delete notification
```

## Testing Guide

### Test 1: View Notifications

1. Login ke aplikasi
2. Buat beberapa Invoice/Contract (akan auto-create notifications)
3. Navigate ke Settings → Notification
4. Verify notifications tampil dengan benar
5. Check badge counts

### Test 2: Mark as Read

1. Di halaman Notifications, go to "Unread" tab
2. Click notification item atau tombol ✓
3. Verify notification pindah ke "Read" tab
4. Verify badge unread count berkurang

### Test 3: Delete Notification

1. Di halaman Notifications, pilih notification
2. Click tombol 🗑️ (trash)
3. Confirm delete di dialog
4. Verify notification hilang
5. Verify badge count update

### Test 4: Delete All

1. Pastikan ada beberapa notifications
2. Click "Delete All" button
3. Confirm di dialog
4. Verify semua notifications terhapus
5. Verify empty state tampil

### Test 5: Multi-User Isolation

1. Login sebagai User A
2. Check notifications User A
3. Logout, login sebagai User B
4. Check notifications User B
5. Verify: User B **tidak bisa** lihat notifikasi User A

### Test 6: Auto Refresh

1. Buka halaman Notifications
2. Dari browser lain/tab lain, create Invoice (sebagai user yang sama)
3. Wait maksimal 30 detik
4. Verify notification baru muncul otomatis
5. Verify badge count update

## JavaScript Functions

### Main Functions

```javascript
loadNotifications(); // Load from API
renderNotifications(); // Render based on current filter
updateBadges(); // Update badge counts
markAsRead(id); // Mark single as read
markAllAsRead(); // Mark all as read
deleteNotification(id); // Delete single
deleteAllNotifications(); // Delete all
formatTimeAgo(dateString); // Format timestamp
```

### Event Handlers

```javascript
$("#notificationTabs button").on("click"); // Tab switching
$("#btnMarkAllRead").on("click"); // Mark all button
$("#btnDeleteAll").on("click"); // Delete all button
$(".btn-mark-read").on("click"); // Mark single
$(".btn-delete").on("click"); // Delete single
$(".notification-item.unread").on("click"); // Click to read
```

## Styling Classes

### Custom CSS

```css
.notification-item           // Base notification item
.notification-item.unread    // Unread state (blue background)
.notification-icon          // Icon container (40x40px)
.notification-content       // Content area (flex: 1)
.notification-title         // Title (font-weight: 600)
.notification-message       // Message text (14px)
.notification-time          // Timestamp (12px, gray)
.notification-actions       // Action buttons container
```

## Security Features

1. **Authentication Required**: User must be logged in
2. **User Isolation**: User hanya bisa lihat notifikasi sendiri
3. **Delete Authorization**: Verify ownership sebelum delete
4. **XSS Prevention**: HTML escaping pada title & message

## Performance Optimizations

1. **Client-side Filtering**: Filter All/Unread/Read tanpa API call
2. **Local Data Cache**: Store notifications di memory
3. **Batch Delete**: Delete all menggunakan Promise.all()
4. **Debounced Refresh**: Auto-refresh setiap 30s (tidak terlalu frequent)

## Error Handling

### Network Errors

```javascript
// Show error toast
showError("Failed to load notifications");

// Retry logic: reload on next auto-refresh (30s)
```

### Empty States

```javascript
// No notifications
<div class="text-center py-5">
  <i class="feather-bell"></i>
  <p>No notifications</p>
</div>
```

### Delete Confirmation

```javascript
Swal.fire({
  title: "Delete Notification?",
  text: "This action cannot be undone",
  icon: "warning",
  showCancelButton: true,
});
```

## Files Created

```
/Controllers/Web/NotificationController.cs        // Web controller
/Views/Notification/Index.cshtml                  // View page
/wwwroot/js/notification-page.js                  // Frontend logic
/Controllers/API/NotificationsController.cs       // Added DELETE endpoint
/Services/Infrastructure/NotificationService.cs   // Added DeleteAsync method
/Interfaces/INotifications.cs                     // Added DeleteAsync interface
/Views/Shared/_LayoutDashboard.cshtml            // Updated menu links
```

## Troubleshooting

### Issue: Notifications tidak muncul

**Solution**:

- Check browser console untuk errors
- Verify user sudah login
- Check API endpoint `/api/notifications` returns data

### Issue: Delete tidak berhasil

**Solution**:

- Verify notification belongs to current user
- Check console untuk 403 Forbidden errors
- Verify DeleteAsync method di service

### Issue: Auto-refresh tidak jalan

**Solution**:

- Check console untuk JavaScript errors
- Verify `setInterval(loadNotifications, 30000)` executed
- Check network tab untuk periodic API calls

## Best Practices

1. ✅ **Always confirm before delete** (especially Delete All)
2. ✅ **Show loading states** during API calls
3. ✅ **Update UI optimistically** (local update + API call)
4. ✅ **Handle errors gracefully** with user-friendly messages
5. ✅ **Keep badge counts synchronized** across all UI elements
6. ✅ **Use semantic icons** untuk notification types
7. ✅ **Implement empty states** untuk better UX

## Future Enhancements

1. 🔔 **Push Notifications**: Real-time using SignalR
2. 🔍 **Search/Filter**: Search notifications by title/message
3. 📅 **Date Range Filter**: Filter by date created
4. 🏷️ **Category Filter**: Filter by notification type
5. ⭐ **Priority Levels**: High/Medium/Low priority
6. 📧 **Email Notifications**: Send email untuk important notifications
7. 🔕 **Notification Preferences**: User settings untuk notification types
8. 📊 **Notification Analytics**: Dashboard untuk notification stats

---

**Created**: January 26, 2026  
**Last Updated**: January 26, 2026  
**Version**: 1.0
