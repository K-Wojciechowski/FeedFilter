FROM node:24 AS node-build
WORKDIR /src
COPY src/feedfilter.web.client ./
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /src
COPY src ./
COPY --from=node-build /src/dist ./feedfilter.web.client/dist
WORKDIR /src/FeedFilter.Web.Server
RUN dotnet restore
RUN dotnet publish -c Release -o out -p:PublishReadyToRun=true -p:ShouldRunNpmInstall=false -p:BuildCommand=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=dotnet-build /src/FeedFilter.Web.Server/out .
ENTRYPOINT ["dotnet", "FeedFilter.Web.Server.dll"]
