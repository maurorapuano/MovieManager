FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY MovieManager/*.csproj ./MovieManager/
COPY MovieManager.Tests/*.csproj ./MovieManager.Tests/
RUN dotnet restore

COPY . .
WORKDIR /src/MovieManager
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MovieManager.dll"]