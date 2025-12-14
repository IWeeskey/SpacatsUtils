#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CreateAssetMenu(menuName = "Spacats/Enum Edit Asset", fileName = "EnumEditAsset")]
    /// <summary>
    /// Editor ScriptableObject that helps validate and append string tags as enum members into a target .cs enum file.
    /// It sanitizes provided names, generates stable int values via FNV-1a 32-bit hash, and safely writes them into the enum.
    /// </summary>
    public sealed class EnumEditAsset : ScriptableObject
    {
        // Reference to the .cs file that contains the enum to read/modify
        [SerializeField]
        public MonoScript  _enumScriptFile;
        
        // Prefix added to all log messages from this tool
        private string _logPrefix = "[EnumEdit]";

        // List of strings to be added as new enum members
        // The name NewTagsToAdd is kept as requested by the task
        [SerializeField]
        public List<string> NewTagsToAdd = new List<string>();

        /// <summary>
        /// Main editor entry point that validates NewTagsToAdd and appends valid ones to the target enum file.
        /// </summary>
        /// <remarks>
        /// Local variables used:
        /// - enumLines: temporary collector for lines inside the enum body (kept for potential future use).
        /// - _enumScriptFile: reference to the MonoScript which contains the enum. Used to resolve the asset path and enum name.
        /// - path: file system path to the target enum .cs file.
        /// - allLines: content of the target file split by lines.
        /// - foundEnum/startedBlock/braceDepth: state flags used by ProcessEnumLine to detect and traverse the enum body.
        /// - enumName: assumed to be the same as MonoScript.name; used to locate the enum declaration.
        /// </remarks>
        public void AddToEnum()
        {
            // Temporary list for enum body lines (currently unused but kept for potential future use)
            var enumLines = new List<string>();

            if (_enumScriptFile == null)
            {
                ShowLog("[TagEdit] Не задан файл скрипта с enum. Укажите _enumScriptFile.", LogType.Error);
                return;
            }

            string path = AssetDatabase.GetAssetPath(_enumScriptFile);
            if (string.IsNullOrEmpty(path))
            {
                ShowLog("[TagEdit] Путь к файлу enum пуст. Укажите корректный скрипт.", LogType.Error);
                return;
            }

            if (!File.Exists(path))
            {
                ShowLog($"[TagEdit] Файл не найден: {path}", LogType.Error);
                return;
            }
            
            // Validate provided tags before any processing
            ValidateNewTags();

            string[] allLines = File.ReadAllLines(path);
            if (allLines == null || allLines.Length == 0)
            {
                ShowLog("[TagEdit] Файл пуст.", LogType.Error);
                return;
            }

            // Search for enum declaration by name and collect lines inside its curly braces
            bool foundEnum = false;
            bool startedBlock = false;
            int braceDepth = 0;

            // Assume enum name equals the script file name (Unity MonoScript.name)
            string enumName = _enumScriptFile != null ? _enumScriptFile.name : null;

            for (int i = 0; i < allLines.Length; i++)
            {
                string line = allLines[i];
                bool shouldBreak = ProcessEnumLine(line, enumName, enumLines, ref foundEnum, ref startedBlock, ref braceDepth);
                if (shouldBreak) break;
            }
            
            // Add validated tags to the enum
            AddValidatedTagsToEnum();
            NewTagsToAdd.Clear();
        }

        /// <summary>
        /// Validates and sanitizes all strings in NewTagsToAdd, rewriting the list with cleaned values.
        /// </summary>
        /// <remarks>
        /// - NewTagsToAdd: input/output list of raw tag strings provided by the user. After validation it contains sanitized enum-safe names.
        /// - validated: local temporary list that accumulates cleaned names produced by ProcessNewTag.
        /// The method also marks this ScriptableObject dirty and saves assets to persist the updated list in the project.
        /// </remarks>
        private void ValidateNewTags()
        {
            if (NewTagsToAdd == null)
            {
                return;
            }

            var validated = new List<string>(NewTagsToAdd.Count);

            for (int i = 0; i < NewTagsToAdd.Count; i++)
            {
                ProcessNewTag(NewTagsToAdd[i], validated);
            }

            NewTagsToAdd = validated;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Cleans a single raw tag string into a valid enum member name and appends it to the validated list if not empty.
        /// </summary>
        /// <param name="original">Raw user-provided tag string.</param>
        /// <param name="validated">Accumulator list that receives the sanitized name.</param>
        /// <remarks>
        /// Local variables used:
        /// - trimmed/lower: normalized versions of the input string to simplify validation.
        /// - sb: StringBuilder used to construct a filtered identifier (a-z, 0-9, '_').
        /// - lastUnderscore: tracks whether the last emitted character was '_' to avoid duplicates.
        /// - start/end: indices used to trim leading/trailing underscores from the result.
        /// - fixedStr: final sanitized name adjusted to not start with a digit.
        /// Logs warnings for null or fully invalid strings; logs info when an input is auto-corrected.
        /// </remarks>
        private void ProcessNewTag(string original, List<string> validated)
        {
            if (original == null)
            {
                ShowLog("[TagEdit] Найдена null-строка в списке тегов. Пропускаю.", LogType.Warning);
                return;
            }

            string trimmed = original.Trim();
            string lower = trimmed.ToLowerInvariant();

            // Allowed characters: a-z, 0-9, _
            var sb = new StringBuilder(lower.Length);
            bool lastUnderscore = false;
            for (int c = 0; c < lower.Length; c++)
            {
                char ch = lower[c];
                bool isAllowed = (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_';
                if (isAllowed)
                {
                    sb.Append(ch);
                    lastUnderscore = ch == '_';
                }
                else
                {
                    if (!lastUnderscore)
                    {
                        sb.Append('_');
                        lastUnderscore = true;
                    }
                }
            }

            // Remove leading/trailing underscores if they ended up there
            int start = 0;
            int end = sb.Length - 1;
            while (start <= end && sb.Length > 0 && sb[start] == '_') { start++; }
            while (end >= start && sb.Length > 0 && sb[end] == '_') { end--; }

            string fixedStr;
            if (start <= end)
            {
                fixedStr = sb.ToString(start, end - start + 1);
            }
            else
            {
                fixedStr = string.Empty;
            }

            // Name must not start with a digit — prepend an underscore
            if (!string.IsNullOrEmpty(fixedStr))
            {
                char first = fixedStr[0];
                if (first >= '0' && first <= '9')
                {
                    fixedStr = "_" + fixedStr;
                }
            }

            if (string.IsNullOrEmpty(fixedStr))
            {
                ShowLog($"[TagEdit] В теге \"{original}\" найден некорректный формат. После очистки строка пуста — тег пропущен.", LogType.Warning);
                return;
            }

            if (fixedStr != original)
            {
                ShowLog($"[TagEdit] В теге \"{original}\" найден некорректный формат и исправлен на \"{fixedStr}\".", LogType.Log);
            }

            validated.Add(fixedStr);
        }

        /// <summary>
        /// Adds the validated names from NewTagsToAdd into the target enum source file.
        /// </summary>
        /// <remarks>
        /// Uses:
        /// - _enumScriptFile: to resolve the file path and enum name.
        /// - path: full path to the target .cs file.
        /// - allLines: all lines of the target file used for analysis and insertion.
        /// - enumName: name of the enum assumed to match MonoScript.name.
        /// - enumDeclLine/enumEndBraceLine: indices of the enum declaration and its closing brace, computed by FindEnumRange.
        /// - existingNames/existingNameToValue/existingValues/maxValue: collections filled by CollectEnumInfo describing existing members and their numeric values.
        /// - newNames: only names that are not already present in the enum.
        /// - toInsert: final list of "Name = Id," lines to inject before the closing brace.
        /// </remarks>
        private void AddValidatedTagsToEnum()
        {
            if (_enumScriptFile == null)
            {
                ShowLog("[TagEdit] Не задан файл скрипта с enum. Укажите _enumScriptFile.", LogType.Error);
                return;
            }
            if (NewTagsToAdd == null || NewTagsToAdd.Count == 0)
            {
                ShowLog("[TagEdit] Нет новых тегов для добавления. Список пуст.", LogType.Log);
                return;
            }

            string path = AssetDatabase.GetAssetPath(_enumScriptFile);
            if (string.IsNullOrEmpty(path))
            {
                ShowLog("[TagEdit] Путь к файлу enum пуст. Укажите корректный скрипт.", LogType.Error);
                return;
            }
            if (!File.Exists(path))
            {
                ShowLog($"[TagEdit] Файл не найден: {path}", LogType.Error);
                return;
            }

            string[] allLines = File.ReadAllLines(path);
            if (allLines == null || allLines.Length == 0)
            {
                ShowLog("[TagEdit] Файл пуст.", LogType.Error);
                return;
            }

            string enumName = _enumScriptFile != null ? _enumScriptFile.name : null;
            if (string.IsNullOrEmpty(enumName))
            {
                ShowLog("[TagEdit] Не удалось определить имя enum по файлу скрипта.", LogType.Error);
                return;
            }

            // Find enum body range
            if (!FindEnumRange(allLines, enumName, out int enumDeclLine, out int enumEndBraceLine))
            {
                ShowLog($"[TagEdit] Не удалось найти тело enum {enumName} для редактирования.", LogType.Error);
                return;
            }

            // Gather information about existing enum members
            var existingNames = new List<string>();
            var existingNameToValue = new Dictionary<string, int>();
            var existingValues = new HashSet<int>();
            int maxValue = int.MinValue;
            CollectEnumInfo(allLines, enumDeclLine, enumEndBraceLine, existingNames, existingNameToValue, existingValues, ref maxValue);

            // Filter only new names
            var newNames = new List<string>();
            for (int i = 0; i < NewTagsToAdd.Count; i++)
            {
                string candidate = NewTagsToAdd[i];
                if (string.IsNullOrEmpty(candidate)) continue;
                bool exists = false;
                for (int j = 0; j < existingNames.Count; j++)
                {
                    if (existingNames[j] == candidate) { exists = true; break; }
                }
                if (!exists) newNames.Add(candidate);
                else ShowLog($"[TagEdit] Тег '{candidate}' уже существует в enum — пропущен.", LogType.Log);
            }
            if (newNames.Count == 0)
            {
                ShowLog("[TagEdit] Нет новых уникальных тегов для добавления.", LogType.Log);
                return;
            }

            var toInsert = BuildInsertLines(newNames, existingValues, existingNameToValue);
            if (toInsert.Count == 0)
            {
                ShowLog("[TagEdit] Новые теги не добавлены из-за коллизий или дубликатов.", LogType.Warning);
                return;
            }

            // Insert lines before the enum's closing brace
            InsertBeforeClosingBrace(path, allLines, enumEndBraceLine, toInsert);
        }

        /// <summary>
        /// Locates the range of lines that define the target enum body within the provided file lines.
        /// </summary>
        /// <param name="allLines">All lines of the source file.</param>
        /// <param name="enumName">The name of the enum to search for.</param>
        /// <param name="enumDeclLine">Output: index of the line where the enum is declared.</param>
        /// <param name="enumEndBraceLine">Output: index of the line with the enum's closing brace.</param>
        /// <returns>True if the enum declaration and its closing brace were found; otherwise false.</returns>
        /// <remarks>
        /// Local variables used:
        /// - depth: tracks current brace depth while scanning.
        /// - started: marks when we've encountered the first '{' of the enum.
        /// The method scans from the enum declaration line to the end to find the matching closing brace.
        /// </remarks>
        private bool FindEnumRange(string[] allLines, string enumName, out int enumDeclLine, out int enumEndBraceLine)
        {
            enumDeclLine = -1;
            enumEndBraceLine = -1;
            if (allLines == null || allLines.Length == 0 || string.IsNullOrEmpty(enumName)) return false;

            for (int i = 0; i < allLines.Length; i++)
            {
                string line = allLines[i];
                if (!line.Contains("enum " + enumName)) continue;

                enumDeclLine = i;
                int depth = 0;
                bool started = false;

                // Count braces starting from the found line to the end of the file
                for (int j = i; j < allLines.Length; j++)
                {
                    string l = allLines[j];
                    for (int c = 0; c < l.Length; c++)
                    {
                        char ch = l[c];
                        if (ch == '{') { depth++; started = true; }
                        else if (ch == '}') { depth--; }
                    }

                    if (started && depth == 0)
                    {
                        enumEndBraceLine = j;
                        return true;
                    }
                }

                // If we've reached here — the closing brace was not found
                return false;
            }

            return false;
        }

        /// <summary>
        /// Scans the enum body and collects information about existing members: names and assigned integer values.
        /// </summary>
        /// <param name="allLines">All lines of the source file.</param>
        /// <param name="enumDeclLine">Line index where the enum is declared.</param>
        /// <param name="enumEndBraceLine">Line index of the enum's closing brace.</param>
        /// <param name="existingNames">Output list receiving all member names in the enum.</param>
        /// <param name="existingNameToValue">Output map from member name to its explicit numeric value, if present.</param>
        /// <param name="existingValues">Output set of all explicit numeric values encountered.</param>
        /// <param name="maxValue">Reference accumulator for the maximum explicit value found.</param>
        /// <remarks>
        /// Local variables used:
        /// - depth/started: track when we are inside the enum braces.
        /// - trimmed: per-line content used to skip empty/comment lines.
        /// The helper TryParseEnumMember extracts a member name and optional value from each meaningful line.
        /// </remarks>
        private void CollectEnumInfo(string[] allLines,
                                     int enumDeclLine,
                                     int enumEndBraceLine,
                                     List<string> existingNames,
                                     Dictionary<string, int> existingNameToValue,
                                     HashSet<int> existingValues,
                                     ref int maxValue)
        {
            int depth = 0;
            bool started = false;

            for (int i = enumDeclLine; i <= enumEndBraceLine; i++)
            {
                string line = allLines[i];
                for (int c = 0; c < line.Length; c++)
                {
                    char ch = line[c];
                    if (ch == '{') { depth++; started = true; }
                    else if (ch == '}') { depth--; }
                }

                if (!started || depth < 1) continue;

                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (trimmed.StartsWith("//")) continue;

                if (TryParseEnumMember(trimmed, out string name, out int? value))
                {
                    if (!string.IsNullOrEmpty(name)) existingNames.Add(name);

                    if (!value.HasValue) continue;
                    
                    int v = value.Value;
                    if (v > maxValue) maxValue = v;
                    if (!string.IsNullOrEmpty(name)) existingNameToValue[name] = v;
                    existingValues.Add(v);
                }
            }
        }

        /// <summary>
        /// Attempts to parse a single enum member line into a name and optional integer value.
        /// </summary>
        /// <param name="trimmed">A trimmed line taken from inside the enum body.</param>
        /// <param name="name">Output: parsed member name.</param>
        /// <param name="value">Output: parsed integer value if present; otherwise null.</param>
        /// <returns>True if a name was parsed; false otherwise.</returns>
        /// <remarks>
        /// Local variables used:
        /// - nameEnd: index of the last character of the name portion.
        /// - eq: index of '=' which starts the value section.
        /// - start/end: indices delimiting the numeric value substring, allowing for a leading minus sign.
        /// </remarks>
        private bool TryParseEnumMember(string trimmed, out string name, out int? value)
        {
            name = null;
            value = null;
            if (string.IsNullOrEmpty(trimmed)) return false;

            // Name — up to '=', ',', space or tab
            int nameEnd = -1;
            for (int k = 0; k < trimmed.Length; k++)
            {
                char ch = trimmed[k];
                if (ch == '=' || ch == ',' || ch == ' ' || ch == '\t')
                {
                    nameEnd = k - 1;
                    break;
                }
            }

            if (nameEnd >= 0) name = trimmed.Substring(0, nameEnd + 1);
            else name = trimmed.TrimEnd(',');

            // Value to the right of '='
            int eq = trimmed.IndexOf('=');
            if (eq >= 0)
            {
                int start = -1;
                int end = -1;
                for (int i = eq + 1; i < trimmed.Length; i++)
                {
                    char ch = trimmed[i];
                    if (start < 0)
                    {
                        if (ch == '-' || (ch >= '0' && ch <= '9')) start = i;
                    }
                    else
                    {
                        if (!(ch >= '0' && ch <= '9')) { end = i - 1; break; }
                    }
                }
                if (start >= 0 && end < start) end = trimmed.Length - 1;
                if (start >= 0 && end >= start)
                {
                    if (int.TryParse(trimmed.Substring(start, end - start + 1), out int parsed))
                    {
                        value = parsed;
                    }
                }
            }

            return !string.IsNullOrEmpty(name);
        }

        /// <summary>
        /// Builds a list of enum member lines to insert for the provided new names using Hash32 for stable IDs.
        /// </summary>
        /// <param name="newNames">Sanitized names that are not yet present in the enum.</param>
        /// <param name="existingValues">Set of integer values already used in the enum (to detect hash collisions).</param>
        /// <param name="existingNameToValue">Map of existing names to their numeric values (helps identify same-name/same-id cases).</param>
        /// <returns>List of lines formatted as "        Name = Id," to be inserted into the enum body.</returns>
        /// <remarks>
        /// Local variables used:
        /// - toInsert: resulting formatted lines to inject.
        /// - newGeneratedValues: tracks IDs generated in this pass to catch collisions between new items.
        /// - indent: spaces prepended to each new line to match typical formatting.
        /// - name/id: per-item processed values; id is produced by Hash32(name).
        /// Logs errors for collisions with existing members and between new items; logs when duplicates are skipped.
        /// </remarks>
        private List<string> BuildInsertLines(List<string> newNames,
                                              HashSet<int> existingValues,
                                              Dictionary<string, int> existingNameToValue)
        {
            var toInsert = new List<string>(newNames.Count);
            var newGeneratedValues = new HashSet<int>();
            string indent = "        ";

            for (int i = 0; i < newNames.Count; i++)
            {
                string name = newNames[i];
                int id = Hash32(name);

                if (existingValues.Contains(id))
                {
                    if (existingNameToValue.TryGetValue(name, out var same) && same == id)
                    {
                        ShowLog($"[TagEdit] Тег '{name}' уже присутствует c тем же значением — пропущен.", LogType.Log);
                        continue;
                    }

                    string boundName = null;
                    foreach (var kv in existingNameToValue)
                    {
                        if (kv.Value == id) { boundName = kv.Key; break; }
                    }
                    ShowLog($"[TagEdit] Коллизия хеша для тега '{name}'. Вычисленный id {id} уже занят именем '{boundName ?? "<unknown>"}'. Тег пропущен.", LogType.Error);
                    continue;
                }

                if (newGeneratedValues.Contains(id))
                {
                    ShowLog($"[TagEdit] Коллизия хеша между новыми тегами при одном добавлении. Имя '{name}' имеет тот же id {id}, что и другой новый тег. Тег пропущен.", LogType.Error);
                    continue;
                }

                newGeneratedValues.Add(id);
                toInsert.Add(indent + name + " = " + id + ",");
            }

            return toInsert;
        }

        /// <summary>
        /// Inserts the prepared lines right before the enum's closing brace and updates the asset on disk.
        /// </summary>
        /// <param name="path">Full file path of the target enum .cs file.</param>
        /// <param name="allLines">All original lines of the file.</param>
        /// <param name="enumEndBraceLine">Index of the closing brace line for the enum body.</param>
        /// <param name="toInsert">List of formatted members (e.g., "Name = Id,") to add.</param>
        /// <remarks>
        /// Local variables used:
        /// - updated: accumulator list containing the new file content with inserted lines.
        /// - i/k: loop counters used to reconstruct the file and inject new lines at the precise location.
        /// After writing, the method forces Unity to re-import and refresh the asset, then logs the result.
        /// </remarks>
        private void InsertBeforeClosingBrace(string path, string[] allLines, int enumEndBraceLine, List<string> toInsert)
        {
            var updated = new List<string>(allLines.Length + toInsert.Count);
            for (int i = 0; i < allLines.Length; i++)
            {
                if (i == enumEndBraceLine)
                {
                    for (int k = 0; k < toInsert.Count; k++) updated.Add(toInsert[k]);
                }
                updated.Add(allLines[i]);
            }

            File.WriteAllLines(path, updated.ToArray());
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            ShowLog($"[TagEdit] Добавлено новых тегов в enum: {toInsert.Count}. Файл сохранён: {path}", LogType.Log);
        }

        /// <summary>
        /// Computes a stable 32-bit integer hash (FNV-1a) for the provided string.
        /// </summary>
        /// <param name="s">Input string to hash.</param>
        /// <returns>Signed 32-bit integer hash. Negative values are possible and expected.</returns>
        /// <remarks>
        /// Local variables used:
        /// - offset/prime: FNV-1a constants.
        /// - hash: rolling unsigned accumulator updated per character.
        /// - i: loop index through the characters of the input string.
        /// </remarks>
        public static int Hash32(string s)
        {
            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                uint hash = offset;
                for (int i = 0; i < s.Length; i++)
                {
                    hash ^= s[i];
                    hash *= prime;
                }
                return (int)hash; // may be negative — that's expected
            }
        }

        /// <summary>
        /// Processes a single source line to locate and traverse the enum body while collecting its content lines.
        /// </summary>
        /// <param name="line">Current source line.</param>
        /// <param name="enumName">Target enum name to search for.</param>
        /// <param name="enumLines">Collector list where non-comment lines inside the enum body are appended.</param>
        /// <param name="foundEnum">Ref flag: becomes true once the enum declaration line is detected.</param>
        /// <param name="startedBlock">Ref flag: becomes true once the opening brace '{' for the enum body is found.</param>
        /// <param name="braceDepth">Ref counter: tracks current nesting level of braces while traversing.</param>
        /// <returns>True when the enum body is closed and outer parsing loop can stop; otherwise false.</returns>
        /// <remarks>
        /// Local variables used:
        /// - braceIndex: index of '{' on the current line when searching for the body start.
        /// - ch: per-character iteration variable used to update braceDepth.
        /// - trimmed: current line without leading/trailing spaces used to filter out empty lines and comments.
        /// </remarks>
        private bool ProcessEnumLine(string line, string enumName, List<string> enumLines, ref bool foundEnum, ref bool startedBlock, ref int braceDepth)
        {
            if (!foundEnum)
            {
                // Find the enum declaration line
                if (!string.IsNullOrEmpty(enumName) && line.Contains("enum " + enumName))
                {
                    foundEnum = true;

                    // The '{' may be on the same line
                    int braceIndex = line.IndexOf('{');
                    if (braceIndex >= 0)
                    {
                        startedBlock = true;
                        braceDepth = 1;
                                // Continue parsing the body after this line
                    }
                }

                return false; // continue parsing the next lines
            }

            if (!startedBlock)
            {
                // Waiting for the enum body to open
                int braceIndex = line.IndexOf('{');
                if (braceIndex >= 0)
                {
                    startedBlock = true;
                    braceDepth = 1;
                    return false; // proceed to the next line
                }
                else
                {
                    return false; // keep waiting for the opening brace
                }
            }

            // Already inside the enum body. Count brace depth by all characters.
            for (int c = 0; c < line.Length; c++)
            {
                char ch = line[c];
                if (ch == '{')
                {
                    braceDepth++;
                }
                else if (ch == '}')
                {
                    braceDepth--;
                }
            }

            // Before exiting (braceDepth==0), add the previous line if it is content
            if (braceDepth >= 1)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    // Skip comment lines
                    if (!trimmed.StartsWith("//"))
                    {
                        enumLines.Add(trimmed);
                    }
                }

                return false; // remain inside the enum body
            }
            else
            {
                // Closed the enum body — exit the outer for-loop
                return true;
            }
        }

        /// <summary>
        /// Outputs a message to the Unity console with a unified prefix. Supports log, warning and error levels.
        /// </summary>
        /// <param name="message">Text to log.</param>
        /// <param name="type">Log type: Log, Warning or Error.</param>
        private void ShowLog(string message, LogType type = LogType.Log)
        {
            string fullMessage = _logPrefix + " " + message;

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    Debug.LogError(fullMessage);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(fullMessage);
                    break;
                default:
                    Debug.Log(fullMessage);
                    break;
            }
        }
    }
}
#endif
