using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FFDWrapper64
{
    static class Magic
    {
        public static void Log(string a)
        {
            System.IO.File.AppendAllText("D:\\crap\\dbg.txt", a);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DIHIDFFINITINFO
    {
        public uint dwSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszDeviceInterface;
        public Guid GuidInstance;
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
        void DownloadEffect(uint dwID, uint dwEffectID, ref uint pdwEffect, ref DIEFFECT peff, uint dwFlags);
        void DestroyEffect(uint dwID, uint dwEffect);
        void StartEffect(uint dwID, uint dwEffect, uint dwMode, uint dwCount);
        void StopEffect(uint dwID, uint dwEffect);
        void GetEffectStatus(uint dwID, uint dwEffect, ref uint pdwStatus);
    }
    /*[Guid("087D06F8-3A16-4FA9-9EFE-0B73771BB34D"), ComVisible(true)]
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
    }*/

    static class Extensions
    {
        public static void WriteInt(this Socket socket, int value)
        {
            byte a = (byte)(value & 0xFF);
            byte b = (byte)(value >> 8);
            byte c = (byte)(value >> 16);
            byte d = (byte)(value >> 24);
            socket.Send(new byte[] { a, b, c, d });
        }
    }

    [ComVisible(true),
    Guid("3E603A28-32FB-45A8-B16A-D6B7C02BA53A")]
    public class JoytechFFD : IDirectInputEffectDriver
    {
        /*private ICSSimpleObject _i = null;
        private dynamic _obj = null;*/
        private TcpClient _client = null;

        public JoytechFFD()
        {
            Magic.Log("Constructed\r\n");
            //_obj = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("61DE6C37-2FE8-43F2-832F-E222EDC872DB")));
            // _i = (ICSSimpleObject)_obj;
            _client = new TcpClient("localhost", 9911);
            Magic.Log("Yip!\r\n");
        }
        ~JoytechFFD()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
            Magic.Log("Destroyed\r\n");
        }

        public void DeviceID(uint dwDIVer, uint dwExternalID, uint fBegin, uint dwInternalId, DIHIDFFINITINFO lpDIHIDInitInfo)
        {
            Magic.Log("DeviceID called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)1);
            fmt.Serialize(ms, dwDIVer);
            fmt.Serialize(ms, dwExternalID);
            fmt.Serialize(ms, fBegin);
            fmt.Serialize(ms, dwInternalId);
            fmt.Serialize(ms, lpDIHIDInitInfo.dwSize);
            fmt.Serialize(ms, lpDIHIDInitInfo.pwszDeviceInterface);
            fmt.Serialize(ms, lpDIHIDInitInfo.GuidInstance);
            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);
        }

        public void GetVersions(IntPtr pvers)
        {
            // This does nothing in the driver
        }

        public void Escape(uint dwID, uint dwEffect, IntPtr pesc)
        {
            // This throws E_NOTIMPL in driver
            throw new NotImplementedException();
        }

        public void SetGain(uint dwID, uint dwGain)
        {
            Magic.Log("SetGain called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)2);
            fmt.Serialize(ms, dwID);
            fmt.Serialize(ms, dwGain);
            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);
        }

        public void SendForceFeedbackCommand(uint dwID, uint dwCommand)
        {
            Magic.Log("SendForceFeedbackCommand called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)3);
            fmt.Serialize(ms, dwID);
            fmt.Serialize(ms, dwCommand);
            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);
        }

        public void GetForceFeedbackState(uint dwID, IntPtr pds)
        {
            // Does nothing in driver
        }

        public unsafe void DownloadEffect(uint dwID, uint dwEffectID, ref uint pdwEffect, ref DIEFFECT peff, uint dwFlags)
        {
            Magic.Log("DownloadEffect called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)4);
            fmt.Serialize(ms, dwID);
            fmt.Serialize(ms, dwEffectID);
            fmt.Serialize(ms, pdwEffect);
            fmt.Serialize(ms, dwFlags);
            fmt.Serialize(ms, peff.dwSize);
            fmt.Serialize(ms, peff.dwFlags);
            fmt.Serialize(ms, peff.dwDuration);
            fmt.Serialize(ms, peff.dwSamplePeriod);
            fmt.Serialize(ms, peff.dwGain);
            fmt.Serialize(ms, peff.dwTriggerButton);
            fmt.Serialize(ms, peff.dwTriggerRepeatInterval);
            fmt.Serialize(ms, peff.cAxes);
            fmt.Serialize(ms, peff.cbTypeSpecificParams);
            fmt.Serialize(ms, peff.dwStartDelay);

            uint* axes = (uint*)peff.rgdwAxes.ToPointer();
            int* directions = (int*)peff.rglDirection.ToPointer();

            for (int i = 0; i < peff.cAxes; i++)
            {
                fmt.Serialize(ms, axes[i]);
                fmt.Serialize(ms, directions[i]);
            }

            if (peff.lpEnvelope != IntPtr.Zero)
            {
                DIENVELOPE* envelope = (DIENVELOPE*)peff.lpEnvelope.ToPointer();
                fmt.Serialize(ms, true);
                fmt.Serialize(ms, envelope->dwSize);
                fmt.Serialize(ms, envelope->dwAttackLevel);
                fmt.Serialize(ms, envelope->dwAttackTime);
                fmt.Serialize(ms, envelope->dwFadeLevel);
                fmt.Serialize(ms, envelope->dwFadeTime);
            }
            else
            {
                fmt.Serialize(ms, false);
            }

            if (peff.lpvTypeSpecificParams != IntPtr.Zero)
            {
                fmt.Serialize(ms, true);
                var b = new byte[peff.cbTypeSpecificParams];
                Marshal.Copy(peff.lpvTypeSpecificParams, b, 0, b.Length);
                ms.Write(b, 0, b.Length);
            }
            else
            {
                fmt.Serialize(ms, false);
            }

            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);

            var retval = new byte[4];
            _client.Client.Receive(retval);
            pdwEffect = BitConverter.ToUInt32(retval, 0);
        }

        public void DestroyEffect(uint dwID, uint dwEffect)
        {
            Magic.Log("DestroyEffect called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)5);
            fmt.Serialize(ms, dwID);
            fmt.Serialize(ms, dwEffect);
            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);
        }

        public void StartEffect(uint dwID, uint dwEffect, uint dwMode, uint dwCount)
        {
            Magic.Log("StartEffect called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)6);
            fmt.Serialize(ms, dwID);
            fmt.Serialize(ms, dwEffect);
            fmt.Serialize(ms, dwMode);
            fmt.Serialize(ms, dwCount);
            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);
        }

        public void StopEffect(uint dwID, uint dwEffect)
        {
            Magic.Log("StopEffect called!\r\n");
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();
            fmt.Serialize(ms, (uint)7);
            fmt.Serialize(ms, dwID);
            fmt.Serialize(ms, dwEffect);
            var data = ms.ToArray();
            ms.Close();

            _client.Client.WriteInt(data.Length);
            _client.Client.Send(data);
        }

        public void GetEffectStatus(uint dwID, uint dwEffect, ref uint pdwStatus)
        {
            Magic.Log("GetEffectStatus called!\r\n");
            throw new NotImplementedException();
        }

    }
}
