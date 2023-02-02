FROM mcr.microsoft.com/dotnet/aspnet:6.0 as base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ./*.sln .
COPY Blog.Api/*.csproj ./Blog.Api/
COPY Blog.Features/*.csproj ./Blog.Features/
COPY Blog.Models/*.csproj ./Blog.Models/
COPY Blog.Persistence/*.csproj ./Blog.Persistence/

RUN dotnet restore
COPY . .
WORKDIR /src
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Blog.Api.dll"]

