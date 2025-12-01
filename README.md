# Bug Detection Tool — Backend (Ocean Professional)

A modern, minimal .NET 8 API that analyzes code snippets and returns detected issues. This container hosts the bug detection backend and exposes RESTful endpoints.

Preview Docs: http://localhost:3001/docs  
OpenAPI JSON: http://localhost:3001/openapi.json  
Health: http://localhost:3001/health

## Endpoints

- GET /health
  - Returns service health.
  - Response:
    ```json
    { "status": "healthy", "name": "bug-detector-backend", "version": "v1" }
    ```

- POST /api/bugs/analyze
  - Analyze a code snippet using a rule-based/static analyzer (placeholder).
  - Request body:
    ```json
    {
      "language": "javascript",
      "code": "function test(){ console.log('debug'); }",
      "fileName": "test.js"
    }
    ```
  - Response body:
    ```json
    {
      "language": "javascript",
      "issues": [
        { "severity": 1, "message": "Debug print/log statement detected.", "line": 1, "ruleId": "GEN0002" }
      ],
      "count": 1,
      "isClean": false
    }
    ```

## Quickstart

- Requirements: .NET 8 SDK
- Run:
  ```
  dotnet run --project bug_detector_backend
  ```
- Navigate to:
  - Swagger UI: http://localhost:3001/docs
  - Health: http://localhost:3001/health

Port 3001 is pre-configured for the preview profile.

## Curl Examples

- Health
  ```
  curl -s http://localhost:3001/health
  ```

- Analyze (JavaScript)
  ```
  curl -s -X POST http://localhost:3001/api/bugs/analyze \
    -H 'Content-Type: application/json' \
    -d '{
      "language": "javascript",
      "code": "function test(){ console.log(42); }"
    }' | jq .
  ```

- Analyze (Python)
  ```
  curl -s -X POST http://localhost:3001/api/bugs/analyze \
    -H 'Content-Type: application/json' \
    -d "{
      \"language\": \"python\",
      \"code\": \"try:\\n    risky()\\nexcept:\\n    pass\\n\"
    }"
  ```

## Notes

- This is a starter, rule-based analyzer with straightforward patterns (debug prints, TODOs, naive secret detection, and a few language-specific checks).
- No external services or environment variables are required.
- Style: Ocean Professional — clean naming, simple structure, and clear API docs.
