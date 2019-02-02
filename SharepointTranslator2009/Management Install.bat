cd C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\BIN
stsadm.exe -o addsolution -filename %1
stsadm.exe -o deploysolution -name Translator2009Admin.wsp -immediate 
stsadm.exe -o execadmsvcjobs
stsadm.exe -o installfeature -name Translator2009Admin -force
cd C:\