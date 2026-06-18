# Bug Report Template

## Thông tin chung

- **Bug ID:** `BUG-YYYYMMDD-###`
- **Tiêu đề:** `[Khu vực] Mô tả ngắn kết quả sai`
- **Ngày phát hiện:**
- **Người báo:**
- **Môi trường:** Local / QA / Staging / Production
- **Build/commit:**
- **Browser/OS:**
- **Database:** SQL Server / InMemory
- **Tài khoản và role:** Không ghi password, token hoặc OTP
- **Project/Test data:**
- **Test case liên quan:**

## Phân loại

- **Severity:** Blocker / Critical / Major / Minor / Trivial
- **Priority:** P0 / P1 / P2 / P3
- **Loại:** Functional / Visual / API / Permission / Security / i18n / Performance / Data
- **Tần suất:** Always / Intermittent / Once
- **Regression:** Yes / No / Unknown

## Mô tả

Mô tả ngắn hành vi sai và ảnh hưởng đến người dùng hoặc dữ liệu.

## Điều kiện trước

1. 
2. 

## Bước tái hiện

1. 
2. 
3. 

## Kết quả thực tế

Mô tả chính xác kết quả quan sát được.

## Kết quả mong đợi

Mô tả expected dựa trên test case, source contract hoặc screenshot target.

## Dữ liệu và bằng chứng

- Screenshot/video:
- Console log:
- Request:
  - Method/URL:
  - Headers đã loại dữ liệu nhạy cảm:
  - Body đã che dữ liệu nhạy cảm:
- Response:
  - Status:
  - Body:
- Timestamp:
- Screenshot target và viewport, nếu là visual:

## Phạm vi ảnh hưởng

- Route/component/controller:
- Role bị ảnh hưởng:
- Dữ liệu có bị mất/sai không:
- Có workaround không:

## Visual deviation

Chỉ điền cho bug visual:

- Screenshot ID:
- Kích thước target:
- Viewport thực tế:
- Theme/language:
- Vùng sai:
- Sai layout / spacing / typography / color / content / overflow:
- Có phải annotation không: Yes / No
- `[CẦN XÁC NHẬN]` nếu target không đủ rõ:

## Security/permission

Chỉ điền cho bug quyền hoặc bảo mật:

- User/system role:
- Project role:
- API gọi trực tiếp hay qua UI:
- Expected status `401/403/400/429`:
- Dữ liệu có bị lộ hoặc thay đổi không:

## Ghi chú xử lý

- Owner:
- Root cause:
- Fix version:
- PR/commit:
- Cần regression các case:

## Retest

- Ngày retest:
- Build retest:
- Người retest:
- Kết quả: PASS / FAIL
- Bằng chứng:

