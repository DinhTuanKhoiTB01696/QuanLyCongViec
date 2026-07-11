using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.AI;
using TaskManagement.Application.DTOs.WorkTask;

namespace TaskManagement.Application.Interfaces
{
    public interface IAiService
    {
        Task<string> ChatAsync(Guid userId, AiChatRequestDto request);
        Task<string> GenerateDescriptionAsync(Guid userId, AiGenerateDescriptionRequestDto request);
        Task<List<AiSubTaskDto>> BreakdownTaskAsync(Guid userId, AiBreakdownRequestDto request);
        Task<List<WorkTaskResponseDto>> BreakdownAndCreateSubtasksAsync(Guid userId, AiBreakdownRequestDto request);
        Task<List<WorkTaskResponseDto>> CreateSubtasksFromPreviewAsync(Guid userId, AiCreateSubtasksFromPreviewRequestDto request);
        Task<AiEstimateSuggestionDto> SuggestEstimateAsync(Guid userId, AiEstimateSuggestionRequestDto request);
        Task<AiAssigneeSuggestionDto> SuggestAssigneesAsync(Guid userId, AiAssigneeSuggestionRequestDto request);
        Task<AiRepositoryAnalysisDto> AnalyzeRepositoryAsync(Guid userId, AiRepositoryAnalysisRequestDto request);
        Task<List<WorkTaskResponseDto>> CreateBacklogItemsFromAnalysisAsync(Guid userId, AiCreateBacklogFromAnalysisRequestDto request);
        Task<AiUsageDto> GetUsageAsync(Guid userId);
        Task<AiFileAnalysisResultDto> AnalyzeFileAsync(Guid userId, string fileName, string mimeType, byte[] fileBytes, string? userPrompt, Guid? projectId, string? previousAnalysisJson);
        Task<AiProjectAssistantResponseDto> ProjectAssistantAsync(Guid userId, AiProjectAssistantRequestDto request);
        Task<AiContextChatResponseDto> ContextChatAsync(Guid userId, AiContextChatRequestDto request);
    }

    public class AiFileAnalysisResultDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyPoints { get; set; } = new();
        public List<string> Risks { get; set; } = new();
        public List<SuggestedTaskDto> SuggestedTasks { get; set; } = new();
        public List<string> SuggestedPrompts { get; set; } = new();
        public List<string> Questions { get; set; } = new();
        public string RawTextPreview { get; set; } = string.Empty;
    }

    public class SuggestedTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 3;
        public string? DueDate { get; set; }
        public string? AssigneeEmail { get; set; }
    }

    public class AiProjectAssistantRequestDto
    {
        public Guid ProjectId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Mode { get; set; } = "chat"; // chat | summarize | plan | prompt
        public List<AiChatMessageDto> History { get; set; } = new();
    }

    public class AiChatMessageDto
    {
        public string Role { get; set; } = string.Empty; // user | assistant
        public string Content { get; set; } = string.Empty;
    }

    public class AiProjectAssistantResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public List<SuggestedTaskDto> SuggestedTasks { get; set; } = new();
        public List<string> SuggestedPrompts { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Sources { get; set; } = new();
    }

    public class AiContextChatRequestDto
    {
        public string Route { get; set; } = string.Empty;
        public Guid? ProjectId { get; set; }
        public Guid? WorkspaceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SelectedText { get; set; }
        public AiContextPageDto PageContext { get; set; } = new();
    }

    public class AiContextPageDto
    {
        public string PageType { get; set; } = "unknown";
        public string CurrentView { get; set; } = string.Empty;
        public List<Guid> VisibleTaskIds { get; set; } = new();
        public List<string> VisibleStatuses { get; set; } = new();
        public Dictionary<string, string> Filters { get; set; } = new();
        public Dictionary<string, string> Extra { get; set; } = new();
    }

    public class AiContextChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<AiSuggestedActionDto> Actions { get; set; } = new();
    }

    public class AiSuggestedActionDto
    {
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public Dictionary<string, string> Payload { get; set; } = new();
    }
}
