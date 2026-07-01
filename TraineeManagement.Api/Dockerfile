FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY TraineeManagement.Api.Messaging/*.csproj TraineeManagement.Api.Messaging/
COPY TraineeManagement.Api.Data/*.csproj TraineeManagement.Api.Data/
COPY TraineeManagement.Api.Contract/*.csproj TraineeManagement.Api.Contract/
COPY TraineeManagement.Api/*.csproj TraineeManagement.Api/

RUN dotnet restore TraineeManagement.Api/TraineeManagement.Api.csproj 

COPY . .
RUN dotnet publish TraineeManagement.Api/TraineeManagement.Api.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "TraineeManagement.Api.dll"]