# QFace.Sdk.Extensions

A comprehensive collection of extension methods and utility helpers for .NET applications.

## Installation

```shell
dotnet add package QFace.Sdk.Extensions
```

## Features

The extension package includes a wide range of utility methods across several categories:

- **JSON Extensions**: Serialization, deserialization, and JSON manipulation
- **String Extensions**: String manipulation, formatting, and validation
- **Collection Extensions**: Collection manipulation, searching, and transformation
- **DateTime Extensions**: Date and time formatting, calculations, and conversions
- **HTTP Extensions**: HTTP request/response manipulation and cookie management
- **Reflection Extensions**: Type inspection, property access, and dynamic method invocation
- **Security Extensions**: Password generation, hashing, and claims management

## Usage Examples

### JSON Extensions

```csharp
// Serialize an object to JSON
var user = new User { Id = 1, Name = "John" };
string json = user.Serialize();

// Deserialize JSON to an object
User? deserializedUser = json.Deserialize<User>();

// Deep clone an object
var clonedUser = user.DeepClone();
```

### String Extensions

```csharp
// Check if a string is empty
if (str.IsEmpty()) { /* handle empty string */ }

// Convert to slug (URL-friendly string)
string slug = "This is a title!".ToSlug(); // "this-is-a-title"

// Truncate a long string
string truncated = longText.Truncate(100);

// Format to title case
string title = "hello world".ToTitleCase(); // "Hello World"
```

### Collection Extensions

```csharp
// Check if a collection is empty
if (list.IsNullOrEmpty()) { /* handle empty list */ }

// Get distinct items by a selector
var distinctUsers = users.DistinctBy(u => u.Email);

// Split a collection into chunks
var chunks = largeList.Chunk(100);

// Process items in parallel
await tasks.ForEachParallelAsync(async task => {
    await ProcessTaskAsync(task);
});
```

### DateTime Extensions

```csharp
// Check if a date is between two dates
if (date.IsBetween(startDate, endDate)) { /* date is in range */ }

// Get the first/last day of the month
var firstDay = date.FirstDayOfMonth();
var lastDay = date.LastDayOfMonth();

// Format as relative time
string relativeTime = dateTime.ToRelativeTime(); // "2 days ago"

// Get a person's age
int age = birthDate.GetAge();
```

### HTTP Extensions

```csharp
// Get client IP address
string ipAddress = httpContext.GetClientIpAddress();

// Get query string parameter
string? param = httpContext.GetQueryString("id");
int pageSize = httpContext.GetQueryString("pageSize", 10);

// Set a cookie
httpContext.SetCookie("preferences", preferences.Serialize(), 30);

// Write JSON response
await httpContext.WriteJsonAsync(result);
```

### Reflection Extensions

```csharp
// Get property value by name
var value = obj.GetPropertyValue("Name");
string? name = obj.GetPropertyValue<string>("Name");

// Set property value by name
obj.SetPropertyValue("Name", "New Name");

// Get property name from lambda expression
string propName = ReflectionExtensions.GetPropertyName<User, string>(u => u.Name);

// Check if a type has an attribute
bool hasAttribute = typeof(User).HasAttribute<TableAttribute>();
```

### Security Extensions

```csharp
// Generate a random password
string password = SecurityExtensions.GenerateRandomPassword(length: 16);

// Hash a string with SHA-256
string hash = "mysecretvalue".ToSha256Hash();

// Generate a random token
string token = SecurityExtensions.GenerateRandomToken();

// Get user info from claims
string? userId = User.GetUserId();
string? email = User.GetUserEmail();
```

## Result Class

The package includes a simple `Result<T>` class for representing operation results:

```csharp
// Create a successful result
var successResult = Result<User>.Ok(user);

// Create a failed result
var failResult = Result<User>.Fail("User not found");

// Create a not found result
var notFoundResult = Result<User>.NotFound();

// Convert to IResult for API responses
return userResult.ToIResult();
```

## Advanced Techniques

### Chaining Extension Methods

```csharp
// Chain multiple extension methods together
var processedString = input
    .Trim()
    .ToLowerCase()
    .ToSlug()
    .Truncate(50);
```

### Working with Async and Parallel Operations

```csharp
// Process a collection of items asynchronously
await items.ForEachAsync(async item => {
    await ProcessItemAsync(item);
});

// Process items in parallel with limited concurrency
await items.ForEachParallelAsync(
    async item => await ProcessItemAsync(item),
    maxDegreeOfParallelism: 5
);
```

## Integration with QFace.Sdk

This extensions package is designed to work seamlessly with other QFace.Sdk packages:

```csharp
// Using with QFace.Sdk.BlobStorage
var fileUrl = await fileUploadService.UploadFileAsync(file, "uploads");
if (fileUrl.IsNotEmpty()) {
    // Successfully uploaded
}

// Using with QFace.Sdk.SendMessage
var replacements = new Dictionary<string, string> {
    { "Name", user.GetPropertyValue<string>("Name") ?? "User" }
};
var command = SendEmailCommand.CreateWithTemplate(
    user.Email,
    "Welcome!",
    welcomeTemplate,
    replacements
);
serviceProvider.SendEmail(command);
```

## License

This package is licensed under the MIT License - see the LICENSE file for details.
