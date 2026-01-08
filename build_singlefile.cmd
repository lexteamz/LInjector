@echo off
set "PROJECT_PATH=LInjector\LInjector\LInjector.csproj"
set "BUILD_FOLDER=build"
set "INTERNAL_PROJ_DIR=LInjector\LInjector"
set "TEMP_PUB=temp_publish"

echo [1/4] Cleaning previous builds
if exist "%BUILD_FOLDER%" rd /S /Q "%BUILD_FOLDER%"
if exist "%TEMP_PUB%" rd /S /Q "%TEMP_PUB%"

echo [2/4] Publishing Single-File .NET 9 Executable
dotnet publish "%PROJECT_PATH%" -r win-x64 -c Release --self-contained false -p:PublishSingleFile=true -p:DisableFody=false -p:IncludeNativeLibrariesForSelfExtract=true -o "%TEMP_PUB%"

if exist "%TEMP_PUB%\LInjector.exe" (
    echo [3/4] Moving executable to /build folder
    mkdir "%BUILD_FOLDER%"
    move /Y "%TEMP_PUB%\LInjector.exe" "%BUILD_FOLDER%\LInjector.exe"
    rd /S /Q "%TEMP_PUB%"

    echo [4/4] Wiping /bin and /obj folders
    if exist "%INTERNAL_PROJ_DIR%\bin" rd /S /Q "%INTERNAL_PROJ_DIR%\bin"
    if exist "%INTERNAL_PROJ_DIR%\obj" rd /S /Q "%INTERNAL_PROJ_DIR%\obj"

    echo.
    echo =======================================================
    echo SUCCESS: LInjector.exe is ready in /%BUILD_FOLDER%
    echo Source folders are clean.
    echo =======================================================
) else (
    echo.
    echo ERROR: Build failed. Check the logs above.
)
pause