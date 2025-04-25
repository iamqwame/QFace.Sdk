# Installing QFace SDKs

All QFace SDKs are available as NuGet packages. Follow these steps to add them to your project.

## Requirements

- .NET 6.0 or later
- For Actor System SDK: Akka.NET 1.4.46 or later

## Using the .NET CLI

To install any QFace SDK using the .NET CLI, use the following command:

```bash
# Install Actor System SDK
dotnet add package QFace.Sdk.ActorSystems

# Install Messaging SDK
dotnet add package QFace.Sdk.Messaging

# Install Data Access SDK
dotnet add package QFace.Sdk.DataAccess
```

## Using Visual Studio Package Manager Console

```powershell
# Install Actor System SDK
Install-Package QFace.Sdk.ActorSystems

# Install Messaging SDK
Install-Package QFace.Sdk.Messaging

# Install Data Access SDK
Install-Package QFace.Sdk.DataAccess
```

## Using the Visual Studio UI

1. Right-click on your project in Solution Explorer
2. Select "Manage NuGet Packages..."
3. Click on the "Browse" tab
4. Search for "QFace.Sdk"
5. Select the SDK you want to install
6. Click "Install"

## Verifying Installation

After installation, you should be able to add the appropriate using statements to your code:

```csharp
// For Actor System SDK
using QFace.Sdk.ActorSystems;

// For Messaging SDK
using QFace.Sdk.Messaging;

// For Data Access SDK
using QFace.Sdk.DataAccess;
```

## Next Steps

After installing the packages, refer to the configuration guide for each SDK to set up and start using the functionality in your application.

## Troubleshooting

If you encounter any issues during installation:

1. Make sure your project targets .NET 6.0 or later
2. Verify that your NuGet sources include nuget.org
3. Check for any dependency conflicts in your project

For further assistance, open an issue on our GitHub repository.
