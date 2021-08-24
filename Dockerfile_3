FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["src/Customers.Persistence/Customers.Persistence.csproj", "Customers.Persistence/"]
RUN dotnet restore "Customers.Persistence/Customers.Persistence.csproj"

COPY ["src/Customers.Worker/Customers.Worker.csproj", "Customers.Worker/"]
RUN dotnet restore "Customers.Worker/Customers.Worker.csproj"

COPY . .
WORKDIR "/src/src/Customers.Worker"
RUN dotnet build "Customers.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Customers.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Customers.Worker.dll"]
