#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Aplikacja webowa do zarządzania zespołami.csproj", "Aplikacja webowa do zarządzania zespołami/"]
RUN dotnet restore "Aplikacja webowa do zarządzania zespołami/Aplikacja webowa do zarządzania zespołami.csproj"


WORKDIR "/src/Aplikacja webowa do zarządzania zespołami"
COPY . .
RUN dotnet build "Aplikacja webowa do zarządzania zespołami.csproj" -c Release -o /app/build

RUN dotnet tool install --global dotnet-ef --version 7.0.9

ENV PATH="/root/.dotnet/tools:${PATH}"

FROM build AS publish
RUN dotnet publish "Aplikacja webowa do zarządzania zespołami.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Aplikacja webowa do zarządzania zespołami.dll"]
