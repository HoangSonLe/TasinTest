# 🔔 Hệ Thống Thông Báo Đẹp

## 📋 Tổng Quan

Hệ thống thông báo hiện đại với thiết kế đẹp mắt, animations mượt mà và responsive design. Được xây dựng dựa trên Kendo UI Notification với CSS tùy chỉnh và JavaScript nâng cao.

## ✨ Tính Năng

### 🎨 Thiết Kế
- **Modern UI**: Gradient backgrounds, rounded corners, shadows
- **Responsive**: Tự động điều chỉnh cho mobile và desktop
- **Accessible**: Hỗ trợ high contrast và reduced motion
- **Progress Bar**: Hiển thị thời gian còn lại trước khi tự ẩn

### 🚀 Animations
- **Slide-in**: Hiệu ứng trượt từ phải sang trái
- **Smooth Transitions**: Chuyển đổi mượt mà
- **Stacking**: Xếp chồng thông báo theo chiều dọc

### 🎯 Loại Thông Báo
- **Success**: Màu xanh lá với icon check-circle
- **Error**: Màu đỏ với icon times-circle  
- **Warning/Info**: Màu cam với icon exclamation-triangle

## 📁 Cấu Trúc Files

```
wwwroot/css/
├── notification-common.css          # CSS chính cho notifications
└── NOTIFICATION_README.md          # File hướng dẫn này

wwwroot/js/kendogridresize/
└── notification.js                 # Cấu hình JavaScript

Views/Shared/
└── _Layout.cshtml                  # Templates và CSS reference

Views/Home/
├── Index.cshtml                    # Demo cơ bản
└── NotificationDemo.cshtml         # Trang demo đầy đủ
```

## 🔧 Cách Sử Dụng

### Cơ Bản

```javascript
// Thông báo thành công
notification.show({
    title: "Thành công!",
    message: "Dữ liệu đã được lưu thành công."
}, "success");

// Thông báo lỗi
notification.show({
    title: "Lỗi!",
    message: "Đã xảy ra lỗi trong quá trình xử lý."
}, "error");

// Thông báo thông tin
notification.show({
    title: "Thông tin",
    message: "Hệ thống sẽ bảo trì vào 2:00 AM."
}, "info");
```

### Nâng Cao

```javascript
// Nhiều thông báo liên tiếp
function showProgressNotifications() {
    const steps = [
        { title: "Bước 1", message: "Đang khởi tạo...", type: "info" },
        { title: "Bước 2", message: "Đang xử lý...", type: "info" },
        { title: "Hoàn thành!", message: "Thành công!", type: "success" }
    ];
    
    steps.forEach((step, index) => {
        setTimeout(() => {
            notification.show({
                title: step.title,
                message: step.message
            }, step.type);
        }, index * 1000);
    });
}
```

## 🎮 Demo

### Truy Cập Demo
1. **Demo Cơ Bản**: `/Home/Index` - Các nút test cơ bản
2. **Demo Đầy Đủ**: `/Home/NotificationDemo` - Trang demo chuyên nghiệp

### Tính Năng Demo
- ✅ Test các loại thông báo
- ✅ Tùy chỉnh nội dung
- ✅ Demo scenarios thực tế
- ✅ Code examples
- ✅ Interactive effects

## ⚙️ Cấu Hình

### CSS Variables (có thể tùy chỉnh)

```css
:root {
    --notification-border-radius: 12px;
    --notification-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
    --notification-animation-duration: 0.4s;
    --notification-auto-hide: 5000ms;
}
```

### JavaScript Settings

```javascript
var notification = $("#notification").kendoNotification({
    position: { pinned: true, bottom: 20, right: 20 },
    autoHideAfter: 5000,
    stacking: "up",
    animation: {
        open: { effects: "slideIn:right", duration: 400 },
        close: { effects: "slideIn:right", reverse: true, duration: 300 }
    }
});
```

## 📱 Responsive Design

### Desktop
- Kích thước: 320px - 420px width
- Vị trí: Bottom-right corner
- Margin: 20px từ cạnh

### Mobile
- Kích thước: calc(100vw - 32px)
- Margin: 16px từ cạnh
- Font size nhỏ hơn

## 🎨 Customization

### Thay Đổi Màu Sắc

```css
/* Success - Xanh lá */
.upload-success {
    background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
    border-left: 5px solid #2E7D32;
}

/* Error - Đỏ */
.wrong-pass {
    background: linear-gradient(135deg, #f44336 0%, #d32f2f 100%);
    border-left: 5px solid #c62828;
}

/* Warning/Info - Cam */
.wrong-temp {
    background: linear-gradient(135deg, #FF9800 0%, #F57C00 100%);
    border-left: 5px solid #E65100;
}
```

### Thêm Loại Thông Báo Mới

1. Thêm CSS class mới
2. Thêm template trong _Layout.cshtml
3. Cập nhật JavaScript configuration

## 🔍 Troubleshooting

### Thông Báo Không Hiển Thị
- ✅ Kiểm tra `notification-common.css` đã được load
- ✅ Kiểm tra `notification.js` đã được load
- ✅ Kiểm tra element `#notification` tồn tại

### Animations Không Mượt
- ✅ Kiểm tra CSS animations được hỗ trợ
- ✅ Kiểm tra `prefers-reduced-motion` setting
- ✅ Kiểm tra performance của browser

### Responsive Issues
- ✅ Kiểm tra viewport meta tag
- ✅ Kiểm tra CSS media queries
- ✅ Test trên các device khác nhau

## 📞 Hỗ Trợ

Nếu gặp vấn đề hoặc cần hỗ trợ:
1. Kiểm tra console browser để xem lỗi
2. Kiểm tra network tab để đảm bảo CSS/JS được load
3. Test trên trang demo để so sánh

## 🚀 Phiên Bản Tương Lai

### Planned Features
- [ ] Sound notifications
- [ ] Custom icons
- [ ] Notification history
- [ ] Batch operations
- [ ] Theme customization panel

---

**Tạo bởi**: Augment Agent  
**Ngày**: 2024  
**Phiên bản**: 1.0.0
