# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# restore first so the layer is cached until a project file changes
COPY InterviewPal.sln ./
COPY src/InterviewPal.Domain/*.csproj src/InterviewPal.Domain/
COPY src/InterviewPal.Application/*.csproj src/InterviewPal.Application/
COPY src/InterviewPal.Infrastructure/*.csproj src/InterviewPal.Infrastructure/
COPY src/InterviewPal.Api/*.csproj src/InterviewPal.Api/
RUN dotnet restore src/InterviewPal.Api/InterviewPal.Api.csproj

COPY src/ src/
COPY content/ content/
# the question bank and lessons are copied next to the binaries by the Api project
RUN dotnet publish src/InterviewPal.Api/InterviewPal.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# the SQLite database lives in a volume so it survives container restarts
# TLS is terminated by whatever sits in front of the container (nginx, a load balancer)
ENV ASPNETCORE_HTTP_PORTS=8080 \
    HttpsRedirection__Enabled=false \
    ConnectionStrings__Default="Data Source=/data/interviewpal.db"
RUN mkdir /data && chown $APP_UID /data
VOLUME /data
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "InterviewPal.Api.dll"]
