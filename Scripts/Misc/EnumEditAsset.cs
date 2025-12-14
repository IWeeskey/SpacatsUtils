#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Spacats.Utils
{
    [CreateAssetMenu(menuName = "Spacats/Enum Edit Asset", fileName = "EnumEditAsset")]
    public sealed class EnumEditAsset : ScriptableObject
    {
        // Ссылка на файл .cs с enum, который будем читать/изменять
        [SerializeField]
        public MonoScript  _enumScriptFile;
        
        private string _logPrefix = "[EnumEdit]";

        // Поле со списком строк для добавления в enum
        // Имя оставлено в виде NewTagsToAdd по запросу задачи
        [SerializeField]
        public List<string> NewTagsToAdd = new List<string>();

        // Читает строки внутри enum TagEnum, выводит их в лог и печатает максимальный присвоенный номер
        public void AddToEnum()
        {
            // Временный список строк из енума
            var enumLines = new List<string>();

            if (_enumScriptFile == null)
            {
                Debug.LogError("[TagEdit] Не задан файл скрипта с enum. Укажите _enumScriptFile.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(_enumScriptFile);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[TagEdit] Путь к файлу enum пуст. Укажите корректный скрипт.");
                return;
            }

            if (!File.Exists(path))
            {
                Debug.LogError($"[TagEdit] Файл не найден: {path}");
                return;
            }
            
            // Валидируем введённые теги перед любой обработкой
            ValidateNewTags();

            string[] allLines = File.ReadAllLines(path);
            if (allLines == null || allLines.Length == 0)
            {
                Debug.LogError("[TagEdit] Файл пуст.");
                return;
            }

            // Ищем определение enum по имени из файла и собираем строки внутри его фигурных скобок
            bool foundEnum = false;
            bool startedBlock = false;
            int braceDepth = 0;

            // Имя енума предполагаем как имя файла скрипта (Unity MonoScript.name)
            string enumName = _enumScriptFile != null ? _enumScriptFile.name : null;

            for (int i = 0; i < allLines.Length; i++)
            {
                string line = allLines[i];
                bool shouldBreak = ProcessEnumLine(line, enumName, enumLines, ref foundEnum, ref startedBlock, ref braceDepth);
                if (shouldBreak) break;
            }
            
            // Добавляем отвалидированные теги в enum
            AddValidatedTagsToEnum();
            NewTagsToAdd.Clear();
        }

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

        private void ProcessNewTag(string original, List<string> validated)
        {
            if (original == null)
            {
                Debug.LogWarning("[TagEdit] Найдена null-строка в списке тегов. Пропускаю.");
                return;
            }

            string trimmed = original.Trim();
            string lower = trimmed.ToLowerInvariant();

            // Разрешены символы: a-z, 0-9, _
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

            //Убираем начальные/конечные подчёркивания, если они склеились
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

            // Имя не должно начинаться с цифры — добавляем подчёркивание в начало
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
                Debug.LogWarning($"[TagEdit] В теге \"{original}\" найден некорректный формат. После очистки строка пуста — тег пропущен.");
                return;
            }

            if (fixedStr != original)
            {
                Debug.Log($"[TagEdit] В теге \"{original}\" найден некорректный формат и исправлен на \"{fixedStr}\".");
            }

            validated.Add(fixedStr);
        }

        private void AddValidatedTagsToEnum()
        {
            if (_enumScriptFile == null)
            {
                Debug.LogError("[TagEdit] Не задан файл скрипта с enum. Укажите _enumScriptFile.");
                return;
            }
            if (NewTagsToAdd == null || NewTagsToAdd.Count == 0)
            {
                Debug.Log("[TagEdit] Нет новых тегов для добавления. Список пуст.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(_enumScriptFile);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[TagEdit] Путь к файлу enum пуст. Укажите корректный скрипт.");
                return;
            }
            if (!File.Exists(path))
            {
                Debug.LogError($"[TagEdit] Файл не найден: {path}");
                return;
            }

            string[] allLines = File.ReadAllLines(path);
            if (allLines == null || allLines.Length == 0)
            {
                Debug.LogError("[TagEdit] Файл пуст.");
                return;
            }

            string enumName = _enumScriptFile != null ? _enumScriptFile.name : null;
            if (string.IsNullOrEmpty(enumName))
            {
                Debug.LogError("[TagEdit] Не удалось определить имя enum по файлу скрипта.");
                return;
            }

            // Найти диапазон строк енума
            if (!FindEnumRange(allLines, enumName, out int enumDeclLine, out int enumEndBraceLine))
            {
                Debug.LogError($"[TagEdit] Не удалось найти тело enum {enumName} для редактирования.");
                return;
            }

            // Собрать информацию о существующих элементах енума
            var existingNames = new List<string>();
            var existingNameToValue = new Dictionary<string, int>();
            var existingValues = new HashSet<int>();
            int maxValue = int.MinValue;
            CollectEnumInfo(allLines, enumDeclLine, enumEndBraceLine, existingNames, existingNameToValue, existingValues, ref maxValue);

            // Отфильтровать только новые имена
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
                else Debug.Log($"[TagEdit] Тег '{candidate}' уже существует в enum — пропущен.");
            }
            if (newNames.Count == 0)
            {
                Debug.Log("[TagEdit] Нет новых уникальных тегов для добавления.");
                return;
            }

            var toInsert = BuildInsertLines(newNames, existingValues, existingNameToValue);
            if (toInsert.Count == 0)
            {
                Debug.LogWarning("[TagEdit] Новые теги не добавлены из-за коллизий или дубликатов.");
                return;
            }

            // Вставить строки перед закрывающей скобкой енума
            InsertBeforeClosingBrace(path, allLines, enumEndBraceLine, toInsert);
        }

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

                // Считаем скобки начиная с найденной строки до конца файла
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

                // Если дошли сюда — не нашли закрывающую скобку
                return false;
            }

            return false;
        }

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

        private bool TryParseEnumMember(string trimmed, out string name, out int? value)
        {
            name = null;
            value = null;
            if (string.IsNullOrEmpty(trimmed)) return false;

            // Имя — до '=', ',', пробела или таба
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

            // Значение справа от '='
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
                        Debug.Log($"[TagEdit] Тег '{name}' уже присутствует c тем же значением — пропущен.");
                        continue;
                    }

                    string boundName = null;
                    foreach (var kv in existingNameToValue)
                    {
                        if (kv.Value == id) { boundName = kv.Key; break; }
                    }
                    Debug.LogError($"[TagEdit] Коллизия хеша для тега '{name}'. Вычисленный id {id} уже занят именем '{boundName ?? "<unknown>"}'. Тег пропущен.");
                    continue;
                }

                if (newGeneratedValues.Contains(id))
                {
                    Debug.LogError($"[TagEdit] Коллизия хеша между новыми тегами при одном добавлении. Имя '{name}' имеет тот же id {id}, что и другой новый тег. Тег пропущен.");
                    continue;
                }

                newGeneratedValues.Add(id);
                toInsert.Add(indent + name + " = " + id + ",");
            }

            return toInsert;
        }

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
            Debug.Log($"[TagEdit] Добавлено новых тегов в enum: {toInsert.Count}. Файл сохранён: {path}");
        }

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
                return (int)hash; // может быть отрицательным — это нормально
            }
        }

        private bool ProcessEnumLine(string line, string enumName, List<string> enumLines, ref bool foundEnum, ref bool startedBlock, ref int braceDepth)
        {
            if (!foundEnum)
            {
                // Ищем строку с объявлением енума
                if (!string.IsNullOrEmpty(enumName) && line.Contains("enum " + enumName))
                {
                    foundEnum = true;

                    // Может быть, что "{" на той же строке
                    int braceIndex = line.IndexOf('{');
                    if (braceIndex >= 0)
                    {
                        startedBlock = true;
                        braceDepth = 1;
                        // Всё после этой строки продолжим разбирать как тело
                    }
                }

                return false; // продолжаем разбор следующих строк
            }

            if (!startedBlock)
            {
                // Ждём открытия тела енума
                int braceIndex = line.IndexOf('{');
                if (braceIndex >= 0)
                {
                    startedBlock = true;
                    braceDepth = 1;
                    return false; // переходим к следующей строке
                }
                else
                {
                    return false; // продолжаем ожидать открывающей скобки
                }
            }

            // Уже внутри тела енума. Считаем глубину по всем символам.
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

            // Перед тем как выйти (braceDepth==0), добавим предыдущую строку, если это содержимое
            if (braceDepth >= 1)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    // Пропускаем строки-комментарии
                    if (!trimmed.StartsWith("//"))
                    {
                        enumLines.Add(trimmed);
                    }
                }

                return false; // остаёмся внутри тела енума
            }
            else
            {
                // Закрыли тело енума — выходим из цикла внешнего for
                return true;
            }
        }

        private void ShowLog(string message, bool isError = false)
        {
            string fullMessage = _logPrefix + " " + message;
            
            if (isError) Debug.LogError(fullMessage);
            else Debug.Log(fullMessage);
        }
    }
}
#endif
