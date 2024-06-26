﻿using HarmonyLib;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    // Progression is the content unlock system
    // Characters, skins, emotes are listed here
    // Saving loading what's unlocked is handled here
    public static class MeadowProgression
    {
        internal static void InitializeBuiltinTypes()
        {
            try
            {
                _ = Character.Slugcat;
                _ = Skin.Slugcat_Survivor;
                currentTestSkin = Skin.Eggbug_Blue;

                RainMeadow.Debug($"characters loaded: {Character.values.Count}");
                RainMeadow.Debug($"skins loaded: {Skin.values.Count}");
            }
            catch (Exception e)
            {
                RainMeadow.Error(e);
                throw;
            }
        }

        public static Dictionary<Character, CharacterData> characterData = new();

        public class CharacterData
        {
            public string displayName;
            public string emotePrefix;
            public string emoteAtlas;
            public Color emoteColor;
            public List<Skin> skins = new();
        }

        public class Character : ExtEnum<Character>
        {
            public Character(string value, bool register = false, CharacterData characterDataEntry = null) : base(value, register)
            {
                if (register)
                {
                    characterData[this] = characterDataEntry;
                }
            }

            public static Character Slugcat = new("Slugcat", true, new()
            {
                displayName = "SLUGCAT",
                emotePrefix = "sc_",
                emoteAtlas = "emotes_slugcat",
                emoteColor = new Color(85f, 120f, 120f, 255f) / 255f,
            });
            public static Character Cicada = new("Cicada", true, new()
            {
                displayName = "CICADA",
                emotePrefix = "squid_",
                emoteAtlas = "emotes_squid",
                emoteColor = new Color(81f, 81f, 81f, 255f) / 255f,
            });
            public static Character Lizard = new("Lizard", true, new()
            {
                displayName = "LIZARD",
                emotePrefix = "liz_",
                emoteAtlas = "emotes_lizard",
                emoteColor = new Color(197, 220, 232, 255f) / 255f,
            });
            public static Character Scavenger = new("Scavenger", true, new()
            {
                displayName = "SCAVENGER",
                emotePrefix = "sc_", // "scav_"
                emoteAtlas = "emotes_slugcat",//"emotes_scav",
                emoteColor = new Color(232, 187, 200, 255f) / 255f,
            });
            public static Character Noodlefly = new("Noodlefly", true, new()
            {
                displayName = "NOODLEFLY",
                emotePrefix = "sc_", // "noot_"
                emoteAtlas = "emotes_slugcat",//"emotes_noot",
                emoteColor = new Color(232, 187, 200, 255f) / 255f, // todo
            });
            public static Character Eggbug = new("Eggbug", true, new()
            {
                displayName = "EGGBUG",
                emotePrefix = "sc_", // "noot_"
                emoteAtlas = "emotes_slugcat",//"emotes_noot",
                emoteColor = new Color(232, 187, 200, 255f) / 255f, // todo
            });
        }

        public static Dictionary<Skin, SkinData> skinData = new();
        internal static Skin currentTestSkin;

        public class SkinData
        {
            public Character character;
            public string displayName;
            public CreatureTemplate.Type creatureType;
            public SlugcatStats.Name statsName; // curently only used for color
            public int randomSeed;
            public Color? baseColor;
            public Color? eyeColor;
            public Color? effectColor;
            public float tintFactor = 0.3f;
            public string emoteAtlasOverride;
            public string emotePrefixOverride;
            public Color? emoteColorOverride;
        }

        public class Skin : ExtEnum<Skin>
        {
            public Skin(string value, bool register = false, SkinData skinDataEntry = null) : base(value, register)
            {
                if (register)
                {
                    skinData[this] = skinDataEntry;
                    characterData[skinDataEntry.character].skins.Add(this);
                }
            }

            public static Skin Slugcat_Survivor = new("Slugcat_Survivor", true, new()
            {
                character = Character.Slugcat,
                displayName = "Survivor",
                creatureType = CreatureTemplate.Type.Slugcat,
                statsName = SlugcatStats.Name.White,
            });
            public static Skin Slugcat_Monk = new("Slugcat_Monk", true, new()
            {
                character = Character.Slugcat,
                displayName = "Monk",
                creatureType = CreatureTemplate.Type.Slugcat,
                statsName = SlugcatStats.Name.Yellow,
            });
            public static Skin Slugcat_Hunter = new("Slugcat_Hunter", true, new()
            {
                character = Character.Slugcat,
                displayName = "Hunter",
                creatureType = CreatureTemplate.Type.Slugcat,
                statsName = SlugcatStats.Name.Red,
            });
            public static Skin Slugcat_Fluffy = new("Slugcat_Fluffy", true, new()
            {
                character = Character.Slugcat,
                displayName = "Fluffy",
                creatureType = CreatureTemplate.Type.Slugcat,
                statsName = SlugcatStats.Name.White,
                baseColor = new Color(111, 216, 255, 255) / 255f
            });

            public static Skin Cicada_White = new("Cicada_White", true, new()
            {
                character = Character.Cicada,
                displayName = "White",
                creatureType = CreatureTemplate.Type.CicadaA,
            });
            public static Skin Cicada_Dark = new("Cicada_Dark", true, new()
            {
                character = Character.Cicada,
                displayName = "Dark",
                creatureType = CreatureTemplate.Type.CicadaB,
            });

            public static Skin Lizard_Pink = new("Lizard_Pink", true, new()
            {
                character = Character.Lizard,
                displayName = "Pink",
                creatureType = CreatureTemplate.Type.PinkLizard,
                tintFactor = 0.5f,
            });
            public static Skin Lizard_Blue = new("Lizard_Blue", true, new()
            {
                character = Character.Lizard,
                displayName = "Blue",
                creatureType = CreatureTemplate.Type.BlueLizard,
                tintFactor = 0.5f,
            });
            public static Skin Lizard_Yellow = new("Lizard_Yellow", true, new()
            {
                character = Character.Lizard,
                displayName = "Yellow",
                creatureType = CreatureTemplate.Type.YellowLizard,
                randomSeed = 1366,
                tintFactor = 0.5f,
            });
            public static Skin Lizard_Cyan = new("Lizard_Cyan", true, new()
            {
                character = Character.Lizard,
                displayName = "Cyan",
                creatureType = CreatureTemplate.Type.CyanLizard,
                randomSeed = 1366,
                tintFactor = 0.5f,
            });

            public static Skin Scavenger_Twigs = new("Scavenger_Twigs", true, new()
            {
                character = Character.Scavenger,
                displayName = "Twigs",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 4481,
            });
            public static Skin Scavenger_Acorn = new("Scavenger_Acorn", true, new()
            {
                character = Character.Scavenger,
                displayName = "Acorn",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 1213,
            });
            public static Skin Scavenger_Oak = new("Scavenger_Oak", true, new()
            {
                character = Character.Scavenger,
                displayName = "Oak",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 9503,
            });
            public static Skin Scavenger_Shrub = new("Scavenger_Shrub", true, new()
            {
                character = Character.Scavenger,
                displayName = "Shrub",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 1139,
            });
            public static Skin Scavenger_Branches = new("Scavenger_Branches", true, new()
            {
                character = Character.Scavenger,
                displayName = "Branches",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 1503,
            });
            public static Skin Scavenger_Sage = new("Scavenger_Sage", true, new()
            {
                character = Character.Scavenger,
                displayName = "Sage",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 1184,
            });
            public static Skin Scavenger_Cherry = new("Scavenger_Cherry", true, new()
            {
                character = Character.Scavenger,
                displayName = "Cherry",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 9464,
            });
            public static Skin Scavenger_Lavender = new("Scavenger_Lavender", true, new()
            {
                character = Character.Scavenger,
                displayName = "Lavender",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 8201,
            });
            public static Skin Scavenger_Peppermint = new("Scavenger_Peppermint", true, new()
            {
                character = Character.Scavenger,
                displayName = "Peppermint",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 8750,
            });
            public static Skin Scavenger_Juniper = new("Scavenger_Juniper", true, new()
            {
                character = Character.Scavenger,
                displayName = "Juniper",
                creatureType = CreatureTemplate.Type.Scavenger,
                randomSeed = 4566,
            });

            public static Skin Noodlefly_Big = new("Noodlefly_Big", true, new()
            {
                character = Character.Noodlefly,
                displayName = "Big",
                creatureType = CreatureTemplate.Type.BigNeedleWorm,
            });
            public static Skin Noodlefly_Small = new("Noodlefly_Small", true, new()
            {
                character = Character.Noodlefly,
                displayName = "Small",
                creatureType = CreatureTemplate.Type.SmallNeedleWorm,
            });

            public static Skin Eggbug_Blue = new("Eggbug_Blue", true, new()
            {
                character = Character.Eggbug,
                displayName = "Blue",
                creatureType = CreatureTemplate.Type.EggBug,
                randomSeed = 1001,
            });
            public static Skin Eggbug_Teal = new("Eggbug_Teal", true, new()
            {
                character = Character.Eggbug,
                displayName = "Teal",
                creatureType = CreatureTemplate.Type.EggBug,
                randomSeed = 1002,
            });
        }

        public class Emote : ExtEnum<Emote>
        {
            public Emote(string value, bool register = false) : base(value, register) { }
            public static Emote none = new("none", true);

            // emotions
            public static Emote emoteHello = new("emoteHello", true);
            public static Emote emoteHappy = new("emoteHappy", true);
            public static Emote emoteSad = new("emoteSad", true);
            public static Emote emoteConfused = new("emoteConfused", true);
            public static Emote emoteGoofy = new("emoteGoofy", true);
            public static Emote emoteDead = new("emoteDead", true);
            public static Emote emoteAmazed = new("emoteAmazed", true);
            public static Emote emoteShrug = new("emoteShrug", true);
            public static Emote emoteHug = new("emoteHug", true);
            public static Emote emoteAngry = new("emoteAngry", true);
            public static Emote emoteWink = new("emoteWink", true);
            public static Emote emoteMischievous = new("emoteMischievous", true);

            // ideas
            public static Emote symbolYes = new("symbolYes", true);
            public static Emote symbolNo = new("symbolNo", true);
            public static Emote symbolQuestion = new("symbolQuestion", true);
            public static Emote symbolTime = new("symbolTime", true);
            public static Emote symbolSurvivor = new("symbolSurvivor", true);
            public static Emote symbolFriends = new("symbolFriends", true);
            public static Emote symbolGroup = new("symbolGroup", true);
            public static Emote symbolKnoledge = new("symbolKnoledge", true);
            public static Emote symbolTravel = new("symbolTravel", true);
            public static Emote symbolMartyr = new("symbolMartyr", true);

            // things
            public static Emote symbolCollectible = new("symbolCollectible", true);
            public static Emote symbolFood = new("symbolFood", true);
            public static Emote symbolLight = new("symbolLight", true);
            public static Emote symbolShelter = new("symbolShelter", true);
            public static Emote symbolGate = new("symbolGate", true);
            public static Emote symbolEcho = new("symbolEcho", true);
            public static Emote symbolPointOfInterest = new("symbolPointOfInterest", true);
            public static Emote symbolTree = new("symbolTree", true);
            public static Emote symbolIterator = new("symbolIterator", true);

            // verbs
            // todo
        }

        public static List<Skin> AllAvailableSkins(Character character)
        {
            return characterData[character].skins.Intersect(progressionData.characterProgress[character].unlockedSkins).ToList();
        }

        public static List<Character> AllAvailableCharacters()
        {
            return characterData.Keys.Intersect(progressionData.unlockedCharacters).ToList();
        }

        internal static void ItemCollected(AbstractMeadowCollectible abstractMeadowCollectible)
        {
            var meadowHud = (Custom.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0].hud.parts.First(p => p is MeadowHud) as MeadowHud;
            if (abstractMeadowCollectible.type == RainMeadow.Ext_PhysicalObjectType.MeadowTokenGold) // creature unlock
            {
                meadowHud.AnimateChar();
                progressionData.characterUnlockProgress++;
                if (progressionData.characterUnlockProgress >= characterProgressTreshold)
                {
                    if (NextUnlockableCharacter() is Character chararcter)
                    {
                        progressionData.unlockedCharacters.Add(chararcter);
                        progressionData.characterUnlockProgress -= characterProgressTreshold;
                        meadowHud.NewCharacterUnlocked(chararcter);
                    }
                    else
                    {
                        progressionData.characterUnlockProgress = characterProgressTreshold;
                    }
                }
            }
            else if (abstractMeadowCollectible.type == RainMeadow.Ext_PhysicalObjectType.MeadowTokenRed)
            {
                meadowHud.AnimateEmote();
                progressionData.currentCharacterProgress.emoteUnlockProgress++;
                if (progressionData.currentCharacterProgress.emoteUnlockProgress >= emoteProgressTreshold)
                {
                    if (NextUnlockableEmote() is Emote emote)
                    {
                        progressionData.currentCharacterProgress.unlockedEmotes.Add(emote);
                        progressionData.currentCharacterProgress.emoteUnlockProgress -= emoteProgressTreshold;
                        meadowHud.NewEmoteUnlocked(emote);
                    }
                    else
                    {
                        progressionData.currentCharacterProgress.emoteUnlockProgress = emoteProgressTreshold;
                    }
                }
            }
            else if (abstractMeadowCollectible.type == RainMeadow.Ext_PhysicalObjectType.MeadowTokenBlue)
            {
                meadowHud.AnimateSkin();
                progressionData.currentCharacterProgress.skinUnlockProgress++;
                if (progressionData.currentCharacterProgress.skinUnlockProgress >= skinProgressTreshold)
                {
                    if (NextUnlockableSkin() is Skin skin)
                    {
                        progressionData.currentCharacterProgress.unlockedSkins.Add(skin);
                        progressionData.currentCharacterProgress.skinUnlockProgress -= skinProgressTreshold;
                        meadowHud.NewSkinUnlocked(skin);
                    }
                    else
                    {
                        progressionData.currentCharacterProgress.skinUnlockProgress = skinProgressTreshold;
                    }
                }
            }
        }

        private static Emote NextUnlockableEmote()
        {
            return emoteEmotes.Except(progressionData.currentCharacterProgress.unlockedEmotes).FirstOrDefault();
        }

        private static Skin NextUnlockableSkin()
        {
            return characterData[progressionData.currentlySelectedCharacter].skins.Except(progressionData.currentCharacterProgress.unlockedSkins).FirstOrDefault();
        }

        private static Character NextUnlockableCharacter()
        {
            return characterData.Keys.Except(progressionData.unlockedCharacters).FirstOrDefault();
        }

        internal static Color TokenRedColor = new Color(248f / 255f, 89f / 255f, 93f / 255f);
        internal static Color TokenBlueColor = RainWorld.AntiGold.rgb;
        internal static Color TokenGoldColor = RainWorld.GoldRGB;

        public static void LoadProgression()
        {
            if (progressionData != null) return;
            // todo unfake me
            progressionData = new ProgressionData();
            progressionData.unlockedCharacters = new() { Character.Slugcat, Character.Cicada, Character.Lizard };
            progressionData.characterProgress = progressionData.unlockedCharacters.Select(c => new KeyValuePair<Character, ProgressionData.CharacterProgressionData>(c, new ProgressionData.CharacterProgressionData()
            {
                unlockedEmotes = new List<Emote>() { Emote.emoteHello, Emote.emoteHappy, Emote.emoteSad },
                unlockedSkins = characterData[c].skins.Take(1).ToList()
            })).ToDictionary();
            progressionData.currentlySelectedCharacter = Character.Lizard;
            progressionData.SetSelectedCharacter(progressionData.currentlySelectedCharacter);
        }

        public static ProgressionData progressionData;
        internal static int emoteProgressTreshold = 4;
        internal static int skinProgressTreshold = 6;
        internal static int characterProgressTreshold = 8;
        public static List<Emote> emoteEmotes = new()
        {
            Emote.emoteHello,
            Emote.emoteHappy,
            Emote.emoteSad,
            Emote.emoteConfused,
            Emote.emoteGoofy,
            Emote.emoteDead,
            Emote.emoteAmazed,
            Emote.emoteShrug,
            Emote.emoteHug,
            Emote.emoteAngry,
            Emote.emoteWink,
            Emote.emoteMischievous,
        };

        public class ProgressionData
        {
            internal int characterUnlockProgress;
            internal List<Character> unlockedCharacters;
            internal Dictionary<Character, CharacterProgressionData> characterProgress;
            internal Character currentlySelectedCharacter;

            public void SetSelectedCharacter(Character character)
            {
                currentlySelectedCharacter = character;
                if (!characterProgress.ContainsKey(character)) characterProgress[character] = new CharacterProgressionData();
            }

            internal CharacterProgressionData currentCharacterProgress => characterProgress[currentlySelectedCharacter];

            internal class CharacterProgressionData
            {
                internal int emoteUnlockProgress;
                internal int skinUnlockProgress;
                internal List<Emote> unlockedEmotes;
                internal List<Skin> unlockedSkins;
            }
        }
    }
}
