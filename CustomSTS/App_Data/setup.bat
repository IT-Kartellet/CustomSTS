echo off
NET SESSION >nul 2>&1
IF %ERRORLEVEL% EQU 0 (
    IF NOT EXIST "%~dp0\CustomSTSTokenSigningCertificate.cer" (
        call %~dp0\makecert -a SHA256 -len 2048 -r -pe -n "CN=CustomSTSTokenSigningCertificate" -b 01/01/2012 -e 01/01/2032 -eku 1.3.6.1.5.5.7.3.3 -ss my -sr localMachine -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 "%~dp0\CustomSTSTokenSigningCertificate.cer" 
    )
    IF NOT EXIST "%~dp0\..\appSettings.config" (
        call %~dp0\EncryptionKeyGenerator.exe > %~dp0\..\appSettings.config
    )  
) ELSE (
    echo This script needs to be run with Administrator rights
    pause
)

