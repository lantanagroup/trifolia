set BuildConfig=%~1

IF exist ".\Dist" ( rmdir /S /Q ".\Dist" )
mkdir ".\Dist"

xcopy /Y /E ".\Trifolia.Web\obj\%BuildConfig%\Package\PackageTmp" ".\Dist\Trifolia.Web\"
xcopy /Y ".\Trifolia.Web\obj\%BuildConfig%\TransformWebConfig\transformed\Web.config" ".\Dist\Trifolia.Web\Web.config"
xcopy ".\packages\EntityFramework.6.1.3\tools\migrate.exe" ".\Dist\Trifolia.Web\bin\"
xcopy ".\*.ps1" ".\Dist\"

@echo off
copy /y nul Dist\manifest.txt
setlocal disableDelayedExpansion
for /f "delims=" %%A in ('forfiles /p .\Dist\Trifolia.Web /s /c "cmd /c echo @relpath"') do (
  set "file=%%~A"
  setlocal enableDelayedExpansion
  echo !file:~2! >> Dist\manifest.txt
  endlocal
)
move Dist\manifest.txt Dist\Trifolia.Web\