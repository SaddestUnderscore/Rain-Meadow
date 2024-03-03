﻿using UnityEngine;
using RWCustom;
using System;

namespace RainMeadow
{
    public class EmoteControllerInput : HUD.HudPart
    {
        static EmoteType[][] radialMappingPages = new EmoteType[][]{
            new[]{
                EmoteType.emoteHello,
                EmoteType.emoteHappy,
                EmoteType.emoteSad,
                EmoteType.emoteConfused,
                EmoteType.emoteGoofy,
                EmoteType.emoteDead,
                EmoteType.emoteAmazed,
                EmoteType.emoteShrug,
            },new[]{
                EmoteType.emoteHug,
                EmoteType.emoteAngry,
                EmoteType.emoteWink,
                EmoteType.emoteMischievous,
                EmoteType.none,
                EmoteType.none,
                EmoteType.none,
                EmoteType.none,
            },new[]{
                EmoteType.symbolYes,
                EmoteType.symbolNo,
                EmoteType.symbolQuestion,
                EmoteType.symbolTime,
                EmoteType.symbolSurvivor,
                EmoteType.symbolFriends,
                EmoteType.symbolGroup,
                EmoteType.symbolKnoledge,
            },new[]{
                EmoteType.symbolTravel,
                EmoteType.symbolMartyr,
                EmoteType.symbolNo,
                EmoteType.symbolNo,
                EmoteType.symbolCollectible,
                EmoteType.symbolFood,
                EmoteType.symbolLight,
                EmoteType.symbolShelter,
            },new[]{
                EmoteType.symbolGate,
                EmoteType.symbolEcho,
                EmoteType.symbolPointOfInterest,
                EmoteType.symbolTree,
                EmoteType.symbolIterator,
                EmoteType.symbolNo,
                EmoteType.symbolNo,
                EmoteType.symbolNo,
            }
        };
        static int npages = radialMappingPages.Length;

        EmoteRadialPage[] pages;
        private MeadowAvatarCustomization customization;
        private EmoteHandler emoteHandler;
        private FContainer container;

        private FLabel cancelLabel;
        private FSprite knobSprite;
        private Vector2 lastKnobPos;
        private Vector2 knobPos;
        private Vector2 knobVel;
        public int selected;
        private int lastSelected;

        public EmoteControllerInput(HUD.HUD hud, MeadowAvatarCustomization customization, EmoteHandler emoteHandler) : base(hud)
        {
            this.customization = customization;
            this.emoteHandler = emoteHandler;

            container = new FContainer();
            var forthwidth = hud.rainWorld.screenSize.x / 4f;
            var halfheight = hud.rainWorld.screenSize.y / 2f;
            this.pages = new[] {
                new EmoteRadialPage(hud, this.container, customization, radialMappingPages[0], Vector2.zero, true),
                new EmoteRadialPage(hud, this.container, customization, radialMappingPages[npages-1], new Vector2(-forthwidth, 0), false),
                new EmoteRadialPage(hud, this.container, customization, radialMappingPages[1], new Vector2(+forthwidth, 0), false)
            };

            this.knobSprite = new FSprite("Circle20", true);
            knobSprite.alpha = 0.4f;
            this.container.AddChild(this.knobSprite);

            this.cancelLabel = new FLabel(Custom.GetFont(), "CANCEL");
            container.AddChild(cancelLabel);

            hud.fContainers[1].AddChild(this.container);
            this.container.SetPosition(new Vector2(hud.rainWorld.screenSize.x / 2f, halfheight));
        }

        public override void Update()
        {
            this.lastKnobPos = this.knobPos;
            this.knobPos += this.knobVel;
            this.knobVel *= 0.5f;
            var control = RWCustom.Custom.rainWorld.options.controls[0];
            if (control.gamePad)
            {
                var analogDir = new Vector2(Input.GetAxisRaw("Joy1Axis4"), -Input.GetAxisRaw("Joy1Axis5")); // yes it's hardcoded, no I can't get rewired to work
                this.knobVel += (analogDir - this.knobPos) / 8f;
                this.knobPos += (analogDir - this.knobPos) / 4f;
            }
            else
            {
                var package = RWInput.PlayerInput(0, RWCustom.Custom.rainWorld);
                this.knobVel -= this.knobPos / 6f;
                this.knobVel.x += (float)package.x * 0.3f;
                this.knobVel.y += (float)package.y * 0.3f;
                this.knobPos.x += (float)package.x * 0.3f;
                this.knobPos.y += (float)package.y * 0.3f;
            }
            if (this.knobPos.magnitude > 1f)
            {
                Vector2 b = Vector2.ClampMagnitude(this.knobPos, 1f) - this.knobPos;
                this.knobPos += b;
                this.knobVel += b;
            }

            this.lastSelected = selected;
            if (knobPos.magnitude > 0.5f)
            {
                this.selected = Mathf.FloorToInt((-Custom.Angle(Vector2.left, knobPos) + 382.5f) / 45f) % 8;
            }
            else
            {
                selected = -1;
            }
        }

