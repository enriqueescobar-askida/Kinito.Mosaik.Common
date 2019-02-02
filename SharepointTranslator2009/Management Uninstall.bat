cd C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\BIN
stsadm.exe -o uninstallfeature -name Translator2009Admin -force
stsadm.exe -o retractsolution -name Translator2009Admin.wsp -immediate
stsadm.exe -o execadmsvcjobs
stsadm.exe -o deletesolution -name Translator2009Admin.wsp -override


