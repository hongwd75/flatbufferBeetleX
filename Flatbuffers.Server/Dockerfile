FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Flatbuffers.Server/Flatbuffers.Server.csproj", "Flatbuffers.Server/"]
RUN dotnet restore "Flatbuffers.Server/Flatbuffers.Server.csproj"
COPY . .
WORKDIR "/src/Flatbuffers.Server"
RUN dotnet build "Flatbuffers.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Flatbuffers.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Flatbuffers.Server.dll"]
