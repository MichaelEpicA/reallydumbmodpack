@echo off 
taskkill /f /im "Among Us.exe"
echo Among Us Path: %1
echo Updating...
cd %1
cd BepInEx
cd plugins
del ExampleMod.dll
ren ExampleMod_update.dll ExampleMod.dll
echo Update Complete!
cd ..
cd ..
start "" "explorer.exe Among Us.exe" 
exit
