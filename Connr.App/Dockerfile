FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Connr.App/Connr.App.csproj", "Connr.App/"]
RUN dotnet restore "Connr.App/Connr.App.csproj"
COPY . .
WORKDIR "/src/Connr.App"
RUN dotnet build "Connr.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Connr.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Connr.dll"]
