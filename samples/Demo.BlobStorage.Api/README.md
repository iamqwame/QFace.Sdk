# QFace Blob Storage Minimal API Demo

## Prerequisites
- .NET 8.0 SDK
- S3-compatible Storage Account (Digital Ocean, AWS, etc.)

## Project Structure
- Minimal API approach
- Built with .NET 8.0
- Swagger/OpenAPI integration
- Blob Storage extensions

## Configuration
Update `appsettings.json` with your blob storage credentials:
```json
"BlobStorage": {
  "Provider": "DigitalOcean", 
  "ServiceURL": "https://nyc3.digitaloceanspaces.com",
  "Region": "nyc3",
  "Credentials": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  },
  "Bucket": {
    "Name": "your-bucket-name"
  }
}
```

## Endpoints

### File Upload
`POST /api/upload`
- Supports file upload with optional folder and filename
- Validates file type and size
- Returns uploaded file URL

#### Example
```bash
curl -X POST -F "file=@/path/to/image.jpg" \
     -F "folder=user-uploads" \
     -F "fileName=profile" \
     http://localhost:5000/api/upload
```

### Get Pre-Signed URL
`GET /api/presigned-url`
- Generates temporary access URL for a file
- Configurable expiration time

#### Example
```bash
curl "http://localhost:5000/api/presigned-url?fileKey=path/to/file.jpg&expirationMinutes=30"
```

### Delete File
`DELETE /api/delete`
- Removes file from blob storage

#### Example
```bash
curl -X DELETE "http://localhost:5000/api/delete?fileUrl=https://your-cdn-url/path/to/file.jpg"
```

## Key Features
- Minimal API design
- Swagger UI for API exploration
- Flexible blob storage configuration
- Robust error handling
- Supports multiple storage providers

## Running the Project
1. Configure `appsettings.json`
2. Run `dotnet restore`
3. Run `dotnet run`
4. Open Swagger UI at `/swagger`

## Security Considerations
- Pre-signed URLs for temporary access
- File type and size validation
- Configurable access controls