dotnet restore
dotnet build
cd worker
dotnet publish -o ..\worker1
cd ..
mkdir worker2 -ErrorAction Ignore
rm .\Worker2\*
cp .\Worker1\* .\Worker2\