using System.Text.RegularExpressions;
using BugDetectorBackend.Models;
using Microsoft.Extensions.Logging;

namespace BugDetectorBackend.Services
{
    /// <summary>
    /// A simple, fast, rule-based analyzer that flags common patterns likely to be bugs or code smells.
    /// This is a placeholder for static analysis and does not require external services.
    /// </summary>
    public sealed class RuleBasedBugAnalyzer : IBugAnalyzer
    {
        private readonly ILogger<RuleBasedBugAnalyzer> _logger;

        public RuleBasedBugAnalyzer(ILogger<RuleBasedBugAnalyzer> logger)
        {
            _logger = logger;
        }

        public Task<BugAnalysisResponse> AnalyzeAsync(BugAnalysisRequest request, CancellationToken cancellationToken = default)
        {
            var response = new BugAnalysisResponse
            {
                Language = InferLanguage(request)
            };

            try
            {
                var lines = request.Code.Replace("\r\n", "\n").Split('\n');

                // Generic rules across languages.
                for (int i = 0; i < lines.Length; i++)
                {
                    var lineNum = i + 1;
                    var line = lines[i];

                    // 1) TODO left in code
                    if (line.Contains("TODO", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Issues.Add(new DetectedIssue
                        {
                            Severity = SeverityLevel.Info,
                            Message = "TODO comment found; ensure it is resolved or tracked.",
                            Line = lineNum,
                            RuleId = "GEN0001"
                        });
                    }

                    // 2) Debug print/log traces committed
                    if (Regex.IsMatch(line, @"\b(console\.log|print\(|fmt\.Print|Debug\.WriteLine|System\.out\.println)\b"))
                    {
                        response.Issues.Add(new DetectedIssue
                        {
                            Severity = SeverityLevel.Warning,
                            Message = "Debug print/log statement detected.",
                            Line = lineNum,
                            RuleId = "GEN0002"
                        });
                    }

                    // 3) Hardcoded secrets (very naive)
                    if (Regex.IsMatch(line, @"(?i)(api[_-]?key|secret|password)\s*[:=]\s*['""][^'""]+['""]"))
                    {
                        response.Issues.Add(new DetectedIssue
                        {
                            Severity = SeverityLevel.Error,
                            Message = "Possible hardcoded credential detected.",
                            Line = lineNum,
                            RuleId = "SEC0001"
                        });
                    }

                    // Language-specific quick checks
                    switch (response.Language.ToLowerInvariant())
                    {
                        case "csharp":
                        case "cs":
                            // Unused discard or empty catch
                            if (Regex.IsMatch(line, @"catch\s*\(\s*\)\s*\{?\s*\}?$") || Regex.IsMatch(line, @"catch\s*\([\w\s,:<>?]*\)\s*\{\s*\}"))
                            {
                                response.Issues.Add(new DetectedIssue
                                {
                                    Severity = SeverityLevel.Warning,
                                    Message = "Empty catch block detected; consider handling or logging exceptions.",
                                    Line = lineNum,
                                    RuleId = "CSHARP0001"
                                });
                            }
                            break;

                        case "javascript":
                        case "typescript":
                        case "js":
                        case "ts":
                            if (Regex.IsMatch(line, @"var\s+\w+\s*="))
                            {
                                response.Issues.Add(new DetectedIssue
                                {
                                    Severity = SeverityLevel.Info,
                                    Message = "Usage of 'var' detected; consider 'let' or 'const' for clarity.",
                                    Line = lineNum,
                                    RuleId = "JS0001"
                                });
                            }
                            break;

                        case "python":
                            if (Regex.IsMatch(line, @"except\s*:\s*$"))
                            {
                                response.Issues.Add(new DetectedIssue
                                {
                                    Severity = SeverityLevel.Warning,
                                    Message = "Bare 'except:' detected; catch specific exceptions.",
                                    Line = lineNum,
                                    RuleId = "PY0001"
                                });
                            }
                            break;
                    }
                }

                // Multi-line rule example: large function heuristic (very naive)
                var approxFunctionCount = Regex.Matches(request.Code, @"\b(function|def|public\s+|private\s+|class\s+\w+)\b").Count;
                if (approxFunctionCount <= 1 && request.Code.Split('\n').Length > 120)
                {
                    response.Issues.Add(new DetectedIssue
                    {
                        Severity = SeverityLevel.Info,
                        Message = "Large block detected; consider refactoring into smaller functions.",
                        Line = null,
                        RuleId = "GEN0003"
                    });
                }

                _logger.LogInformation("Analyzed snippet: language={Language}, issues={IssueCount}", response.Language, response.Issues.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed due to an unexpected error.");
                response.Issues.Add(new DetectedIssue
                {
                    Severity = SeverityLevel.Error,
                    Message = $"Analyzer failed: {ex.Message}",
                    Line = null,
                    RuleId = "SYS0001"
                });
            }

            return Task.FromResult(response);
        }

        private static string InferLanguage(BugAnalysisRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Language))
            {
                return request.Language.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.FileName))
            {
                var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
                return ext switch
                {
                    ".cs" => "csharp",
                    ".js" => "javascript",
                    ".ts" => "typescript",
                    ".py" => "python",
                    ".java" => "java",
                    _ => "unknown"
                };
            }

            return "unknown";
        }
    }
}
