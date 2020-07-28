using ArgumentParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProcDump
{
	class Program
  {
    [DllImport("dbghelp.dll")]
    public static extern bool MiniDumpWriteDump(IntPtr hProcess, int processId, IntPtr hFile, int dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

    public static class MINIDUMPTYPE
    {
      public const int MiniDumpNormal = 0x00000000;
      public const int MiniDumpWithDataSegs = 0x00000001;
      public const int MiniDumpWithFullMemory = 0x00000002;
      public const int MiniDumpWithHandleData = 0x00000004;
      public const int MiniDumpFilterMemory = 0x00000008;
      public const int MiniDumpScanMemory = 0x00000010;
      public const int MiniDumpWithUnloadedModules = 0x00000020;
      public const int MiniDumpWithIndirectlyReferencedMemory = 0x00000040;
      public const int MiniDumpFilterModulePaths = 0x00000080;
      public const int MiniDumpWithProcessThreadData = 0x00000100;
      public const int MiniDumpWithPrivateReadWriteMemory = 0x00000200;
      public const int MiniDumpWithoutOptionalData = 0x00000400;
      public const int MiniDumpWithFullMemoryInfo = 0x00000800;
      public const int MiniDumpWithThreadInfo = 0x00001000;
      public const int MiniDumpWithCodeSegs = 0x00002000;
    }

    static void Main(string[] args)
		{
      ArgParse argparse = new ArgParse
      (
        new ArgItem("dir", "d", false, "The directory where dump will be saved", @"C:\Users\Public", ArgParse.ArgParseType.String),
        new ArgItem("name", "n", false, "Name of process, with out the file extension, that should be dumped.", "", ArgParse.ArgParseType.String),
        new ArgItem("pid", "p", false, "Process ID of the process to be dumped.", "0", ArgParse.ArgParseType.Pid)
      );

      argparse.parse(args);

      int pid = argparse.Get<int>("pid");
      string processName = argparse.Get<string>("name");
      string dir = argparse.Get<string>("dir");
      
      List<Process> ProcList = new List<Process>();

      if (pid == 0)
      {
        Process[] procs = Process.GetProcessesByName(processName);
        ProcList = procs.ToList<Process>();
      }
      else
      {
        Process proc = Process.GetProcessById(pid);
        if (proc != null)
        {
          ProcList.Add(proc);
        }
      }

      string DumpDir = dir;
      if (!Directory.Exists(DumpDir))
      {
        Directory.CreateDirectory(DumpDir);
      }

      foreach (Process p in ProcList)
      {
        if(IntPtr.Size == 4)
        {
          Console.WriteLine("This application should be compiled and run as x64");
          Environment.Exit(0);
        }

        string filename = string.Format("{0}-{1}-{2}.dump", p.ProcessName, p.Id, DateTime.Now.ToString("yyyyMMddHHmmss"));
        string fullpath = Path.Combine(DumpDir, filename);

        using (FileStream fs = new FileStream(fullpath, FileMode.Create))
        {
          bool b = MiniDumpWriteDump(p.Handle, p.Id, fs.SafeFileHandle.DangerousGetHandle(), MINIDUMPTYPE.MiniDumpWithFullMemory, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
          if (!b)
          {
            Console.WriteLine(string.Format("Dump Failed {0}", Marshal.GetLastWin32Error()));
          }
          else
          {
            Console.WriteLine(string.Format("{0} written to {1}", filename, DumpDir));
          }
          fs.Close();
        }
      }
    }
  }
}
