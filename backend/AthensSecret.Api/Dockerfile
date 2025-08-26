# Multi-stage build for ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project file first for better caching
COPY AthensSecret.Api.csproj ./
RUN dotnet restore AthensSecret.Api.csproj

# Copy everything else
COPY . ./

# Build and publish
RUN dotnet publish AthensSecret.Api.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published app
COPY --from=build /app/out .

# Expose port (Render will set this automatically)
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "AthensSecret.Api.dll"]
