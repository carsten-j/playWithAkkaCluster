cd seednode
start powershell { dotnet run }

cd ..\worker1
start powershell { dotnet worker.dll }
Start-Sleep -Seconds 5

cd ..\worker2
start powershell { dotnet worker.dll }
Start-Sleep -Seconds 5

cd ..\watchdog
start powershell { dotnet run }
cd ..

