#!/usr/bin/env python3

import sys

def get_scene_class_mode(name):
    if "/Scene/Point/" in name:
        return ("ConfigScene", "s")

    if "/Scene/SceneNpcBorn/" in name:
        return ("ConfigLevelNpcBornPos", "s")

    if "/Scene/SceneNpcBornNoGroup/" in name:
        return ("ConfigLevelNpcBornPosNoGroup", "s")

    if "/Scene/LevelLayout/" in name:
        return ("ConfigLevelLayout", "s")

    if "/Scene/WorldArea/" in name:
        return ("ConfigWorldAreaLayout", "s")

    raise NotImplementedError(name)

def get_voice_class_mode(name):
    if "/Voice/Items/" in name:
        return ("ConfigExternalVoiceItem", "d")

    if "/Voice/Lut/" in name:
        return ("ConfigExternalVoiceLookupItem", "s") # TODO: Unknown

    if "/Voice/Emo/" in name:
        return ("ConfigExternalVoiceLookupItem", "s") # TODO: Unknown

    raise NotImplementedError(name)

def get_talk_class_mode(name):
    if "/Talk/Activity/" in name:
        return ("ConfigActivityDialogGroup", "s")

    if "/Talk/ActivityGroup/" in name:
        return ("ConfigActivityTalkScheme", "s")

    if "/Talk/Blossom/" in name:
        return ("ConfigBlossomDialogGroup", "s")

    if "/Talk/BlossomGroup/" in name:
        return ("ConfigTalkScheme", "s")

    if "/Talk/Gadget/" in name:
        return ("ConfigGadgetDialogGroup", "s")

    if "/Talk/GadgetGroup/" in name:
        return ("ConfigGadgetTalkScheme", "s")

    if "/Talk/Npc/" in name:
        return ("ConfigNarratorDialogGroup", "s")

    if "/Talk/NpcGroup/" in name:
        return ("ConfigNpcTalkScheme", "s")

    if "/Talk/Coop/" in name:
        return ("ConfigCoopDialogGroup", "s")

    if "/Talk/FreeGroup/" in name:
        return ("ConfigFreeDialogGroup", "s")

    if "/Talk/Other/" in name:
        return ("ConfigDialogGroup", "s")

    if "/Talk/Quest/" in name:
        return ("ConfigDialogGroup", "s-") # HACK!

    raise NotImplementedError(name)

def get_music_class_mode(name):
    if "/Music/Music/" in name:
        return ("ConfigMusic", "s")

    if "/Music/Songs/" in name:
        return ("ConfigSong", "l")

    if "/Music/TransitionConditions/" in name:
        return ("ConfigMusicCondition", "l")

    raise NotImplementedError(name)

def get_leveldesign_class_mode(name):
    if "/LevelDesign/ActionPoints/" in name:
        return ("ConfigLevelActionPoint", "s")

    if "/LevelDesign/Gadgets/" in name:
        return ("ConfigLevelGadgetData", "s")

    if "/LevelDesign/Monsters/" in name:
        return ("ConfigLevelMonsterData", "s")

    if "/LevelDesign/Polygons/" in name:
        return ("ConfigLevelPolygons", "s")

    if "/LevelDesign/Routes/" in name:
        return ("ConfigLevelRoute", "s")

    if "/LevelDesign/SimplePolygons/" in name:
        return ("ConfigPolygonArea", "s")

    if "/LevelDesign/Meta/" in name:
        return ("ConfigLevelMeta", "s")

    raise NotImplementedError(name)

def get_homefurniture_class_mode(name):
    if "/HomeFurniture/Fishpond/" in name:
        return ("ConfigHomeFishpondSet", "s")

    if "/HomeFurniture/HomeworldGroup/" in name:
        return ("ConfigHomeworldFurnitureSet", "s")

    raise NotImplementedError(name)

def get_global_class_mode(name):
    if "/Global/Embeded/TextMap/" in name:
        return ("ConfigTextMapLevel", "s")

    if "/Global/Embeded/UI/" in name:
        return ("ConfigUIGlobal", "s")

    if "/Global/Embeded/UI/" in name:
        return ("ConfigUIGlobal", "s")

    if "/Global/GlobalValues/" in name:
        return ("ConfigGlobalValues", "s")

    if "/Global/Mark/" in name:
        return ("ConfigMarkGlobal", "s")

    if "/Global/GlobalData" in name:
        return ("ConfigModeStateMap", "s")

    raise NotImplementedError(name)

