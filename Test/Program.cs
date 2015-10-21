using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Test
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DIHIDFFINITINFO
    {
        uint dwSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        string pwszDeviceInterface;
        Guid GuidInstance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DIEFFECT
    {
        uint dwSize;
        uint dwFlags;
        uint dwDuration;
        uint dwSamplePeriod;
        uint dwGain;
        uint dwTriggerButton;
        uint dwTriggerRepeatInterval;
        uint cAxes;
        IntPtr rgdwAxes;
        IntPtr rglDirection;
        IntPtr lpEnvelope;
        uint cbTypeSpecificParams;
        IntPtr lpvTypeSpecificParams;
        uint dwStartDelay;
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("02538130-898F-11D0-9AD0-00A0C9A06E35")]
    public interface IDirectInputEffectDriver
    {
        void DeviceID(uint dwDIVer, uint dwExternalID, uint fBegin, uint dwInternalId, DIHIDFFINITINFO lpDIHIDInitInfo);
        void GetVersions(IntPtr pvers); // contents doesn't matter!
        void Escape(uint dwID, uint dwEffect, IntPtr pesc); // irrelevant :3
        void SetGain(uint dwID, uint dwGain);
        void SendForceFeedbackCommand(uint dwID, uint dwCommand);
        void GetForceFeedbackState(uint dwID, IntPtr pds); // irrelevant
        void DownloadEffect(uint dwID, uint dwEffectID, ref uint pdwEffect, IntPtr peff, uint dwFlags);
        void DestroyEffect(uint dwID, uint dwEffect);
        void StartEffect(uint dwID, uint dwEffect, uint dwMode, uint dwCount);
        void StopEffect(uint dwID, uint dwEffect);
        void GetEffectStatus(uint dwID, uint dwEffect, ref uint pdwStatus);
    }
    [Guid("087D06F8-3A16-4FA9-9EFE-0B73771BB34D"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICSSimpleObject
    {
        void DeviceID(uint dwDIVer, uint dwExternalID, uint fBegin, uint dwInternalId, DIHIDFFINITINFO lpDIHIDInitInfo);
        void GetVersions(IntPtr pvers); // contents doesn't matter!
        void Escape(uint dwID, uint dwEffect, IntPtr pesc); // irrelevant :3
        void SetGain(uint dwID, uint dwGain);
        void SendForceFeedbackCommand(uint dwID, uint dwCommand);
        void GetForceFeedbackState(uint dwID, IntPtr pds); // irrelevant
        void DownloadEffect(uint dwID, uint dwEffectID, ref uint pdwEffect, IntPtr peff, uint dwFlags);
        void DestroyEffect(uint dwID, uint dwEffect);
        void StartEffect(uint dwID, uint dwEffect, uint dwMode, uint dwCount);
        void StopEffect(uint dwID, uint dwEffect);
        void GetEffectStatus(uint dwID, uint dwEffect, ref uint pdwStatus);
    }
    class Program
    {
        static void Main(string[] args)
        {
            //var asdf = Activator.CreateInstance(Type.GetTypeFromProgID())
            Console.WriteLine("Spawning...");
            dynamic asdf = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("61DE6C37-2FE8-43F2-832F-E222EDC872DB")));
            Console.WriteLine("Spawned...");
            ICSSimpleObject ics = (ICSSimpleObject)asdf;
            Console.WriteLine("beep :3");

            //Console.WriteLine(asdf.HelloWorld());
            Console.ReadKey();
        }
    }
}
