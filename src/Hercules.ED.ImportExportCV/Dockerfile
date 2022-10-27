#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
#COPY ["EditorCV.csproj", ""]
#RUN dotnet restore "./EditorCV.csproj"
#COPY ["Hercules.CommonsEDMA.DisambiguationEngine/Hercules.CommonsEDMA.DisambiguationEngine.csproj", "./Hercules.CommonsEDMA.DisambiguationEngine/"]
COPY ["Hercules.ED.ImportExportCV/Hercules.ED.ImportExportCV.csproj", "./Hercules.ED.ImportExportCV/"]
RUN dotnet restore "./Hercules.ED.ImportExportCV/Hercules.ED.ImportExportCV.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "Hercules.ED.ImportExportCV/Hercules.ED.ImportExportCV.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hercules.ED.ImportExportCV/Hercules.ED.ImportExportCV.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hercules.ED.ImportExportCV.dll"]

#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#WORKDIR /src
#COPY ["Hercules.ED.ImportExportCV.csproj", ""]
#RUN dotnet restore "./Hercules.ED.ImportExportCV.csproj"
#COPY . .
#WORKDIR "/src/."
#RUN dotnet build "Hercules.ED.ImportExportCV.csproj" -c Release -o /app/build

#FROM build AS publish
#RUN dotnet publish "Hercules.ED.ImportExportCV.csproj" -c Release -o /app/publish

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Hercules.ED.ImportExportCV.dll"]