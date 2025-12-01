using BugDetectorBackend.Models;

namespace BugDetectorBackend.Services
{
    // PUBLIC_INTERFACE
    /// <summary>
    /// Abstraction for analyzing code snippets and returning detected issues.
    /// </summary>
    public interface IBugAnalyzer
    {
        /// <summary>
        /// Analyze the given code snippet using the supplied language.
        /// </summary>
        /// <param name="request">Bug analysis request including language and code.</param>
        /// <returns>BugAnalysisResponse containing any detected issues.</returns>
        Task<BugAnalysisResponse> AnalyzeAsync(BugAnalysisRequest request, CancellationToken cancellationToken = default);
    }
}
