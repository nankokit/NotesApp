FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NotesApp.sln .
COPY NotesApp.WebApi/NotesApp.WebApi.csproj NotesApp.WebApi/
COPY NotesApp.Application/NotesApp.Application.csproj NotesApp.Application/
COPY NotesApp.Infrastructure/NotesApp.Infrastructure.csproj NotesApp.Infrastructure/
COPY NotesApp.Domain/NotesApp.Domain.csproj NotesApp.Domain/

RUN dotnet restore

COPY . .

WORKDIR /src/NotesApp.WebApi
RUN dotnet publish -c Release -o /app/publish 

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NotesApp.WebApi.dll"]