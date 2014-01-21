echo off
SETLOCAL
set path=%path%;C:\Windows\Microsoft.NET\Framework\v4.0.30319\

set msbuild_deploy_args=/t:rebuild /p:DeployOnBuild=True /p:DeployTarget=Package
rem /p:PackageAsSingleFile=true 
rem /p:AutoParameterizationWebConfigConnectionStrings=False
set msbuild_cmd=msbuild /v:normal /nologo /m /p:Platform="Any CPU"

call %msbuild_cmd% %msbuild_deploy_args% "%~dp0\..\CustomSTS.sln" /p:Configuration=Test /p:PackageLocation="%~dp0\package\CustomSTSTest.zip" /t:CustomSTS
if %errorlevel% neq 0 exit /b %errorlevel%
call:createinstall CustomSTSTest
call:zip CustomSTSTest
goto:eof

:zip
del "%~dp0\%~1.zip"
call "%~dp0\zip.exe" -D -j "%~dp0\%~1.zip" %~dp0\package\%~1.install.bat %~dp0\package\%~1.deploy.cmd %~dp0\package\%~1.SetParameters.xml %~dp0\package\%~1.SourceManifest.xml %~dp0\package\%~1.zip
goto:eof

:createinstall
@echo echo off > "%~dp0\package\%~1.Install.bat"
@echo NET SESSION ^>nul 2^>^&1 >> "%~dp0\package\%~1.Install.bat"
@echo IF %%ERRORLEVEL%% EQU 0 ^( >> "%~dp0\package\%~1.Install.bat"
@echo "%%~dp0\%~1.deploy.cmd" -allowUntrusted /Y >> "%~dp0\package\%~1.Install.bat"
@echo pause >> "%~dp0\package\%~1.Install.bat"
@echo ^) ELSE ^( >> "%~dp0\package\%~1.Install.bat"
@echo echo This script needs to be run with Administrator rights >> "%~dp0\package\%~1.Install.bat"
@echo pause >> "%~dp0\package\%~1.Install.bat"
@echo ^) >> "%~dp0\package\%~1.Install.bat"
goto:eof

ENDLOCAL