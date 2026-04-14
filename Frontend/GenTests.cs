using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string[] categories = {
            "WS|Workspace|Quản lý Không gian|20",
            "PJ|Project|Thiết lập Dự án|20",
            "TK|Task|Công việc Kanban|35",
            "CM|Comment|Đính kèm & Bình luận|15",
            "SP|Sprint|Chu kỳ Agile|25",
            "DP|Dependency|Ràng buộc công việc|15",
            "PG|Page|Tài liệu Wiki|20",
            "FRM|Form|Biểu mẫu Inbox|15",
            "SR|SignalR|Thời gian thực|10",
            "ADM|Admin|Quản trị Hệ thống|15",
            "RBA|RBAC|Bảo mật Phân quyền|15"
        };
        
        string[] verbs = { "Tạo mới", "Cập nhật", "Xóa mềm", "Đọc danh sách bảng", "Phân công lại", "Kéo thả Kanban", "Duyệt nhanh" };
        string[] objects = { " hợp lệ với data chuẩn", " với dữ liệu rỗng (Bỏ trống)", " với đoạn text dài 5000 ký tự", " có vượt quá ngưỡng giới hạn", " chứa ký tự đặc biệt (SQL Injection)", " cố tình làm trùng lặp slug" };
        
        StringBuilder sb = new StringBuilder();
        // BOM for Excel
        sb.AppendLine("Test Case ID,Test Case Title / Name,Module / Feature,Preconditions,Test Steps,Test Data,Expected Result,Actual Result,Status,Priority,Severity,Environment,Created By / Executed By,Notes / Comments");

        int idCounter = 1;

        foreach (var cat in categories)
        {
            var parts = cat.Split('|');
            string prefix = parts[0];
            string module = parts[1];
            string vnName = parts[2];
            int count = int.Parse(parts[3]);

            for (int i = 1; i <= count; i++)
            {
                string id = $"TC_{prefix}_{i:D3}";
                string verb = verbs[idCounter % verbs.Length];
                string obj = objects[i % objects.Length];
                
                string title = $"{verb} {vnName} {obj}";
                if (i == 1) title = $"[Luồng chính] Khởi tạo {vnName} chuẩn không lỗi";
                if (i == 2) title = $"[Luồng ngoại lệ] Thao tác {vnName} sai định dạng đầu vào";
                if (i == count) title = $"[Luồng hủy] Xóa vĩnh viễn/Xóa mềm {vnName} khỏi Database";

                string steps = "\"1. Nhấp chuột vào Module tương ứng\n2. Cố tình nhập dữ liệu Data Test vào\n3. Quan sát giao diện và Network F12\"";
                string expected = "\"Hệ thống gọi API thành công 200 OK và Cập nhật Entity Framework DB\"";
                if (obj.Contains("rỗng") || obj.Contains("dài") || obj.Contains("quá") || obj.Contains("trùng") || obj.Contains("đặc biệt") || i==2) {
                    expected = "\"Hệ thống DỘI NGƯỢC mã lỗi HTTP 400 Bad Request để tự vệ\"";
                }
                if (prefix == "RBA") {
                    expected = "\"Truy cập bị chặn với mã 403 Forbidden Access\"";
                }

                string priority = (i % 3 == 0) ? "Medium" : "High";
                string severity = (i % 4 == 0) ? "Minor" : "Major";
                if (title.Contains("xóa") || title.Contains("Luồng chính") || prefix == "RBA" || prefix == "ADM") severity = "Critical";

                string escTitle = $"\"{title}\"";
                
                sb.AppendLine($"{id},{escTitle},{module},\"Xác thực JWT Token từ Dev Login\",{steps},\"Data Mock #{idCounter}\",{expected},,Testing,{priority},{severity},\"Web Browser\",QA Lead,");

                idCounter++;
            }
        }

        File.WriteAllText(@"C:\Users\cuong\.gemini\antigravity\brain\665d5e99-62fd-4d6a-97ff-bfeee406eabc\BaoCao_205_TestCases_DayDu.csv", sb.ToString(), new UTF8Encoding(true));
    }
}
