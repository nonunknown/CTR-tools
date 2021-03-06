﻿using CTRFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ctrviewer
{
    class MGLevel
    {
        public static Color ToD = new Color(1f, 1f, 1f);

        public TriList normal = new TriList();
        public TriList wire = new TriList();

        Dictionary<string, QuadList> normalq = new Dictionary<string, QuadList>();
        Dictionary<string, QuadList> animatedq = new Dictionary<string, QuadList>();
        QuadList wireq = new QuadList();

        Dictionary<string, QuadList> flagq = new Dictionary<string, QuadList>();

        public List<string> textureList
        {
            get
            {
                List<string> list = new List<string>();

                foreach (var n in normalq)
                    if (!list.Contains(n.Key))
                        list.Add(n.Key);

                foreach (var n in animatedq)
                    if (!list.Contains(n.Key))
                        list.Add(n.Key);

                return list;
            }
        }

        public MGLevel(SkyBox sb)
        {
            normal.textureEnabled = false;
            normal.scrollingEnabled = false;

            for (int i = 0; i < sb.faces.Count; i++)
            {
                List<VertexPositionColorTexture> tri = new List<VertexPositionColorTexture>();
                tri.Add(MGConverter.ToVptc(sb.verts[sb.faces[i].X], new CTRFramework.Shared.Vector2b(0, 0)));
                tri.Add(MGConverter.ToVptc(sb.verts[sb.faces[i].Y], new CTRFramework.Shared.Vector2b(0, 0)));
                tri.Add(MGConverter.ToVptc(sb.verts[sb.faces[i].Z], new CTRFramework.Shared.Vector2b(0, 0)));

                normal.PushTri(tri);
            }

            normal.Seal();

            wire = new TriList(normal);
            wire.SetColor(Color.Red);
        }



        public MGLevel(Scene s, Detail detail)
        {
            List<VertexPositionColorTexture> monolist = new List<VertexPositionColorTexture>();
            List<CTRFramework.Vertex> vts = new List<Vertex>();

            switch (detail)
            {
                case Detail.Low:
                    {
                        Console.WriteLine("doin low");

                        foreach (QuadBlock qb in s.quads)
                        {
                            monolist.Clear();
                            vts = qb.GetVertexListq(s.verts, -1);

                            if (vts != null)
                            {
                                foreach (Vertex cv in vts)
                                    monolist.Add(MGConverter.ToVptc(cv, cv.uv));

                                TextureLayout t = qb.texlow;

                                string texTag = t.Tag();

                                foreach (QuadFlags fl in (QuadFlags[])Enum.GetValues(typeof(QuadFlags)))
                                {
                                    if (qb.quadFlags.HasFlag(fl))
                                        Push(flagq, fl.ToString(), monolist, "flag");
                                }

                                if (qb.quadFlags.HasFlag(QuadFlags.InvisibleTriggers) || qb.quadFlags.HasFlag(QuadFlags.Invisible))
                                {
                                    Push(flagq, "invis", monolist);
                                    continue;
                                }

                                Push(normalq, texTag, monolist);
                            }
                        }

                        break;
                    }

                case Detail.Med:
                    {
                        Console.WriteLine("doin hi");

                        foreach (QuadBlock qb in s.quads)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                monolist.Clear();
                                vts = qb.GetVertexListq(s.verts, j);

                                if (vts != null)
                                {
                                    foreach (Vertex cv in vts)
                                        monolist.Add(MGConverter.ToVptc(cv, cv.uv));

                                    bool isAnimated = false;
                                    string texTag = "test";

                                    if (qb.ptrTexMid[j] != 0)
                                    {
                                        isAnimated = qb.tex[j].isAnimated;
                                        if (texTag != "00000000")
                                            texTag = qb.tex[j].midlods[2].Tag();
                                    }

                                    foreach (QuadFlags fl in (QuadFlags[])Enum.GetValues(typeof(QuadFlags)))
                                    {
                                        if (qb.quadFlags.HasFlag(fl))
                                            Push(flagq, fl.ToString(), monolist, "flag");
                                    }

                                    if (qb.quadFlags.HasFlag(QuadFlags.InvisibleTriggers) || qb.quadFlags.HasFlag(QuadFlags.Invisible))
                                    {
                                        Push(flagq, "invis", monolist);
                                        continue;
                                    }

                                    Push((isAnimated ? animatedq : normalq), texTag, monolist);

                                    if (isAnimated)
                                        animatedq[texTag].scrollingEnabled = true;
                                }
                            }
                        }

                        break;
                    }
            }

            foreach (var ql in normalq)
                ql.Value.Seal();

            foreach (var ql in animatedq)
                ql.Value.Seal();

            foreach (var ql in flagq)
                ql.Value.Seal();
        }


        public void Push(Dictionary<string, QuadList> dict, string name, List<VertexPositionColorTexture> monolist, string custTex = "")
        {
            if (dict.ContainsKey(name))
            {
                dict[name].PushQuad(monolist);
            }
            else
            {
                QuadList ql = new QuadList(monolist, true, (custTex != "" ? custTex : name));
                dict.Add(name, ql);
            }
        }


        public void Update(GameTime gameTime)
        {
            foreach (var ql in animatedq)
                ql.Value.Update(gameTime);
        }

        public void Render(GraphicsDeviceManager graphics, BasicEffect effect)
        {
            Samplers.SetToDevice(graphics, EngineSampler.Default);

            foreach (var ql in normalq)
                ql.Value.Render(graphics, effect);

            Samplers.SetToDevice(graphics, EngineSampler.Animated);

            foreach (var ql in animatedq)
                ql.Value.Render(graphics, effect);

            Samplers.SetToDevice(graphics, EngineSampler.Default);

            if (flagq.ContainsKey(((QuadFlags)(1 << Game1.currentflag)).ToString()))
                flagq[((QuadFlags)(1 << Game1.currentflag)).ToString()].Render(graphics, effect);

            if (!Game1.HideInvisible)
            {
                effect.Alpha = 0.25f;

                if (flagq.ContainsKey("invis"))
                    flagq["invis"].Render(graphics, effect);

                effect.Alpha = 1f;
            }
        }

        public void RenderSky(GraphicsDeviceManager graphics, BasicEffect effect)
        {
            normal.Render(graphics, effect);
        }
    }
}