set BuildConfig=%~1

IF exist ".\Dist" ( rmdir /S /Q ".\Dist" )
mkdir ".\Dist"

xcopy /Y /E ".\Trifolia.Web\obj\%BuildConfig%\Package\PackageTmp" ".\Dist\Trifolia.Web\"
xcopy /Y ".\Trifolia.Web\obj\%BuildConfig%\TransformWebConfig\transformed\Web.config" ".\Dist\Trifolia.Web\Web.config"
xcopy ".\packages\EntityFramework.6.1.3\tools\migrate.exe" ".\Dist\Trifolia.Web\bin\"
xcopy ".\*.ps1" ".\Dist\"