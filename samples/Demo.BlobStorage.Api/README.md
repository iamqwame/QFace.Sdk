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
- Returns uploaded file URL (private files require pre-signed URLs for access)

#### Example
```bash
curl -X POST -F "file=@/path/to/image.jpg" \
     -F "folder=user-uploads" \
     -F "fileName=profile" \
     http://localhost:5000/api/upload
```

### Public File Upload
`POST /api/upload-public`
- Uploads files as public (accessible directly via CDN URL)
- Perfect for profile pictures, public assets, shared media
- No pre-signed URL needed for access
- Returns both CDN URL and pre-signed URL (for compatibility)

#### Example
```bash
curl -X POST -F "file=@/path/to/profile.jpg" \
     -F "folder=profiles" \
     -F "fileName=avatar" \
     http://localhost:5000/api/upload-public
```

#### Response
```json
{
  "message": "File uploaded as public",
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/profiles/avatar.jpg",
  "preSignedUrl": "https://bucket.region.digitaloceanspaces.com/profiles/avatar.jpg?..."
}
```

### Base64 Image Upload
`POST /api/upload-base64`
- Uploads Base64 encoded images
- Supports both public and private uploads
- Automatic content type detection

#### Example
```bash
curl -X POST -H "Content-Type: application/json" \
     -d '{
       "base64Image": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ...",
       "folder": "images",
       "fileName": "uploaded-image",
       "contentType": "image/jpeg"
     }' \
     http://localhost:5000/api/upload-base64
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
- **Public and private file uploads** - Choose access level per file
- **Direct CDN access** for public files (no pre-signed URLs needed)
- Robust error handling
- Supports multiple storage providers
- Base64 image upload support

## Running the Project
1. Configure `appsettings.json`
2. Run `dotnet restore`
3. Run `dotnet run`
4. Open Swagger UI at `/swagger`

## Security Considerations
- **Public files**: Accessible directly via CDN URL (use for profile pictures, public assets)
- **Private files**: Require pre-signed URLs for temporary access (use for sensitive documents)
- File type and size validation
- Configurable access controls
- Choose appropriate ACL settings based on content sensitivity