        public override void Draw(float timeStacker)
        {
            var outterRadius = 1.975f * 80f;
            this.knobSprite.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            Vector2 vector2 = Vector2.Lerp(this.lastKnobPos, this.knobPos, timeStacker);
            this.knobSprite.x = vector2.x * (outterRadius - 18f) + 0.01f;
            this.knobSprite.y = vector2.y * (outterRadius - 18f) + 0.01f;
            if (selected != lastSelected)
            {
                pages[0].SetSelected(selected);
            }
        }


        public override void ClearSprites()
        {
            base.ClearSprites();
            container.RemoveAllChildren();
            container.RemoveFromContainer();
        }

        public class EmoteRadialPage
        {
            private EmoteType[] emotes;
            private MeadowAvatarCustomization customization;
            private TriangleMesh[] meshes;
            private FSprite[] icons;
            private TriangleMesh centerMesh;
            

            public Color colorUnselected = new Color(0f, 0f, 0f, 0.2f);
            public Color colorSelected = new Color(1f, 1f, 1f, 0.2f);

            // relative to emote size
            const float innerRadiusFactor = 0.888f;
            const float outterRadiusFactor = 1.975f;
            const float emoteRadiusFactor = 1.32f;
            float innerRadius;
            float outterRadius;
            float emoteRadius;


            // maybe these are visual-only parts and there's a containing class that handles the input logic?
            // or if(ismain) all the way down?
            public EmoteRadialPage(HUD.HUD hud, FContainer container, MeadowAvatarCustomization customization, EmoteType[] emotes, Vector2 pos, bool big)
            {
                this.customization = customization;
                this.meshes = new TriangleMesh[8];
                this.icons = new FSprite[8];
                this.centerMesh = new TriangleMesh("Futile_White", new TriangleMesh.Triangle[] { new(0, 1, 2), new(0, 2, 3), new(0, 3, 4), new(0, 4, 5), new(0, 5, 6), new(0, 6, 7), new(0, 7, 8), new(0, 8, 1), }, false);
                container.AddChild(this.centerMesh);

                var emotesSize = big ? 80f : 40f;
                this.innerRadius = innerRadiusFactor * emotesSize;
                this.outterRadius = outterRadiusFactor * emotesSize;
                this.emoteRadius = emoteRadiusFactor * emotesSize;

                centerMesh.color = colorUnselected;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dira = RWCustom.Custom.RotateAroundOrigo(Vector2.left, (-1f + 2 * i) * (360f / 16f));
                    Vector2 dirb = RWCustom.Custom.RotateAroundOrigo(Vector2.left, (1f + 2 * i) * (360f / 16f));
                    this.meshes[i] = new TriangleMesh("Futile_White", new TriangleMesh.Triangle[] { new(0, 1, 2), new(2, 3, 0) }, false);

                    meshes[i].vertices[0] = pos + dira * innerRadius;
                    meshes[i].vertices[1] = pos + dira * outterRadius;
                    meshes[i].vertices[2] = pos + dirb * outterRadius;
                    meshes[i].vertices[3] = pos + dirb * innerRadius;

                    meshes[i].color = colorUnselected;

                    container.AddChild(meshes[i]);

                    icons[i] = new FSprite("Futile_White");
                    icons[i].scale = emotesSize / EmoteDisplayer.emoteSourceSize;
                    icons[i].alpha = 0.6f;
                    icons[i].SetPosition(pos + RWCustom.Custom.RotateAroundOrigo(Vector2.left * emoteRadius, (i) * (360f / 8f)));
                    container.AddChild(icons[i]);

                    centerMesh.vertices[i + 1] = pos + dira * innerRadius;
                }
                centerMesh.vertices[0] = pos;

                SetEmotes(emotes);
            }

            public void SetEmotes(EmoteType[] emotes)
            {
                this.emotes = emotes;
                for (int i = 0; i < icons.Length; i++)
                {
                    if (emotes[i] != EmoteType.none)
                    {
                        icons[i].SetElementByName(customization.GetEmote(emotes[i]));
                        icons[i].alpha = 0.6f;
                    }
                    else
                    {
                        icons[i].alpha = 0f;
                    }
                }
            }

            internal void SetSelected(int selected)
            {
                centerMesh.color = colorUnselected;
                for (int i = 0; i < 8; i++)
                {
                    meshes[i].color = colorUnselected;
                }
                if (selected > -1) meshes[selected].color = colorSelected; else centerMesh.color = colorSelected;
            }
        }
    }
}
