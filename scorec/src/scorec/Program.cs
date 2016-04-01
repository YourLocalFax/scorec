using System;
using System.Collections.Generic;
using System.IO;

using LLVMSharp;

namespace ScoreC
{
    using Compile;
    using Compile.Logging;
    using Compile.Source;
    using Compile.SyntaxTree;

    static class Program
    {
        static void Main(string[] args)
        {
            /*  LLVM supported types:

            INT:
            
            LLVM.Int1Type();
            LLVM.Int8Type();
            LLVM.Int16Type();
            LLVM.Int32Type();
            LLVM.Int64Type();
            LLVM.IntType(uint);

            FLOAT:

            LLVM.HalfType();
            LLVM.FloatType();
            LLVM.DoubleType();
            LLVM.X86FP80Type();
            LLVM.FP128Type();
            LLVM.PPCFP128Type();

            * /

            // That's all we have, actually, wait.. how do..
            {
                var context = LLVM.ContextCreate();
                var module = LLVM.ModuleCreateWithNameInContext("module", context);
                // "ContextCreate", "CreateBuilder" such consistent LLVM thanks </3
                var builder = LLVM.CreateBuilderInContext(context);

                var fnTy = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { }, false);
                var fn = LLVM.AddFunction(module, "main", fnTy);

                var entry = LLVM.AppendBasicBlockInContext(context, fn, ".entry");

                LLVM.PositionBuilderAtEnd(builder, entry);
                var test = LLVM.BuildAlloca(builder, LLVM.FP128Type(), "test");
                LLVM.BuildStore(builder, LLVM.ConstRealOfString(LLVM.FP128Type(), "9.9e-99"), test);
                // This works, of course, but what if it doesn't fit into a double?
                var test2 = LLVM.BuildAlloca(builder, LLVM.FP128Type(), "test");
                LLVM.BuildStore(builder, LLVM.ConstReal(LLVM.FP128Type(), 9.9e-99), test2);

                LLVM.DumpModule(module);
            }

            LLVM.ConstIntOfString(LLVM.IntType(128), "18446744073709551616", (char)10);

            //*/

            if (args.Length == 0)
            {
                // throw new ArgumentException("Need file to compile!");
                Console.WriteLine("Expected file to compile. Duh.");
                Exit();
                return;
            }

            var fileName = args[0];

            Project.Create(fileName);
            //Project.Compile();

            Exit();
        }

        private static void Exit()
        {
#if DEBUG
            Console.Write("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
#endif
        }
    }
}
