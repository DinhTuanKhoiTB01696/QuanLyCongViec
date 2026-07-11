using System;

namespace TaskManagement.Domain.Entities
{
    /// <summary>
    /// Hàng chờ "Inbox" — cho phép gom nhặt Issue từ bên ngoài (Khách hàng, Bot).
    /// Manager phải "Accept" hoặc "Decline" trước khi đẩy vào Backlog chính thức.
    /// </summary>
    public class Intake
    {
        public Guid Id { get; set; }

        /// <summary>Tiêu đề yêu cầu</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Mô tả chi tiết (JSON block editor format)</summary>
        public string? Description { get; set; }

        /// <summary>Nguồn gửi: "EMAIL", "API", "FORM", "MANUAL"</summary>
        public string Source { get; set; } = "MANUAL";

        /// <summary>Trạng thái: Pending, Accepted, Declined, Duplicate, Snoozed</summary>
        public string Status { get; set; } = "Pending";

        /// <summary>Mức độ ưu tiên (1: Khẩn cấp, 2: Cao, 3: Trung bình, 4: Thấp)</summary>
        public int Priority { get; set; } = 3;

        /// <summary>Hạn mong muốn hoàn thành</summary>
        public DateTime? DesiredDueDate { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        /// <summary>Người gửi (có thể null nếu từ external)</summary>
        public Guid? SubmittedById { get; set; }
        public User? SubmittedBy { get; set; }

        /// <summary>Người duyệt (PM/PO)</summary>
        public Guid? ReviewedById { get; set; }
        public User? ReviewedBy { get; set; }

        /// <summary>Khi Accept, tạo ra WorkTask tương ứng</summary>
        public Guid? CreatedIssueId { get; set; }
        public WorkTask? CreatedIssue { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
