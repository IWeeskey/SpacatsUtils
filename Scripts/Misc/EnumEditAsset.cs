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
            // Валидируем введённые теги перед любой обработкой
            ValidateNewTags();

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
            // Используем уже отвалидированный список _newTagsToAdd
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

            // Находим диапазон строк енума: индекс начала тела после '{' и индекс строки с закрывающей '}'
            bool foundEnum = false;
            int braceDepth = 0;
            int enumStartBodyLine = -1; // первая строка после строки с '{' (может быть на той же строке)
            int enumEndBraceLine = -1;  // строка с закрывающей '}' для енума

            // Имя енума предполагаем как имя файла скрипта (Unity MonoScript.name)
            string enumName = _enumScriptFile != null ? _enumScriptFile.name : null;

            // Определяем базовый отступ для элементов енума
            string indent = "        "; // 8 пробелов по текущему стилю файла

            // Собираем существующие имена и максимальное значение
            var existingNames = new List<string>();
            var existingNameToValue = new Dictionary<string, int>();
            var existingValues = new HashSet<int>();
            int maxValue = int.MinValue;

            for (int i = 0; i < allLines.Length; i++)
            {
                string line = allLines[i];

                if (!foundEnum)
                {
                    if (!string.IsNullOrEmpty(enumName) && line.Contains("enum " + enumName))
                    {
                        foundEnum = true;
                        // Считаем фигурные скобки, включая текущую строку
                        for (int c = 0; c < line.Length; c++)
                        {
                            char ch = line[c];
                            if (ch == '{')
                            {
                                braceDepth++;
                                if (enumStartBodyLine < 0)
                                {
                                    enumStartBodyLine = i + 1; // содержимое начинается со следующей строки
                                }
                            }
                            else if (ch == '}')
                            {
                                braceDepth--;
                                if (braceDepth == 0)
                                {
                                    enumEndBraceLine = i;
                                    break;
                                }
                            }
                        }
                    }

                    continue;
                }

                // После обнаружения енума, двигаемся по строкам, отслеживая глубину
                for (int c = 0; c < line.Length; c++)
                {
                    char ch = line[c];
                    if (ch == '{')
                    {
                        braceDepth++;
                        if (enumStartBodyLine < 0)
                        {
                            enumStartBodyLine = i + 1;
                        }
                    }
                    else if (ch == '}')
                    {
                        braceDepth--;
                    }
                }

                if (braceDepth >= 1)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        if (!trimmed.StartsWith("//"))
                        {
                            // Парсим имя до '=' или до запятой
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

                            string name;
                            if (nameEnd >= 0)
                            {
                                name = trimmed.Substring(0, nameEnd + 1);
                            }
                            else
                            {
                                // Строка вида "value" без '=' и без запятой на конце
                                name = trimmed.TrimEnd(',');
                            }

                            if (!string.IsNullOrEmpty(name))
                            {
                                existingNames.Add(name);
                            }

                            // Ищем максимальное присвоенное число
                            int eq = trimmed.IndexOf('=');
                            if (eq >= 0)
                            {
                                int numberStart = -1;
                                int numberEnd = -1;
                                for (int k = eq + 1; k < trimmed.Length; k++)
                                {
                                    char tch = trimmed[k];
                                    if (numberStart < 0)
                                    {
                                        if (tch == '-' || (tch >= '0' && tch <= '9'))
                                        {
                                            numberStart = k;
                                        }
                                    }
                                    else
                                    {
                                        if (!(tch >= '0' && tch <= '9'))
                                        {
                                            numberEnd = k - 1;
                                            break;
                                        }
                                    }
                                }

                                if (numberStart >= 0 && numberEnd < numberStart)
                                {
                                    numberEnd = trimmed.Length - 1;
                                }

                                if (numberStart >= 0 && numberEnd >= numberStart)
                                {
                                    string numStr = trimmed.Substring(numberEnd >= numberStart ? numberStart : numberStart, numberEnd - numberStart + 1);
                                    int parsed;
                                    if (int.TryParse(numStr, out parsed))
                                    {
                                        if (parsed > maxValue)
                                        {
                                            maxValue = parsed;
                                        }

                                        // Сохраняем маппинг имя->значение и множество существующих значений
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            if (!existingNameToValue.ContainsKey(name))
                                            {
                                                existingNameToValue.Add(name, parsed);
                                            }
                                            else
                                            {
                                                existingNameToValue[name] = parsed;
                                            }
                                        }

                                        if (!existingValues.Contains(parsed))
                                        {
                                            existingValues.Add(parsed);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    enumEndBraceLine = i;
                    break;
                }
            }

            if (!foundEnum || enumStartBodyLine < 0 || enumEndBraceLine < 0)
            {
                Debug.LogError($"[TagEdit] Не удалось найти тело enum {enumName ?? "<unknown>"} для редактирования.");
                return;
            }

            // Формируем список новых имён, которых нет в текущем енума
            var newNames = new List<string>();
            for (int i = 0; i < NewTagsToAdd.Count; i++)
            {
                string candidate = NewTagsToAdd[i];
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                bool exists = false;
                for (int j = 0; j < existingNames.Count; j++)
                {
                    if (existingNames[j] == candidate)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    newNames.Add(candidate);
                }
                else
                {
                    Debug.Log($"[TagEdit] Тег '{candidate}' уже существует в enum — пропущен.");
                }
            }

            if (newNames.Count == 0)
            {
                Debug.Log("[TagEdit] Нет новых уникальных тегов для добавления.");
                return;
            }

            // Генерируем значения через стабильный хеш от имени и проверяем коллизии
            var toInsert = new List<string>(newNames.Count);
            var newGeneratedValues = new HashSet<int>();
            for (int i = 0; i < newNames.Count; i++)
            {
                string name = newNames[i];
                int id = Hash32(name);

                // Проверка коллизии с уже существующими значениями
                if (existingValues.Contains(id))
                {
                    // Если такой же id уже закреплён за этим же именем — значит тег уже был, но мы сюда не попали, т.к. newNames не содержит существующие.
                    // Следовательно, это коллизия с другим именем — пропускаем и логируем ошибку.
                    string boundName;
                    if (existingNameToValue.TryGetValue(name, out var existingForSameName) && existingForSameName == id)
                    {
                        Debug.Log($"[TagEdit] Тег '{name}' уже присутствует c тем же значением — пропущен.");
                        continue;
                    }

                    // Найдём имя, которому принадлежит этот id (для сообщения), если получится
                    boundName = null;
                    foreach (var kv in existingNameToValue)
                    {
                        if (kv.Value == id)
                        {
                            boundName = kv.Key;
                            break;
                        }
                    }

                    Debug.LogError($"[TagEdit] Коллизия хеша для тега '{name}'. Вычисленный id {id} уже занят именем '{boundName ?? "<unknown>"}'. Тег пропущен.");
                    continue;
                }

                // Проверка коллизии среди новых генерируемых тегов в рамках одного добавления
                if (newGeneratedValues.Contains(id))
                {
                    Debug.LogError($"[TagEdit] Коллизия хеша между новыми тегами при одном добавлении. Имя '{name}' имеет тот же id {id}, что и другой новый тег. Тег пропущен.");
                    continue;
                }

                newGeneratedValues.Add(id);
                string line = indent + name + " = " + id + ",";
                toInsert.Add(line);
            }

            if (toInsert.Count == 0)
            {
                Debug.LogWarning("[TagEdit] Новые теги не добавлены из-за коллизий или дубликатов.");
                return;
            }

            // Вставляем перед строкой с закрывающей скобкой енума
            var updated = new List<string>(allLines.Length + toInsert.Count);
            for (int i = 0; i < allLines.Length; i++)
            {
                if (i == enumEndBraceLine)
                {
                    for (int k = 0; k < toInsert.Count; k++)
                    {
                        updated.Add(toInsert[k]);
                    }
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
