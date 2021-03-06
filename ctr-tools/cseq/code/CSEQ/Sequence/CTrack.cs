﻿using cseq.Helpers;
using CTRFramework.Shared;
using NAudio.Midi;
using System.Collections.Generic;
using System.Text;

namespace cseq
{
    public class CTrack
    {
        //meta
        public string name;
        public string address = "";
        public int trackNum;

        public int instrument = 0;
        public bool isDrumTrack = false;

        List<Command> cmd;

        public CTrack()
        {
            cmd = new List<Command>();
        }


        public void Import(string s)
        {

        }

        public void Read(BinaryReaderEx br, int num)
        {
            trackNum = num;
            address = br.HexPos();

            switch (br.ReadInt16())
            {
                case 0: isDrumTrack = false; break;
                case 1: isDrumTrack = true; break;
                default: Log.WriteLine("drum value not boolean at " + br.HexPos()); break;
            }

            Command cx;

            do
            {
                cx = new Command();
                cx.Read(br);

                if (cx.evt == CSEQEvent.ChangePatch)
                    instrument = cx.pitch;

                cmd.Add(cx);
            }
            while (cx.evt != CSEQEvent.EndTrack);

            name = "Track_" + trackNum.ToString("00") + (isDrumTrack ? "_drum" : "");
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(address + "\r\n");

            foreach (Command c in cmd)
                sb.Append(c.ToString());

            return sb.ToString();
        }


        public List<MidiEvent> ToMidiEventList(CSeqHeader header, int channel, CSEQ seq)
        {
            List<MidiEvent> me = new List<MidiEvent>();
            //MidiEvent x;

            int absTime = 0;

            me.Add(new TextEvent(name, MetaEventType.SequenceTrackName, absTime));
            me.Add(new TempoEvent(header.MPQN, absTime));

            if (channel == 10)
            {
                me.Add(new ControlChangeEvent(absTime, channel, MidiController.BankSelect, 120));
                me.Add(new ControlChangeEvent(absTime, channel, MidiController.BankSelect, 0));
                me.Add(new PatchChangeEvent(absTime, channel, CTRJson.GetBankIndex(CSEQ.PatchName)));
            }

            if (CSEQ.UseSampleVolumeForTracks && !CSEQ.IgnoreVolume)
                me.Add(new ControlChangeEvent(absTime, channel, MidiController.MainVolume, seq.samplesReverb[instrument].Volume / 2));

            foreach (Command c in cmd)
            {
                me.AddRange(c.ToMidiEvent(absTime, channel, seq, this));
                absTime += c.wait;
            }

            return me;
        }


        public void WriteBytes(BinaryWriterEx bw)
        {
            bw.Write(isDrumTrack ? (short)1 : (short)0);

            foreach (Command c in cmd)
            {
                c.WriteBytes(bw);
            }
        }
    }
}
