FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["CPU-Benchmark-Database-Aggregator/CPU-Benchmark-Database-Aggregator.csproj", "CPU-Benchmark-Database-Aggregator/"]
RUN dotnet restore "CPU-Benchmark-Database-Aggregator/CPU-Benchmark-Database-Aggregator.csproj"
COPY . .
WORKDIR "/src/CPU-Benchmark-Database-Aggregator"
RUN dotnet build "CPU-Benchmark-Database-Aggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CPU-Benchmark-Database-Aggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CPU-Benchmark-Database-Aggregator.dll"]