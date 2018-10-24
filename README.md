## CSharp-Shellcode

# I got inspired by @vysecurity and his FSharp Shell code (https://github.com/vysec/FSharp-Shellcode) to try and get it to 0 detections on virus total.

So I hacked together a few peices of code from:
- https://webstersprodigy.net/2012/08/31/av-evading-meterpreter-shell-from-a-net-service/ - as a base
- https://raw.githubusercontent.com/vysec/FSharp-Shellcode/master/FSharp-Shellcode.fs - as inspiration
- https://stackoverflow.com/questions/1361965/compile-simple-string - as a source of code
- https://www.exploit-db.com/exploits/28996/ - for a msgbox popup shellcode test

# tl;dr; 
C# code to compile another C# code from string and run shellcode 

# Usage
Same as Vincents code: Replace with 32 bit shellcode if compiling as 32 bit, and 64 bit if compiling as 64 bit.

## note: because this code compiles another program using the C# compiler, the new compiled binary will touch disk at a temp folder location so it might get detected by an AV

### For educational purposes only! 
