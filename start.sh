#!/bin/bash
export ASPNETCORE_URLS="http://+:${PORT:-8080}"
dotnet PMO.API.dll
