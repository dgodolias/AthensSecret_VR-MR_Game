# Use .NET SDK for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy backend files
COPY . .

# Copy static content from parent directories
COPY ../../registration_form ./registration_form
COPY ../../frontend_v2 ./frontend_v2

# Restore, build and publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Copy static content to runtime
COPY --from=build /src/registration_form ./registration_form
COPY --from=build /src/frontend_v2 ./frontend_v2

EXPOSE 80
ENTRYPOINT ["dotnet", "AthensSecret.Api.dll"]
