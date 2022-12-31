FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Connr/Connr.csproj", "Connr/"]
RUN dotnet restore "Connr/Connr.csproj"
COPY . .
WORKDIR "/src/Connr"
RUN dotnet build "Connr.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Connr.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Connr.dll"]
