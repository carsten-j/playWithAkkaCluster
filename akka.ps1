cd seednode
start powershell { dotnet run }

cd ..\worker1
start powershell { dotnet worker.dll }

cd ..\worker2
start powershell { dotnet worker.dll }

cd ..\watchdog
start powershell { dotnet run }
cd ..

