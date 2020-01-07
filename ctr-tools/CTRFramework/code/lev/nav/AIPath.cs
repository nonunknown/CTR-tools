﻿using CTRFramework.Shared;
using System.Collections.Generic;
using System.Text;

namespace CTRFramework
{
    public class AIPath : IRead, IWrite
    { 
        public ushort unk1;
        public ushort numFrames;
        public byte[] data;
        public NavFrame start;

        public List<NavFrame> frames = new List<NavFrame>();

        public AIPath()
        {

        }

        public AIPath(BinaryReaderEx br)
        {
            Read(br);
        }
        public void Read(BinaryReaderEx br)
        {
            unk1 = br.ReadUInt16();
            numFrames = br.ReadUInt16();
            data = br.ReadBytes(4*18); //0x4c = total header size

            start = new NavFrame(br);

            for (int i = 0; i < numFrames; i++)
            {
                frames.Add(new NavFrame(br));
            }
        }

        public string ToObj()
        {
            StringBuilder sb = new StringBuilder();

            foreach (NavFrame f in frames)
                sb.AppendFormat("v {0}\r\n", f.position.ToString());

            return sb.ToString();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("frames = " + numFrames);

            sb.AppendLine("start: " + start.ToString());

            foreach (NavFrame nv in frames)
            {
                sb.AppendLine(nv.ToString());
            }

            return sb.ToString();
        }


        public void Write(BinaryWriterEx bw)
        {
            bw.Write(unk1);
            bw.Write(numFrames);
            bw.Write(data);
            start.Write(bw);

            foreach (NavFrame f in frames)
            {
                f.Write(bw);
            }
        }
    }
}
