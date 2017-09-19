using RakNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
    public class RaknetCSRunTest
    {
        public static void JudgeRaknetCanRun()
        {
            if (!File.Exists("RakNet.dll"))
            {
                string str = "The SWIG build of the DLL has not been copied to the executable directory\nCopy from Swig/SwigWindowsCSharpSample/SwigTestApp/bin/X86/Debug/RakNet.dll to\nSwigWindowsCSharpSample/SwigTestApp/bin/Debug/RakNet.dll\nPress enter to quit.";
                throw new Exception(str);
            }
            try
            {
                RakString dllCallTest = new RakString();
            }
            catch (Exception)
            {
                string str = "DLL issue\nAdd SwigOutput/CplusDLLIncludes/RakNetWrap.cxx to the project\nDLL_Swig/RakNet.sln and rebuild.\nPress enter to quit.";
                throw new Exception(str);
            }
        }
    }
}
