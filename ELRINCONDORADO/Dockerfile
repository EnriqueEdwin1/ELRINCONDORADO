# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ELRINCONDORADO.csproj", "./"]
RUN dotnet restore "ELRINCONDORADO.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ELRINCONDORADO.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ELRINCONDORADO.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5049
ENV ASPNETCORE_URLS=http://+:5049
ENTRYPOINT ["dotnet", "ELRINCONDORADO.dll"]
