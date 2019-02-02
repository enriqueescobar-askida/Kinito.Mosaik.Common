SET SETUPDIR="C:\Program Files\Alphamosaik\AlphaMosaik Logger Service\"

XCOPY /Y bin\Debug\*.dll %SETUPDIR%
XCOPY /Y bin\Debug\*.exe %SETUPDIR%
XCOPY /Y bin\Debug\*.config %SETUPDIR%