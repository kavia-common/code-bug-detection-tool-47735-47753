using System.ComponentModel.DataAnnotations;

namespace BugDetectorBackend.Models
{
    /// <summary>
    /// Defines severity levels for detected issues.
    /// </summary>
    public enum SeverityLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    // PUBLIC_INTERFACE
    /// <summary>
    /// Request payload for analyzing a code snippet.
    /// </summary>
    public sealed class BugAnalysisRequest
    {
        /// <summary>
        /// Programming language of the snippet (e.g., 'csharp', 'javascript', 'python').
        /// </summary>
        [Required]
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// The code snippet to analyze.
        /// </summary>
        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Optional filename for context (used to infer language if provided).
        /// </summary>
        public string? FileName { get; set; }
    }

    // PUBLIC_INTERFACE
    /// <summary>
    /// Represents a single detected issue in the provided code.
    /// </summary>
    public sealed class DetectedIssue
    {
        /// <summary>
        /// Severity of the issue.
        /// </summary>
        public SeverityLevel Severity { get; set; }

        /// <summary>
        /// Human-readable message describing the issue.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 1-based line number where the issue was detected, if known.
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// Optional rule identifier (useful for suppressions or documentation).
        /// </summary>
        public string? RuleId { get; set; }
    }

    // PUBLIC_INTERFACE
    /// <summary>
    /// Response payload containing analysis results.
    /// </summary>
    public sealed class BugAnalysisResponse
    {
        /// <summary>
        /// Echoed or inferred language used for analysis.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Collection of detected issues.
        /// </summary>
        public List<DetectedIssue> Issues { get; set; } = new();

        /// <summary>
        /// Total issue count.
        /// </summary>
        public int Count => Issues?.Count ?? 0;

        /// <summary>
        /// True if no issues were found.
        /// </summary>
        public bool IsClean => Count == 0;
    }
}
