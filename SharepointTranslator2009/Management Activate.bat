cd C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\BIN
stsadm.exe -o activatefeature -name Translator2009Admin -url %1 -force
cd C:\