# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AthensSecret.Api.csproj", "."]
RUN dotnet restore "AthensSecret.Api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AthensSecret.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AthensSecret.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AthensSecret.Api.dll"]