def get_audio_class_mode(name):
    if "/Audio/Ambience/" in name:
        return ("ConfigAudioAmbience", "s")

    if "/Audio/Area2DAmbience/" in name:
        return ("ConfigAudioArea2DAmbience", "l")

    if "/Audio/Avatar/" in name:
        return ("ConfigAudioAvatar", "s")

    if "/Audio/AvatarMove/" in name:
        return ("ConfigAudioAvatarMove", "s")

    if "/Audio/AvatarSpeech/" in name:
        return ("ConfigAvatarSpeech", "s")

    if "/Audio/BaseMove/" in name:
        return ("ConfigAudioBaseMove", "s")

    if "/Audio/Camera/" in name:
        return ("ConfigAudioCamera", "s")

    if "/Audio/CityBlocks/" in name:
        return ("ConfigAudioCityBlocks", "s")

    if "/Audio/Combat/" in name:
        return ("ConfigAudioCombat", "s")

    if "/Audio/Dialog/" in name:
        return ("ConfigAudioDialog", "s")

    if "/Audio/EventCulling/" in name:
        return ("ConfigAudioEventCulling", "s")

    if "/Audio/Gadget/" in name:
        return ("ConfigAudioGadget", "s")

    if "/Audio/GameViewState/" in name:
        return ("ConfigAudioGameViewState", "s")

    if "/Audio/General/" in name:
        return ("ConfigAudioGeneral", "s")

    if "/Audio/Level/" in name:
        return ("ConfigAudioLevel", "s")

    if "/Audio/Listener/" in name:
        return ("ConfigAudioListener", "s")

    if "/Audio/MapInfo/" in name:
        return ("ConfigAudioMapInfo", "s")

    if "/Audio/Midi/" in name:
        return ("ConfigAudioMIDI", "s")

    if "/Audio/Music/" in name:
        return ("ConfigAudioMusic", "s")

    if "/Audio/MutualExclusion/" in name:
        return ("ConfigAudioMutualExclusion", "s")

    if "/Audio/Npc/" in name:
        return ("ConfigAudioNpc", "s")

    if "/Audio/Quest/" in name:
        return ("ConfigAudioQuest", "s")

    if "/Audio/Resource/" in name:
        return ("ConfigAudioResource", "s")

    if "/Audio/Setting/" in name:
        return ("ConfigAudioSetting", "s")

    if "/Audio/Spatial/" in name:
        return ("ConfigSpatialAudio", "s")

    if "/Audio/SurfaceType/" in name:
        return ("ConfigAudioSurfaceType", "s")

    if "/Audio/UI/" in name:
        return ("ConfigAudioUi", "s")

    if "/Audio/Weather/" in name:
        return ("ConfigAudioWeather", "s")

    raise NotImplementedError(name)

