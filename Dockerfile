FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RestStop.Api/RestStop.Api.csproj", "src/RestStop.Api/"]
COPY ["src/RestStop.Api.Tests/RestStop.Api.Tests.csproj", "src/RestStop.Api.Tests/"]
RUN dotnet restore "src/RestStop.Api/RestStop.Api.csproj"
COPY . .
WORKDIR "/src/src/RestStop.Api"
RUN dotnet build "RestStop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RestStop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestStop.Api.dll"]
