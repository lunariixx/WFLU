using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WFLU
{
    public class Parser
    {
        public Dictionary<string, Message> ParseFile(string path)
        {
            var result = new Dictionary<string, Message>();
            var lines = File.ReadAllLines(path);
            string? currentKey = null;
            bool inChoiceBlock = false;
            var choiceLines = new List<string>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                var match = Regex.Match(line, @"^(.+?)\s*=\s*(.+)$");
                if (match.Success && !inChoiceBlock)
                {
                    currentKey = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();

                    if (value.EndsWith("->"))
                    {
                        inChoiceBlock = true;
                        choiceLines.Clear();
                    }
                    else if (currentKey is not null)
                    {
                        result[currentKey] = new Message { RawValue = value };
                    }

                    continue;
                }

                if (inChoiceBlock)
                {
                    if (line.StartsWith("}"))
                    {
                        inChoiceBlock = false;

                        if (currentKey is not null)
                        {
                            var message = new Message
                            {
                                IsChoice = true,
                                Choices = new Dictionary<string, string>(),
                            };

                            foreach (var l in choiceLines)
                            {
                                var trimmed = l.Trim();
                                var choiceMatch = Regex.Match(trimmed, @"^\[([^\]]+)\]\s*(.+)$");

                                if (choiceMatch.Success)
                                {
                                    message.Choices[choiceMatch.Groups[1].Value.Trim()] =
                                        choiceMatch.Groups[2].Value.Trim();
                                }
                                else if (trimmed.StartsWith("*[other]"))
                                {
                                    message.Choices["other"] = trimmed
                                        .Substring("*[other]".Length)
                                        .Trim();
                                }
                            }

                            result[currentKey] = message;
                        }

                        continue;
                    }

                    choiceLines.Add(line);
                }
            }

            return result;
        }
    }
}
