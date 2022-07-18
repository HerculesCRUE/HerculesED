FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClasesGeneradas/hercules.csproj", "./ClasesGeneradas/"]
COPY ["Hercules.ED.UpdateKeywords/Hercules.ED.UpdateKeywords.csproj", "./Hercules.ED.UpdateKeywords/"]
RUN dotnet restore "./Hercules.ED.UpdateKeywords/Hercules.ED.UpdateKeywords.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "Hercules.ED.UpdateKeywords/Hercules.ED.UpdateKeywords.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hercules.ED.UpdateKeywords/Hercules.ED.UpdateKeywords.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hercules.ED.UpdateKeywords.dll"]