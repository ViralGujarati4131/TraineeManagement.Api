FROM docker-registry-002.zeuslearning.com/zeuslearning/dotnet/sdk:9.0 AS build
# Replace with the zeus code artificat
WORKDIR /src

COPY nuget.config .
COPY TraineeManagement.Api.Messaging/*.csproj TraineeManagement.Api.Messaging/
COPY TraineeManagement.Api.Data/*.csproj TraineeManagement.Api.Data/
COPY TraineeManagement.Api.Contract/*.csproj TraineeManagement.Api.Contract/
COPY TraineeManagement.Api/*.csproj TraineeManagement.Api/

RUN dotnet restore TraineeManagement.Api/TraineeManagement.Api.csproj 

COPY . .
RUN dotnet publish TraineeManagement.Api/TraineeManagement.Api.csproj -c Release -o /app/build

FROM docker-registry-002.zeuslearning.com/zeuslearning/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "TraineeManagement.Api.dll"]