set BaseDir=%~1
set BuildConfig=%~2

IF exist "%BaseDir%Dist" ( rmdir /S /Q "%BaseDir%Dist" )
mkdir "%BaseDir%Dist"
del /q /s "%BaseDir%Dist\*"
xcopy /Y "%BaseDir%Trifolia.Web\obj\%BuildConfig%\TransformWebConfig\transformed\Web.config" "%BaseDir%Trifolia.Web\obj\%BuildConfig%\Package\PackageTmp\Web.config"

xcopy /Y /E "%BaseDir%Trifolia.Web\obj\%BuildConfig%\Package\PackageTmp" "%BaseDir%Dist\Trifolia.Web\"
xcopy /Y /E "%BaseDir%Trifolia.Web\Help" "%BaseDir%Dist\Trifolia.Web\Help\"

xcopy /Y /E "%BaseDir%Database" "%BaseDir%Dist\Database\"

xcopy *.ps1 "%BaseDir%Dist\"