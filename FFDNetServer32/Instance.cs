using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace FFDNetServer32
{
    class Instance
    {
        public Instance(TcpClient client)
        {
            _client = client;

            var joytech = new JoyTechFFDriverClass();
            _driver = (IDirectInputEffectDriver)joytech;
        }

        private byte[] _buffer = new byte[4096];
        private int _bufferPos = 0;
        private readonly TcpClient _client;
        public Socket Socket { get { return _client.Client; } }
        private IDirectInputEffectDriver _driver = null;


        internal bool HandleRead()
        {
            int count;
            try
            {
                count = Socket.Receive(_buffer, _bufferPos, _buffer.Length - _bufferPos, SocketFlags.None);
            }
            catch (SocketException)
            {
                return false;
            }
            Console.WriteLine("read {0} bytes", count);
            _bufferPos += count;

            while (_bufferPos >= 4)
            {
                int size = _buffer[0] | (_buffer[1] << 8) | (_buffer[2] << 16) | (_buffer[3] << 24);
                Console.WriteLine("claimed size of buffer is {0}", size);
                if (_bufferPos < (4 + size))
                    break;

                Console.WriteLine("we have a Thing, folks");
                using (var ms = new MemoryStream(_buffer, 4, size))
                {
                    var fmt = new BinaryFormatter();
                    var cmd = (uint)fmt.Deserialize(ms);

                    switch (cmd)
                    {
                        case 1: HandleDeviceID(fmt, ms); break;
                        case 2: HandleSetGain(fmt, ms); break;
                        case 3: HandleSendForceFeedbackCommand(fmt, ms); break;
                        case 4: HandleDownloadEffect(fmt, ms); break;
                        case 5: HandleDestroyEffect(fmt, ms); break;
                        case 6: HandleStartEffect(fmt, ms); break;
                        case 7: HandleStopEffect(fmt, ms); break;
                    }
                }

                Array.Copy(_buffer, 4 + size, _buffer, 0, _buffer.Length - 4 - size);
                _bufferPos -= (4 + size);
            }

            return (count > 0);
        }


        public void Bye()
        {
            _client.Close();
            _driver = null;
        }


        private void HandleDeviceID(IFormatter fmt, Stream stm)
        {
            uint dwDIVer = (uint)fmt.Deserialize(stm);
            uint dwExternalID = (uint)fmt.Deserialize(stm);
            uint fBegin = (uint)fmt.Deserialize(stm);
            uint dwInternalId = (uint)fmt.Deserialize(stm);

            var info = new DIHIDFFINITINFO();
            info.dwSize = (uint)fmt.Deserialize(stm);
            info.pwszDeviceInterface = (string)fmt.Deserialize(stm);
            info.GuidInstance = (Guid)fmt.Deserialize(stm);

            Console.WriteLine("deviceid fuck Yeah");

            _driver.DeviceID(dwDIVer, dwExternalID, fBegin, dwInternalId, ref info);
        }

        private void HandleSetGain(IFormatter fmt, Stream stm)
        {
            uint dwID = (uint)fmt.Deserialize(stm);
            uint dwGain = (uint)fmt.Deserialize(stm);

            Console.WriteLine("SetGain({0}, {1})", dwID, dwGain);

            _driver.SetGain(dwID, dwGain);
        }

        private void HandleSendForceFeedbackCommand(IFormatter fmt, Stream stm)
        {
            uint dwID = (uint)fmt.Deserialize(stm);
            uint dwCommand = (uint)fmt.Deserialize(stm);

            Console.WriteLine("SendForceFeedbackCommand({0}, {1})", dwID, dwCommand);

            _driver.SendForceFeedbackCommand(dwID, dwCommand);
        }

        private unsafe void HandleDownloadEffect(IFormatter fmt, Stream stm)
        {
            uint dwID = (uint)fmt.Deserialize(stm);
            uint dwEffectID = (uint)fmt.Deserialize(stm);
            uint pdwEffect = (uint)fmt.Deserialize(stm);
            uint dwFlags = (uint)fmt.Deserialize(stm);

            DIEFFECT peff = new DIEFFECT();
            peff.dwSize = (uint)fmt.Deserialize(stm);
            peff.dwFlags = (uint)fmt.Deserialize(stm);
            peff.dwDuration = (uint)fmt.Deserialize(stm);
            peff.dwSamplePeriod = (uint)fmt.Deserialize(stm);
            peff.dwGain = (uint)fmt.Deserialize(stm);
            peff.dwTriggerButton = (uint)fmt.Deserialize(stm);
            peff.dwTriggerRepeatInterval = (uint)fmt.Deserialize(stm);
            peff.cAxes = (uint)fmt.Deserialize(stm);
            peff.cbTypeSpecificParams = (uint)fmt.Deserialize(stm);
            peff.dwStartDelay = (uint)fmt.Deserialize(stm);

            var axes = new uint[peff.cAxes];
            var directions = new int[peff.cAxes];

            Console.WriteLine("downloading effect");

            fixed (uint* pAxes = &axes[0])
            {
                fixed (int* pDirections = &directions[0])
                {
                    peff.rgdwAxes = new IntPtr(pAxes);
                    peff.rglDirection = new IntPtr(pDirections);

                    for (int i = 0; i < peff.cAxes; i++)
                    {
                        axes[i] = (uint)fmt.Deserialize(stm);
                        directions[i] = (int)fmt.Deserialize(stm);
                    }

                    var hasEnvelope = (bool)fmt.Deserialize(stm);
                    var envelope = new DIENVELOPE();
                    if (hasEnvelope)
                    {
                        envelope.dwSize = (uint)fmt.Deserialize(stm);
                        envelope.dwAttackLevel = (uint)fmt.Deserialize(stm);
                        envelope.dwAttackTime = (uint)fmt.Deserialize(stm);
                        envelope.dwFadeLevel = (uint)fmt.Deserialize(stm);
                        envelope.dwFadeTime = (uint)fmt.Deserialize(stm);
                    }

                    var hasTSP = (bool)fmt.Deserialize(stm);
                    var tsp = new byte[hasTSP ? peff.cbTypeSpecificParams : 0];
                    if (hasTSP)
                    {
                        stm.Read(tsp, 0, tsp.Length);
                    }

                    fixed (byte* pTsp = &tsp[0])
                    {
                        if (hasEnvelope)
                            peff.lpEnvelope = new IntPtr(&envelope);
                        if (hasTSP)
                            peff.lpvTypeSpecificParams = new IntPtr(pTsp);
                        Console.WriteLine("here we go");
                        _driver.DownloadEffect(dwID, dwEffectID, ref pdwEffect, ref peff, dwFlags);
                        Console.WriteLine("done!");
                    }
                }
            }

            _client.Client.Send(BitConverter.GetBytes(pdwEffect));
        }

        private void HandleDestroyEffect(IFormatter fmt, Stream stm)
        {
            uint dwID = (uint)fmt.Deserialize(stm);
            uint dwEffect = (uint)fmt.Deserialize(stm);

            Console.WriteLine("DestroyEffect({0}, {1})", dwID, dwEffect);

            _driver.DestroyEffect(dwID, dwEffect);
        }

        private void HandleStartEffect(IFormatter fmt, Stream stm)
        {
            uint dwID = (uint)fmt.Deserialize(stm);
            uint dwEffect = (uint)fmt.Deserialize(stm);
            uint dwMode = (uint)fmt.Deserialize(stm);
            uint dwCount = (uint)fmt.Deserialize(stm);

            Console.WriteLine("StartEffect({0}, {1}, {2}, {3})", dwID, dwEffect, dwMode, dwCount);

            _driver.StartEffect(dwID, dwEffect, dwMode, dwCount);
        }

        private void HandleStopEffect(IFormatter fmt, Stream stm)
        {
            uint dwID = (uint)fmt.Deserialize(stm);
            uint dwEffect = (uint)fmt.Deserialize(stm);

            Console.WriteLine("StopEffect({0}, {1})", dwID, dwEffect);

            _driver.StopEffect(dwID, dwEffect);
        }

    }
}
