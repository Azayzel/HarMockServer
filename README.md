# HarMockServer

## Usage Examples

### 1. Upload a HAR file
```bash
curl -X POST "https://localhost:7000/api/har/upload" \
     -F "harFile=@exported-requests.har" \
     -F "environmentName=My Test Environment"
```

Response:
```json
{
  "environmentId": "a1b2c3d4",
  "message": "HAR file uploaded successfully",
  "mockUrl": "https://localhost:7000/mock/a1b2c3d4"
}
```

### 2. List all environments
```bash
curl -X GET "https://localhost:7000/api/har/environments"
```

### 3. Use mock responses
If your HAR contained a request to `GET https://api.example.com/users/123`, you can now call:
```bash
curl -X GET "https://localhost:7000/mock/a1b2c3d4/users/123"
```

### 4. Get environment details
```bash
curl -X GET "https://localhost:7000/api/har/environments/a1b2c3d4"
```

## Key Features

1. **Environment Isolation**: Each HAR file creates a separate environment with its own mock responses
2. **Domain Replacement**: Original domains are replaced with the local mock server while preserving paths
3. **Fast Lookup**: In-memory dictionary for O(1) response lookup by method+path
4. **YARP Integration**: Uses YARP's IHttpForwarder for efficient request handling
5. **Flexible Matching**: Supports exact path matching and fallback to path-only (ignoring query params)
6. **Production Ready**: Comprehensive logging, error handling, and scalable architecture
7. **RESTful API**: Full CRUD operations for managing HAR environments

## Running the Application

1. Create a new .NET 8 Web API project
2. Replace the default files with the code above
3. Run: `dotnet run`
4. Access Swagger UI at: `https://localhost:7000/swagger`
5. Upload HAR files via the API and start mocking!

This solution provides a complete, scalable HAR mock server that leverages YARP for efficient request processing and serves immediate mock responses for your testing needs.