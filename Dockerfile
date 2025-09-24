FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MovieManager/MovieManager.csproj", "MovieManager/"]
RUN dotnet restore "MovieManager/MovieManager.csproj"
COPY . .
WORKDIR "/src/MovieManager"
RUN dotnet build "MovieManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MovieManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovieManager.dll"]
