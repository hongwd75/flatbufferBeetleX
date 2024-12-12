FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Flatbuffers.Client/Flatbuffers.Client.csproj", "Flatbuffers.Client/"]
RUN dotnet restore "Flatbuffers.Client/Flatbuffers.Client.csproj"
COPY . .
WORKDIR "/src/Flatbuffers.Client"
RUN dotnet build "Flatbuffers.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Flatbuffers.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Flatbuffers.Client.dll"]
