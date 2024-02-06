echo test
sc.exe create ".NET Excel Service" binpath="C:\Users\balaji.g\OneDrive - Kryptos\Documents\Timetrigger\TestWorkerService.exe"

sc qfailure ".NET Excel Service" 

sc.exe failure ".NET Excel Service" reset=0 actions=restart/60000/restart/60000/run/1000  

pause
