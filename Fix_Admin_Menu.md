# 🛠 Fix Admin Menu Hidden Due to Missing systemRoles

## 🎯 Mục tiêu

Đảm bảo admin menu hiển thị đúng dựa trên systemRoles.

---

## ❌ Nguyên nhân

* API `/users/me` không trả về `systemRoles`
* Frontend overwrite toàn bộ currentUser bằng profile API
* Mất `systemRoles` → hasSystemAdminAccess() luôn false

---

## ✅ Kết quả mong muốn

* currentUser luôn chứa:

  * profile data (fullName, email...)
  * systemRoles
* Admin menu hiển thị đúng

---

## 🔍 Phạm vi được phép sửa (ALLOW)

AI chỉ được:

1. Sửa logic gán currentUser / profileData
2. Đảm bảo không mất `systemRoles`

---

## 🚫 KHÔNG ĐƯỢC làm

* ❌ Không merge mù toàn bộ object nếu không kiểm soát field
* ❌ Không thay đổi backend API (trừ khi được yêu cầu)
* ❌ Không refactor auth system
* ❌ Không sửa UI

---

## ⚠️ Quy tắc bắt buộc

* systemRoles phải luôn được giữ lại
* Không overwrite role từ API nếu API không trả về
* Ưu tiên tách rõ:

  * profile data
  * auth data (role)

---

## 📌 Gợi ý fix

Sai:

```js
profileData.value = response.data.data
```

Đúng:

```js
const storedUser = getStoredUser()

profileData.value = {
  ...response.data.data,
  systemRoles: storedUser.systemRoles
}
```

---

## 📤 Output mong muốn

* Chỉ sửa logic mất systemRoles
* Không ảnh hưởng phần khác
