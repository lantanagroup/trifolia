set BaseDir=%~1
set BuildConfig=%~2

IF exist "%BaseDir%Dist" ( rmdir /S /Q "%BaseDir%Dist" )
mkdir "%BaseDir%Dist"

xcopy /Y /E "%BaseDir%Trifolia.Web\obj\%BuildConfig%\Package\PackageTmp" "%BaseDir%Dist\Trifolia.Web\"
xcopy /Y "%BaseDir%Trifolia.Web\obj\%BuildConfig%\TransformWebConfig\transformed\Web.config" "%BaseDir%Dist\Trifolia.Web\Web.config"
xcopy "%BaseDir%packages\EntityFramework.6.1.3\tools\migrate.exe" "%BaseDir%Dist\Trifolia.Web\bin\"
xcopy "%BaseDir%*.ps1" "%BaseDir%Dist\"