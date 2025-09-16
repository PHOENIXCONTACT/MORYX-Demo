#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy NuGet configuration and shared project files
COPY ["NuGet.Config", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]

# Copy project-specific files
COPY ["src/StartProject.Asp/StartProject.Asp.csproj", "src/StartProject.Asp/"]
COPY ["src/Moryx.ControlSystem.Demo/Moryx.ControlSystem.Demo.csproj", "src/Moryx.ControlSystem.Demo/"]
COPY ["src/Moryx.Demo/Moryx.Demo.csproj", "src/Moryx.Demo/"]
COPY ["src/Moryx.Orders.Demo/Moryx.Orders.Demo.csproj", "src/Moryx.Orders.Demo/"]
COPY ["src/Moryx.Products.Demo/Moryx.Products.Demo.csproj", "src/Moryx.Products.Demo/"]
COPY ["src/Moryx.Resources.Demo/Moryx.Resources.Demo.csproj", "src/Moryx.Resources.Demo/"]

# Restore dependencies
RUN dotnet restore "./src/StartProject.Asp/StartProject.Asp.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/StartProject.Asp"

# Build the project
RUN dotnet build "./StartProject.Asp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StartProject.Asp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StartProject.Asp.dll"]