using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CalamityMod.UI.DialogueDisplay;
using CalamityMod.Utilities;
using MonoMod.Cil;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Dialogues;

internal record DialogueTextDataEntry(
    Mod ProviderMod,
    string FilePath,
    string DialogueKey,
    DialogueTextData Data
    );

internal partial class DialogueLoader : ModSystem
{
    private const string DialogueFilePrefix = "Dialogue.";

    private static readonly Dictionary<string, DialogueTextDataEntry> _DialogueLookup = [];
    private static readonly Dictionary<Mod, MainThreadedFileSystemWatcher> _Watchers = [];

    public override void Load()
    {
        _DialogueLookup.Clear();

        var method = typeof(LocalizationLoader).GetMethod("ExtractLocalizationFiles", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        if (method != null)
        {
            MonoModHooks.Modify(method, ExtractDialogueFilesPatch);
        }
    }

    public override void PostSetupContent()
    {
        foreach (var mod in ModLoader.Mods)
        {
            if (mod.GetFileNames() == null)
                continue;

            var path = mod.SourceFolder;
            if (!Directory.Exists(path))
                continue;

            var watcher = new MainThreadedFileSystemWatcher()
            {
                Path = path,
                Filter = "*.json",
                FileNameFilter = CalamityDialogueFileRegex(),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true
            };
            watcher.Changed += (arg) =>
            {
                HandleFileUpdate(mod, arg.FullPath);
            };
            watcher.Renamed += (arg) =>
            {
                HandleFileUpdate(mod, arg.FullPath);
            };
            watcher.EnableRaisingEvents = true;
            _Watchers[mod] = watcher;
        }
    }

    public override void Unload()
    {
        _DialogueLookup.Clear();

        foreach (var watcher in _Watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _Watchers.Clear();
    }

    private static void ExtractDialogueFilesPatch(ILContext il)
    {
        var cursor = new ILCursor(il);

        int pathLdloc = -1;
        int modLdloc = -1;
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdloc(out modLdloc), // Mod mod
            i => i.MatchLdloc(out pathLdloc), // string path
            i => i.MatchCallOrCallvirt(out _), // GameCulture ActiveCulture
            i => i.MatchCallOrCallvirt(typeof(LocalizationLoader), "UpdateLocalizationFilesForMod")))
        {
            CalamityMod.Log.ILFailure("Force Extract Dialogue Files", "Unable to locate UpdateLocalizationFilesForMod call");
        }

        if (modLdloc == -1)
        {
            CalamityMod.Log.ILFailure("Force Extract Dialogue Files", $"Unable to locate ldloc index for mod");
        }

        if (pathLdloc == -1)
        {
            CalamityMod.Log.ILFailure("Force Extract Dialogue Files", $"Unable to locate ldloc index for path");
        }

        cursor.EmitLdloc(modLdloc);
        cursor.EmitLdloc(pathLdloc);
        cursor.EmitDelegate((Mod mod, string basePath) =>
        {
            foreach (var entry in GetDialogueTextEntries(mod, GameCulture.DefaultCulture, skipDeserializeData: true))
            {
                try
                {
                    var destFilePath = Path.Combine(basePath, entry.FilePath);
                    var destDir = Path.GetDirectoryName(destFilePath);
                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                    using var stream = mod.GetFileStream(entry.FilePath);
                    using var fileStream = File.OpenWrite(destFilePath);
                    using var writer = new StreamWriter(fileStream, Encoding.UTF8);
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    writer.Write(reader.ReadToEnd());
                }
                catch (Exception e)
                {
                    CalamityMod.Log.Error($"Error while exporting DialogueTextData entry ({mod.Name}::{entry.FilePath}): {e}");
                }
            }
        });
    }

    public static bool TryGetDialogue(string dialogueKey, out DialogueTextData data)
    {
        if (_DialogueLookup.TryGetValue(dialogueKey, out var entry))
        {
            data = entry.Data;
            return true;
        }

        data = null;
        return false;
    }

