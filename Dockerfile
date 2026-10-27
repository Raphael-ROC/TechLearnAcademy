FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacao
WORKDIR /codigo
COPY TechLearnAcademy.Api/ TechLearnAcademy.Api/
COPY TechLearnAcademy.Web/wwwroot/ TechLearnAcademy.Web/wwwroot/
COPY Banco/ Banco/
RUN dotnet publish TechLearnAcademy.Api/TechLearnAcademy.Api.csproj -c Release -o /publicacao

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=compilacao /publicacao .
ENV ASPNETCORE_HTTP_PORTS=8080
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "TechLearnAcademy.Api.dll"]

