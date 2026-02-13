# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/StackOverflow.Web/StackOverflow.Web.csproj ./StackOverflow.Web/
RUN dotnet restore ./StackOverflow.Web/StackOverflow.Web.csproj

# Copy everything else and build
COPY src/StackOverflow.Web/ ./StackOverflow.Web/
WORKDIR /src/StackOverflow.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Use the built-in non-root user from the .NET image
USER $APP_UID

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "StackOverflow.Web.dll"]
