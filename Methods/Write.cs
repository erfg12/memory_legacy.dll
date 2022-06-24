using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Memory
{
    public class MemWrite : Imps
    {
		Mem m = new Mem();

        ///<summary>
        ///Write to memory address. See https://github.com/erfg12/memory.dll/wiki/writeMemory() for more information.
        ///</summary>
        ///<param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///<param name="type">byte, 2bytes, bytes, float, int, string, double or long.</param>
        ///<param name="write">value to write to address.</param>
        ///<param name="file">path and name of .ini file (OPTIONAL)</param>
        ///<param name="stringEncoding">System.Text.Encoding.UTF8 (DEFAULT = null). Other options: ascii, unicode, utf32, utf7</param>
        ///<param name="RemoveWriteProtection">If building a trainer on an emulator (Ex: RPCS3) you'll want to set this to false</param>
        public bool WriteMemory(string code, string type, string write, string file, System.Text.Encoding stringEncoding, bool RemoveWriteProtection)
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            theCode = m.GetCode(code, file);

            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return false;

            if (type.ToLower() == "float")
            {
                write = Convert.ToString(float.Parse(write, CultureInfo.InvariantCulture));
                memory = BitConverter.GetBytes(Convert.ToSingle(write));
                size = 4;
            }
            else if (type.ToLower() == "int")
            {
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 4;
            }
            else if (type.ToLower() == "byte")
            {
                memory = new byte[1];
                memory[0] = Convert.ToByte(write, 16);
                size = 1;
            }
            else if (type.ToLower() == "2bytes")
            {
                memory = new byte[2];
                memory[0] = (byte)(Convert.ToInt32(write) % 256);
                memory[1] = (byte)(Convert.ToInt32(write) / 256);
                size = 2;
            }
            else if (type.ToLower() == "bytes")
            {
                if (write.LastIndexOf(",")>=0 || write.LastIndexOf(" ")>=0) //check if it's a proper array
                {
                    string[] stringBytes;
                    if (write.LastIndexOf(",")>=0)
                        stringBytes = write.Split(',');
                    else
                        stringBytes = write.Split(' ');
                    //Debug.WriteLine("write:" + write + " stringBytes:" + stringBytes);

                    int c = stringBytes.Length;
                    memory = new byte[c];
                    for (int i = 0; i < c; i++)
                    {
                        memory[i] = Convert.ToByte(stringBytes[i], 16);
                    }
                    size = stringBytes.Length;
                }
                else //wasnt array, only 1 byte
                {
                    memory = new byte[1];
                    memory[0] = Convert.ToByte(write, 16);
                    size = 1;
                }
            }
            else if (type.ToLower() == "double")
            {
                memory = BitConverter.GetBytes(Convert.ToDouble(write));
                size = 8;
            }
            else if (type.ToLower() == "long")
            {
                memory = BitConverter.GetBytes(Convert.ToInt64(write));
                size = 8;
            }
            else if (type.ToLower() == "string")
            {
                if (stringEncoding == null)
                    memory = System.Text.Encoding.UTF8.GetBytes(write);
                else
                    memory = stringEncoding.GetBytes(write);
                size = memory.Length;
            }

            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:" + theCode + "] " + String.Join(",", memory) + Environment.NewLine);
            MemoryProtection OldMemProt = 0x00;
            bool WriteProcMem = false;
            //if (RemoveWriteProtection)
            //    m.ChangeProtection(code, MemoryProtection.ExecuteReadWrite, out OldMemProt, file); // change protection
            WriteProcMem = WriteProcessMemory(m.mProc.Handle, theCode, memory, (UIntPtr)size, IntPtr.Zero);
            //if (RemoveWriteProtection)
            //    m.ChangeProtection(code, OldMemProt, out _, file); // restore
            return WriteProcMem;
        }

        /// <summary>
        /// Write to address and move by moveQty. Good for byte arrays. See https://github.com/erfg12/memory.dll/wiki/Writing-a-Byte-Array for more information.
        /// </summary>
        ///<param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///<param name="type">byte, bytes, float, int, string or long.</param>
        /// <param name="write">byte to write</param>
        /// <param name="MoveQty">quantity to move</param>
        /// <param name="file">path and name of .ini file (OPTIONAL)</param>
        /// <param name="SlowDown">milliseconds to sleep between each byte</param>
        /// <returns></returns>
        public bool WriteMove(string code, string type, string write, int MoveQty, string file, int SlowDown)
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            theCode = m.GetCode(code, file);

            if (type == "float")
            {
                memory = new byte[write.Length];
                memory = BitConverter.GetBytes(Convert.ToSingle(write));
                size = write.Length;
            }
            else if (type == "int")
            {
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 4;
            }
            else if (type == "double")
            {
                memory = BitConverter.GetBytes(Convert.ToDouble(write));
                size = 8;
            }
            else if (type == "long")
            {
                memory = BitConverter.GetBytes(Convert.ToInt64(write));
                size = 8;
            }
            else if (type == "byte")
            {
                memory = new byte[1];
                memory[0] = Convert.ToByte(write, 16);
                size = 1;
            }
            else if (type == "string")
            {
                memory = new byte[write.Length];
                memory = System.Text.Encoding.UTF8.GetBytes(write);
                size = write.Length;
            }

            UIntPtr newCode = m.UIntPtrAdd(theCode, Convert.ToUInt32(MoveQty));

            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:[O]" + theCode + " [N]" + newCode + " MQTY:" + MoveQty + "] " + String.Join(",", memory) + Environment.NewLine);
            Thread.Sleep(SlowDown);
            return WriteProcessMemory(m.mProc.Handle, newCode, memory, (UIntPtr)size, IntPtr.Zero);
        }

        /// <summary>
        /// Write byte array to addresses.
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="write">byte array to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void WriteBytes(string code, byte[] write, string file)
        {
            UIntPtr theCode;
            theCode = m.GetCode(code, file);
            WriteProcessMemory(m.mProc.Handle, theCode, write, (UIntPtr)write.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Takes an array of 8 booleans and writes to a single byte
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="bits">Array of 8 booleans to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void WriteBits(string code, bool[] bits, string file)
        {
            if (bits.Length != 8)
                throw new ArgumentException("Not enough bits for a whole byte");

            byte[] buf = new byte[1];

            UIntPtr theCode = m.GetCode(code, file);

            for (int i = 0; i < 8; i++)
            {
                if (bits[i])
                    buf[0] |= (byte)(1 << i);
            }

            WriteProcessMemory(m.mProc.Handle, theCode, buf, (UIntPtr)1, IntPtr.Zero);
        }

        /// <summary>
        /// Write byte array to address
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="write">Byte array to write to</param>
        public void WriteBytes(UIntPtr address, byte[] write)
        {
			IntPtr bytesRead = new IntPtr();
            WriteProcessMemory(m.mProc.Handle, address, write, (UIntPtr)write.Length, out bytesRead);
        }
    }
}
