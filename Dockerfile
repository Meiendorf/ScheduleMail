FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 3711
EXPOSE 44332

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ScheduleMail/ScheduleMail.csproj ScheduleMail/
COPY MessageContracts/MessageContracts.csproj MessageContracts/
RUN dotnet restore ScheduleMail/ScheduleMail.csproj
COPY . .
WORKDIR /src/ScheduleMail
RUN dotnet build ScheduleMail.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ScheduleMail.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ScheduleMail.dll"]
