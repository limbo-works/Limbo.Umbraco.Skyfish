@echo off
dotnet build src/Limbo.Umbraco.Skyfish --configuration Debug /t:rebuild /t:pack -p:PackageOutputPath=c:/nuget