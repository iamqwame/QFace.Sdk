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

This demo showcases **all six upload methods** available in QFace.Sdk.BlobStorage:

### Explicit Private Uploads (Recommended)

#### Private File Upload
`POST /api/upload`
- Uploads files as private (requires pre-signed URL for access)
- Uses dedicated `UploadPrivateFileAsync()` method
- Validates file type and size
- Returns CDN URL and pre-signed URL

#### Example
```bash
curl -X POST -F "file=@/path/to/image.jpg" \
     -F "folder=user-uploads" \
     -F "fileName=profile" \
     http://localhost:5000/api/upload
```

#### Response
```json
{
  "message": "File uploaded as private",
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/user-uploads/profile.jpg",
  "preSignedUrl": "https://bucket.region.digitaloceanspaces.com/user-uploads/profile.jpg?..."
}
```

### Explicit Public Uploads (Recommended)

#### Public File Upload
`POST /api/upload-public`
- Uploads files as public (accessible directly via CDN URL)
- Uses dedicated `UploadPublicFileAsync()` method
- Perfect for profile pictures, public assets, shared media
- No pre-signed URL needed for access
- Returns only CDN URL (no pre-signed URL)

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
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/profiles/avatar.jpg"
}
```

### Legacy Methods (Backward Compatibility)

#### Legacy Private File Upload
`POST /api/upload-legacy`
- Demonstrates original `UploadFileAsync()` method
- Private by default (same as explicit private)
- Supports more file types (PDF, DOC, DOCX)
- Returns CDN URL and pre-signed URL

#### Example
```bash
curl -X POST -F "file=@/path/to/document.pdf" \
     -F "folder=legacy-docs" \
     -F "fileName=report" \
     http://localhost:5000/api/upload-legacy
```

#### Response
```json
{
  "message": "File uploaded using legacy method (private by default)",
  "method": "UploadFileAsync() - Legacy",
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/legacy-docs/report.pdf",
  "preSignedUrl": "https://bucket.region.digitaloceanspaces.com/legacy-docs/report.pdf?..."
}
```

#### Legacy Private Base64 Upload
`POST /api/upload-legacy-base64`
- Demonstrates original `UploadBase64ImageAsync()` method
- Private by default (same as explicit private)
- Returns CDN URL and pre-signed URL

#### Example
```bash
curl -X POST -H "Content-Type: application/json" \
     -d '{
       "base64Image": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ...",
       "folder": "legacy/images",
       "fileName": "legacy-image",
       "contentType": "image/jpeg"
     }' \
     http://localhost:5000/api/upload-legacy-base64
```

#### Response
```json
{
  "message": "Base64 image uploaded using legacy method (private by default)",
  "method": "UploadBase64ImageAsync() - Legacy",
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/legacy/images/legacy-image.jpg",
  "preSignedUrl": "https://bucket.region.digitaloceanspaces.com/legacy/images/legacy-image.jpg?..."
}
```

#### Private Base64 Image Upload
`POST /api/upload-base64`
- Uploads Base64 encoded images as private files
- Uses dedicated `UploadPrivateBase64ImageAsync()` method
- Automatic content type detection
- Returns CDN URL and pre-signed URL

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

#### Response
```json
{
  "message": "Base64 image uploaded as private",
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/images/uploaded-image.jpg",
  "preSignedUrl": "https://bucket.region.digitaloceanspaces.com/images/uploaded-image.jpg?..."
}
```

#### Public Base64 Image Upload
`POST /api/upload-public-base64`
- Uploads Base64 encoded images as public files
- Uses dedicated `UploadPublicBase64ImageAsync()` method
- Accessible directly via CDN URL
- Perfect for profile pictures, logos, public assets
- Returns only CDN URL (no pre-signed URL)

#### Example
```bash
curl -X POST -H "Content-Type: application/json" \
     -d '{
       "base64Image": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ...",
       "folder": "public/images",
       "fileName": "logo",
       "contentType": "image/jpeg"
     }' \
     http://localhost:5000/api/upload-public-base64
```

#### Response
```json
{
  "message": "Base64 image uploaded as public",
  "cdnUrl": "https://bucket.region.cdn.digitaloceanspaces.com/public/images/logo.jpg"
}
```

### API Methods Summary
`GET /api/methods`
- Returns a complete overview of all available upload methods
- Shows which endpoint demonstrates each method
- Provides usage recommendations

#### Example
```bash
curl http://localhost:5000/api/methods
```

#### Response
```json
{
  "title": "QFace.Sdk.BlobStorage - Complete API Methods",
  "description": "This demo showcases all available upload methods",
  "methods": [
    {
      "method": "UploadFileAsync()",
      "endpoint": "POST /api/upload-legacy",
      "type": "Legacy",
      "access": "Private (default)",
      "description": "Original method - private by default"
    },
    {
      "method": "UploadPublicFileAsync()",
      "endpoint": "POST /api/upload-public",
      "type": "Explicit",
      "access": "Public",
      "description": "Explicit public file upload (direct CDN access)"
    }
  ],
  "usage": {
    "recommendation": "Use explicit methods (UploadPrivate* and UploadPublic*) for clarity",
    "legacy": "Legacy methods still work for backward compatibility",
    "publicFiles": "Profile pictures, logos, public assets - use UploadPublic* methods",
    "privateFiles": "Documents, personal data, sensitive content - use UploadPrivate* methods"
  }
}
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
- **Complete API demonstration** - All 6 upload methods showcased
- **Explicit public and private uploads** - Choose access level per file
- **Direct CDN access** for public files (no pre-signed URLs needed)
- **Legacy method support** - Backward compatibility maintained
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