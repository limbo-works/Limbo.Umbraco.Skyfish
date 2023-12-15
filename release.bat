@echo off
dotnet build src/Limbo.Umbraco.Skyfish --configuration Release /t:rebuild /t:pack -p:PackageOutputPath=../../releases/nuget