def get_class_mode(name):
    if "/RandomQuest/" in name:
        return ("ConfigRandomQuestGlobalScheme", "s")

    if "/Talk/" in name:
        return get_talk_class_mode(name)

    if "/Ability/" in name:
        return ("ConfigAbility", "ld")

    if "/AbilityActionChannel/" in name:
        return ("ConfigPlatformActionTokenChannel", "s")

    if "/AbilityGroup/" in name:
        return ("ConfigAbilityGroup", "d")

    if "/AbilityPath/" in name:
        return ("ConfigAbilityPath", "s")

    if "/AbilitySystem/" in name:
        return ("ConfigAbilitySystem", "s")

    if "/AnimPattern/" in name:
        return ("ConfigAnimPatternPath", "s")

    if "/AnimatorConfig/" in name:
        return ("ConfigAnimator", "s")

    if "/AttachData/" in name:
        return ("ConfigAttachmentData", "s")

    if "/Audio/" in name:
        return get_audio_class_mode(name)

    if "/AudioEmitter/" in name:
        return ("ConfigAudioEmitter", "s")

    if "/Avatar/" in name:
        return ("ConfigAvatar", "s")

    if "/Boundary/" in name:
        return ("ConfigBoundary", "d")

    if "/Climate/" in name:
        return ("ConfigClimate", "s")

    if "/CodexQuest/" in name:
        return ("ConfigCodexQuest", "s")

    if "/Common/ConfigGlobalCombat" in name:
        return ("ConfigGlobalCombat", "s")

    if "/Component/LCBaseIntee" in name:
        return ("ConfigLCBaseIntee", "s")

    if "/Component/LCGadgetIntee" in name:
        return ("ConfigLCGadgetIntee", "s")

    if "/Coop/" in name:
        return ("ConfigMainCoopGroup", "s")

    if "/CrowdGroupInfos/" in name:
        return ("ConfigCrowdGroupInfo", "l")

    if "/CrowdSpawnInfos/" in name:
        return ("ConfigCrowdSpawnInfos", "s")

    if "/Cutscene/" in name:
        return ("ConfigCutsceneIndex", "s")

    if "/DynamicAbilityPreload/" in name:
        return ("ConfigDynamicAbilityPreload", "d")

    if "/Effect/" in name:
        return ("ConfigEffectData", "s")

    if "/EmojiBubble/" in name:
        return ("ConfigEmojiBubbleData", "d")

    if "/EntityBan/" in name:
        return ("ConfigEntityBanData", "s")

    if "/EntityReuse/" in name:
        return ("ConfigEntityReuse", "s")

    if "/Fashion/AvatarCostume/" in name:
        return ("ConfigCostumeInfo", "d")

    if "/Fashion/Flycloak/" in name:
        return ("ConfigFlycloakFashion", "s")

    if "/Gadget/" in name:
        return ("ConfigGadget", "d")

    if "/GadgetPath/" in name:
        return ("ConfigGadgetPath", "s")

    if "/Global/" in name:
        return get_global_class_mode(name)

    if "/Goddess/" in name:
        return ("ConfigResonanceCutSceneMap", "s")

    if "/GraphicsSetting/" in name:
        return ("ConfigPlatformGrahpicsSetting", "s")

    if "/Guide/" in name:
        return ("ConfigGuideTask", "d")

    if "/GuideContextList/" in name:
        return ("ConfigGuideContextList", "d")

    if "/HomeFurniture/" in name:
        return get_homefurniture_class_mode(name)

    if "/Homeworld/" in name:
        return ("ConfigHomePlaceColPath", "d")

    if "/HomeworldDefaultSave/" in name:
        return ("ConfigHomeworldDefaultSave", "s")

    if "/HomeworldFurnitureSuit/" in name:
        return ("ConfigHomeworldFurnitureSet", "s")

    if "/IndexDic/" in name:
        return ("ConfigIndexDic", "s") # TODO: Unknown, stub

    if "/Indicator/" in name:
        return ("ConfigUIIndicator", "d")

    if "/InterAction/" in name:
        return ("ConfigInterContainer", "s")

    if "/KeyboardLayout/" in name:
        return ("ConfigKeyboardLayout", "s")

    if "/LanguageSetting/" in name:
        return ("ConfigLanguageSetting", "s")

    if "/LevelDesign/" in name:
        return get_leveldesign_class_mode(name)

    if "/LevelEntity/" in name:
        return ("ConfigLevelEntity", "d")

    if "/LogoPage/" in name:
        return ("ConfigLogoPageSetting", "s")

    if "/LuaHackConfig/" in name:
        return ("ConfigLuaHack", "s")

    if "/MainPageDisableInfo/" in name:
        return ("ConfigMainPageDisableInfo", "d")

    if "/Mark/" in name:
        return ("ConfigMapGlobal", "s")

    if "/Monster/" in name:
        return ("ConfigMonster", "s")

    if "/MultiPlatformUIData/" in name:
        return ("ConfigMutiPlatformUIData", "s")

    if "/Music/" in name:
        return get_music_class_mode(name)

    if "/MusicGame/" in name:
        return ("ConfigMusicGame", "s")

    if "/MusicGameCamera/" in name:
        return ("ConfigMusicGameCamera", "s")

    if "/Npc/" in name:
        return ("ConfigNpc", "s")

    if "/PlayMode/" in name:
        return ("ConfigModeStateMap", "s")

    if "/Preload/" in name:
        return ("ConfigFullPreload", "s")

    if "/PS4/" in name:
        return ("ConfigPS4TRC", "s")

    if "/Quest/" in name:
        return ("ConfigMainQuestScheme", "s")

    if "/QuestBrief/" in name:
        return ("ConfigMainQuestBrief", "s")

    if "/SCameraMove/" in name:
        return ("ConfigCameraMove", "d") # Not sure!

    if "/Scene/" in name:
        return get_scene_class_mode(name)

    if "/Schedule/" in name:
        return ("ConfigJobData", "s")

    if "/Shape/" in name:
        return ("ConfigBaseShape", "d")

    if "/Skin/" in name:
        return ("ConfigSkin", "s")

    if "/SoundBank/" in name:
        return ("ConfigSoundBankLookup", "s")

    if "/StageAudio/" in name:
        return ("ConfigAudioStageEvents", "s")

    if "/StreamPolygon/" in name:
        return ("ConfigLevelPolygon", "s")

    if "/Talent/" in name:
        return ("ConfigTalentMixin", "dl")

    if "/Tile/" in name:
        return ("ConfigTile", "l")

    if "/UI/" in name:
        return ("ConfigUI", "s")

    if "/Voice/" in name:
        return get_voice_class_mode(name)

    if "/Widget/" in name:
        return ("ConfigWidget", "s")

    if "/WidgetNew/" in name:
        return ("ConfigMainWidgetToy", "s")

    raise NotImplementedError(name)

for line in sys.stdin:
    print("{0} {1}".format(*get_class_mode(line.strip())))
