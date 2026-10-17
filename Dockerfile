# Step 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["SpendSmart.csproj", "./"]
RUN dotnet restore "SpendSmart.csproj"

# Copy all source files and build
COPY . .
RUN dotnet build "SpendSmart.csproj" -c Release -o /app/build

# Step 2: Publish stage
FROM build AS publish
RUN dotnet publish "SpendSmart.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Step 3: Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpendSmart.dll"]