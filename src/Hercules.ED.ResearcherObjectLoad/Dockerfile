FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Hercules.CommonsEDMA.DisambiguationEngine/Hercules.CommonsEDMA.DisambiguationEngine.csproj", "./Hercules.CommonsEDMA.DisambiguationEngine/"]
COPY ["ClasesGeneradas/hercules.csproj", "./ClasesGeneradas/"]
COPY ["Hercules.ED.ResearcherObjectLoad/Hercules.ED.ResearcherObjectLoad.csproj", "./Hercules.ED.ResearcherObjectLoad/"]
RUN dotnet restore "./Hercules.ED.ResearcherObjectLoad/Hercules.ED.ResearcherObjectLoad.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "Hercules.ED.ResearcherObjectLoad/Hercules.ED.ResearcherObjectLoad.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hercules.ED.ResearcherObjectLoad/Hercules.ED.ResearcherObjectLoad.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hercules.ED.ResearcherObjectLoad.dll"]