    public override void OnLocalizationsLoaded()
    {
        _DialogueLookup.Clear();

        // Mods should be sorted by dependency order.
        foreach (var entry in GetDialogueTextEntiresForAllMods(GameCulture.DefaultCulture))
        {
            if (_DialogueLookup.TryGetValue(entry.DialogueKey, out var oldEntry))
            {
                if (entry.Data.Revision != oldEntry.Data.Revision)
                {
                    CalamityMod.Log.Warn($"Dialogue Localization was detected but revision mismatches. This will not be applied! : '{entry.ProviderMod.Name}::{entry.FilePath}'");
                    continue;
                }
            }

            _DialogueLookup[entry.DialogueKey] = entry;
        }

        var activeCulture = LanguageManager.Instance.ActiveCulture;
        if (activeCulture == GameCulture.DefaultCulture)
            return;

        foreach (var entry in GetDialogueTextEntiresForAllMods(activeCulture))
        {
            var mod = entry.ProviderMod;

            if (!_DialogueLookup.TryGetValue(entry.DialogueKey, out var oldEntry))
            {
                CalamityMod.Log.Warn($"Dialogue Localization was detected but original Dialogue file does not exist. This will not be applied! : '{mod.Name}::{entry.FilePath}'");
                continue;
            }

            // Skip if entry is from same file.
            if (oldEntry.ProviderMod == entry.ProviderMod && oldEntry.FilePath == entry.FilePath)
            {
                continue;
            }

            if (oldEntry.Data.Revision != entry.Data.Revision)
            {
                CalamityMod.Log.Warn($"Dialogue Localization was detected but revision mismatches. This will not be applied! : '{mod.Name}::{entry.FilePath}'");
                continue;
            }

            _DialogueLookup[entry.DialogueKey] = entry;
        }
    }

    private static void HandleFileUpdate(Mod mod, string filePath)
    {
        if (!TryGetDialogueFileInfo(filePath, out _, out _, out var dialogueKey))
            return;

        if (!_DialogueLookup.TryGetValue(dialogueKey, out var existingEntry))
            return;

        if (existingEntry.ProviderMod != mod)
            return;

        try
        {
            using var stream = new StreamReader(File.OpenRead(filePath), Encoding.UTF8);
            _DialogueLookup[dialogueKey] = existingEntry with
            {
                Data = JsonSerializer.Deserialize<DialogueTextData>(stream.BaseStream)
            };

            var hotreloadedMessage = $"Dialogue entry has been hot reloaded: '{dialogueKey}', from source: '{filePath}'";
            CalamityMod.Log.Info(hotreloadedMessage);
            if (!Main.gameMenu) Main.NewText(hotreloadedMessage);
        }
        catch (Exception e)
        {
            CalamityMod.Log.Error($"Error while hot reloading DialogueTextData entry ({filePath}): {e}");
        }
    }

    private static IEnumerable<DialogueTextDataEntry> GetDialogueTextEntiresForAllMods(GameCulture targetCulture, bool skipDeserializeData = false)
    {
        return ModLoader.Mods
            .Where(mod => mod.GetFileNames() != null)
            .SelectMany(mod => GetDialogueTextEntries(mod, targetCulture, skipDeserializeData));
    }

    private static IEnumerable<DialogueTextDataEntry> GetDialogueTextEntries(Mod mod, GameCulture targetCulture, bool skipDeserializeData = false)
    {
        if (mod == null)
            yield break;

        if (mod.GetFileNames() == null)
            yield break;

        foreach (string fileName in mod.GetFileNames())
        {
            if (!TryGetDialogueFileInfo(fileName, out var culture, out var prefix, out var dialogueKey))
                continue;

            if (culture != targetCulture)
                continue;

            DialogueTextData data = null;
            if (!skipDeserializeData)
            {
                try
                {
                    using var stream = new StreamReader(mod.GetFileStream(fileName), Encoding.UTF8);
                    data = JsonSerializer.Deserialize<DialogueTextData>(stream.BaseStream);
                }
                catch (Exception e)
                {
                    CalamityMod.Log.Error($"Error while reading DialogueTextData entry ({mod.Name}::{fileName}): {e}");
                }
            }

            if (data != null || skipDeserializeData)
            {
                yield return new DialogueTextDataEntry(mod, fileName, dialogueKey, data);
            }
        }
    }

    private static bool TryGetDialogueFileInfo(string filePath, out GameCulture culture, out string prefix, out string dialogueKey)
    {
        if (!Path.GetExtension(filePath).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
            goto EXIT_INVALID;

        if (!LocalizationLoader.TryGetCultureAndPrefixFromPath(filePath, out culture, out prefix))
            goto EXIT_INVALID;

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        if (fileName.StartsWith($"{prefix}_{DialogueFilePrefix}", StringComparison.InvariantCultureIgnoreCase))
        {
            dialogueKey = fileName[$"{prefix}_{DialogueFilePrefix}".Length..];
            return true;
        }
        else if (fileName.StartsWith(DialogueFilePrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            dialogueKey = fileName[DialogueFilePrefix.Length..];
            return true;
        }

EXIT_INVALID:
        culture = null;
        prefix = null;
        dialogueKey = null;
        return false;
    }

    private static readonly Regex CalamityDialogueFilePattern = new(@"Dialogue\..+?\.jsonc?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static Regex CalamityDialogueFileRegex() => CalamityDialogueFilePattern;
}
