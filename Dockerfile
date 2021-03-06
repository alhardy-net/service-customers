﻿FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["src/Customers.Persistence/Customers.Persistence.csproj", "Customers.Persistence/"]
RUN dotnet restore "Customers.Persistence/Customers.Persistence.csproj"

COPY ["src/Customers.Api/Customers.Api.csproj", "Customers.Api/"]
RUN dotnet restore "Customers.Api/Customers.Api.csproj"

COPY . .
WORKDIR "/src/src/Customers.Api"
RUN dotnet build "Customers.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Customers.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Customers.Api.dll"]
