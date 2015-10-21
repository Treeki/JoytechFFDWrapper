using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace FFDNetServer32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DIHIDFFINITINFO
    {
        public uint dwSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszDeviceInterface;
        public Guid GuidInstance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DIEFFECT
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwDuration;
        public uint dwSamplePeriod;
        public uint dwGain;
        public uint dwTriggerButton;
        public uint dwTriggerRepeatInterval;
        public uint cAxes;
        public IntPtr rgdwAxes;
        public IntPtr rglDirection;
        public IntPtr lpEnvelope;
        public uint cbTypeSpecificParams;
        public IntPtr lpvTypeSpecificParams;
        public uint dwStartDelay;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DIENVELOPE
    {
        public uint dwSize;
        public uint dwAttackLevel;
        public uint dwAttackTime;
        public uint dwFadeLevel;
        public uint dwFadeTime;
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("02538130-898F-11D0-9AD0-00A0C9A06E35")]
    public interface IDirectInputEffectDriver
    {
        void DeviceID(uint dwDIVer, uint dwExternalID, uint fBegin, uint dwInternalId, ref DIHIDFFINITINFO lpDIHIDInitInfo);
        void GetVersions(IntPtr pvers); // contents doesn't matter!
        void Escape(uint dwID, uint dwEffect, IntPtr pesc); // irrelevant :3
        void SetGain(uint dwID, uint dwGain);
        void SendForceFeedbackCommand(uint dwID, uint dwCommand);
        void GetForceFeedbackState(uint dwID, IntPtr pds); // irrelevant
        void DownloadEffect(uint dwID, uint dwEffectID, ref uint pdwEffect, ref DIEFFECT peff, uint dwFlags);
        void DestroyEffect(uint dwID, uint dwEffect);
        void StartEffect(uint dwID, uint dwEffect, uint dwMode, uint dwCount);
        void StopEffect(uint dwID, uint dwEffect);
        void GetEffectStatus(uint dwID, uint dwEffect, ref uint pdwStatus);
    }

    [ComImport, Guid("3E603A28-32FB-45A8-B16A-D6B7C02BA53A")]
    class JoyTechFFDriverClass
    {
    }
}
