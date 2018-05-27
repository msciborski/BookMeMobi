#Builder stage
FROM microsoft/aspnetcore-build AS builder-stage

#Set working directory to /app
WORKDIR /app

#Copy project from the source to the docker container filesystem
COPY ./BookMeMobi2.MobiMetadata/BookMeMobi2.MobiMetadata.csproj ./BookMeMobi2.MobiMetadata/
RUN dotnet restore --disable-parallel ./BookMeMobi2.MobiMetadata/

COPY ./BookMeMobi2.SendGrid/BookMeMobi2.SendGrid.csproj ./BookMeMobi2.SendGrid/
RUN dotnet restore --disable-parallel ./BookMeMobi2.SendGrid/

COPY ./BookMeMobi2/BookMeMobi2.csproj ./BookMeMobi2/
RUN dotnet restore --disable-parallel ./BookMeMobi2/

COPY ./BookMeMobi2.MobiMetadata/ ./BookMeMobi2.MobiMetadata/
RUN dotnet build ./BookMeMobi2.MobiMetadata/

COPY ./BookMeMobi2.SendGrid/ ./BookMeMobi2.SendGrid/
RUN dotnet build ./BookMeMobi2.SendGrid/

COPY ./BookMeMobi2/ ./BookMeMobi2/
RUN dotnet build ./BookMeMobi2/

#Publish
RUN dotnet publish ./BookMeMobi2/ -o /app/out --configuration Release

#Publish stage
FROM microsoft/aspnetcore AS runtime-stage
WORKDIR /app
COPY --from=builder-stage /app/out .

#Entry point
ENTRYPOINT ["dotnet", "BookMeMobi2.dll"]






