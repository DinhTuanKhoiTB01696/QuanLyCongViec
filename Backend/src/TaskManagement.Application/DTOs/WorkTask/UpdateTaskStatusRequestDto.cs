using System;

namespace TaskManagement.Application.DTOs.WorkTask
{
    public class UpdateTaskStatusRequestDto
    {
        public Guid TaskStatusId { get; set; }
        public byte[] RowVersion { get; set; } = null!;
    }
}
