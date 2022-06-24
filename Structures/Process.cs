using System;
using System.Diagnostics;
using System.Text;

namespace Memory
{
    /// <summary>
    /// Information about the opened process.
    /// </summary>
    public class Proc
    {
		private Process process = null;
		private IntPtr handle = IntPtr.Zero;
		private bool is64bit = false;
		private ProcessModule mainmodule = null;

		public Process Process { get { return process; } set { process = value; } }
		public IntPtr Handle { get { return handle; } set { handle = value; } }
        public bool Is64Bit { get { return is64bit; } set { is64bit = value; }}
        //public ConcurrentDictionary<string, IntPtr> Modules { get; set; } // Use mProc.Process.Modules instead
        public ProcessModule MainModule { get { return mainmodule; } set { mainmodule = value; } }
    }
}
