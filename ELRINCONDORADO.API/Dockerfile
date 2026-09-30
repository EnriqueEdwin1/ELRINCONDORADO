# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ELRINCONDORADO.API.csproj", "./"]
RUN dotnet restore "ELRINCONDORADO.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ELRINCONDORADO.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ELRINCONDORADO.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ELRINCONDORADO.API.dll"]
