FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5003


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AdoptionManagerMS/AdoptionManagerMS.csproj", "AdoptionManagerMS/"]
RUN dotnet restore "AdoptionManagerMS/AdoptionManagerMS.csproj"
COPY . .
WORKDIR "/src/AdoptionManagerMS"
RUN dotnet build "AdoptionManagerMS.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AdoptionManagerMS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdoptionManagerMS.dll"]
