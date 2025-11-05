using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace PTH
{
    public class AImoment
    {
            static void CA_21(string proj, string mesto)
            {

                string projectPath = proj;


                var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

                var syntaxTrees = new List<SyntaxTree>();
                var sourceFiles = new Dictionary<SyntaxTree, string>();

                foreach (var file in csFiles)
                {
                    string code = File.ReadAllText(file);
                    var tree = CSharpSyntaxTree.ParseText(code);
                    syntaxTrees.Add(tree);
                    sourceFiles[tree] = file;
                }

                var compilation = CSharpCompilation.Create(
                    "ProjectAnalysis",
                    syntaxTrees,
                    new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                    },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

                var md = new StringBuilder("# Документация проекта\n\n");
                var html = new StringBuilder("<html><body><h1>Документация проекта</h1>");
                var xml = new StringBuilder("<documentation>\n");
                var structure = new StringBuilder("СТРУКТУРА ПРОЕКТА:\n\n");

                foreach (var tree in syntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(tree);
                    var root = tree.GetCompilationUnitRoot();

                    foreach (var member in root.Members)
                    {
                        if (member is NamespaceDeclarationSyntax ns)
                        {
                            structure.AppendLine($"Namespace: {ns.Name}");
                            foreach (var classDecl in ns.DescendantNodes().OfType<ClassDeclarationSyntax>())
                            {
                                AnalyzeClass(classDecl, semanticModel, md, html, xml, structure, 1);
                            }
                        }
                        else if (member is ClassDeclarationSyntax classDecl)
                        {
                            structure.AppendLine("Global Namespace:");
                            AnalyzeClass(classDecl, semanticModel, md, html, xml, structure, 1);
                        }
                    }
                }

                html.Append("</body></html>");
                xml.Append("</documentation>");

                File.WriteAllText(mesto + @"\CA_21\Documentation.md", md.ToString());
                File.WriteAllText("Documentation.html", html.ToString());
                File.WriteAllText("Documentation.xml", xml.ToString());
                File.WriteAllText("Structure.txt", structure.ToString());
                // === AIHelp ===

                var aiHelp = new StringBuilder();

                foreach (var tree in syntaxTrees)
                {
                    var root = tree.GetRoot();

                    var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                    foreach (var classDecl in classes)
                    {
                        aiHelp.AppendLine("******");
                        aiHelp.AppendLine(classDecl.NormalizeWhitespace().ToFullString());
                        aiHelp.AppendLine("******\n");
                    }
                }


                File.WriteAllText(mesto + @"\CA_21\AIHelp.txt", aiHelp.ToString());


                Console.WriteLine("Документация и структура успешно созданы.");
            }

            static void AnalyzeClass(
                ClassDeclarationSyntax classDecl,
                SemanticModel model,
                StringBuilder md,
                StringBuilder html,
                StringBuilder xml,
                StringBuilder structure,
                int indentLevel
            )
            {
                var symbol = model.GetDeclaredSymbol(classDecl);
                if (symbol == null) return;

                string name = symbol.ToDisplayString();
                string summary = GetSummaryFromSymbol(symbol);
                string indent = new string(' ', indentLevel * 2);

                // Документация
                md.AppendLine($"## Класс `{name}`\n{summary}\n");
                html.AppendLine($"<h2>Класс {name}</h2><p>{summary}</p>");
                xml.AppendLine($"  <class name=\"{name}\"><summary>{summary}</summary>");

                // Структура
                structure.AppendLine($"{indent}Класс: {name} [{symbol.DeclaredAccessibility}]");

                foreach (var member in classDecl.Members)
                {
                    if (member is MethodDeclarationSyntax method)
                    {
                        var methodSymbol = model.GetDeclaredSymbol(method);
                        if (methodSymbol == null) continue;

                        string methodName = methodSymbol.ToDisplayString();
                        string methodSummary = GetSummaryFromSymbol(methodSymbol);
                        string returnType = methodSymbol.ReturnType.ToDisplayString();

                        md.AppendLine($"- **Метод** `{methodName}`: возвращает `{returnType}`\n  - {methodSummary}");
                        html.AppendLine($"<h3>Метод {methodName}</h3><p>{methodSummary}</p>");
                        xml.AppendLine($"    <method name=\"{methodName}\" returnType=\"{returnType}\"><summary>{methodSummary}</summary></method>");

                        structure.AppendLine($"{indent}  Метод: {methodSymbol.DeclaredAccessibility} {returnType} {methodSymbol.Name}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Type + " " + p.Name))})");
                    }

                    else if (member is PropertyDeclarationSyntax prop)
                    {
                        var propSymbol = model.GetDeclaredSymbol(prop);
                        if (propSymbol == null) continue;

                        string propSummary = GetSummaryFromSymbol(propSymbol);
                        string propType = propSymbol.Type.ToDisplayString();

                        md.AppendLine($"- **Свойство** `{propSymbol.Name}` : `{propType}`\n  - {propSummary}");
                        html.AppendLine($"<h3>Свойство {propSymbol.Name}</h3><p>{propSummary}</p>");
                        xml.AppendLine($"    <property name=\"{propSymbol.Name}\" type=\"{propType}\"><summary>{propSummary}</summary></property>");

                        structure.AppendLine($"{indent}  Свойство: {propSymbol.DeclaredAccessibility} {propType} {propSymbol.Name}");
                    }

                    else if (member is FieldDeclarationSyntax field)
                    {
                        foreach (var variable in field.Declaration.Variables)
                        {
                            var fieldSymbol = model.GetDeclaredSymbol(variable);
                            if (fieldSymbol is IFieldSymbol fs)
                            {
                                structure.AppendLine($"{indent}  Поле: {fs.DeclaredAccessibility} {fs.Type.ToDisplayString()} {fs.Name}");
                            }
                        }
                    }

                    else if (member is EventDeclarationSyntax evt)
                    {
                        var eventSymbol = model.GetDeclaredSymbol(evt);
                        if (eventSymbol is IEventSymbol es)
                        {
                            structure.AppendLine($"{indent}  Событие: {es.DeclaredAccessibility} {es.Type.ToDisplayString()} {es.Name}");
                        }
                    }
                }

                xml.AppendLine("  </class>");
            }

        static string GetSummaryFromSymbol(ISymbol symbol)
        {
            if (symbol == null) return "";
            var xmlDoc = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrWhiteSpace(xmlDoc)) return "";
            try
            {
                var xml = System.Xml.Linq.XElement.Parse(xmlDoc);
                var summaryElement = xml.Element("summary");
                return summaryElement?.Value?.Trim()?.Replace("\n", " ") ?? "";
            }
            catch
            {
                return "";
            }
        }

        int blockNum { get; set; }

        static int CA_22(string mesto)
            {
                string inputFilePath = mesto + @"\CA_21\AIHelp.txt";
                string outputDirectory = mesto + @"\CA_22";

                try
                {
                    Directory.CreateDirectory(outputDirectory); // Создаем папку для блоков
                    string content = File.ReadAllText(inputFilePath);

                    int blockNumber = 1;
                    int startIndex = 0;

                    while (true)
                    {
                        int blockStart = content.IndexOf("******", startIndex);
                        if (blockStart == -1) break;

                        int blockEnd = content.IndexOf("******", blockStart + 6);
                        if (blockEnd == -1) break;

                        // Извлекаем содержимое блока (без маркеров)
                        string blockContent = content.Substring(blockStart + 6, blockEnd - blockStart - 6);

                        // Сохраняем блок в отдельный файл
                        string outputFilePath = Path.Combine(outputDirectory, $"block_{blockNumber}.txt");
                        File.WriteAllText(outputFilePath, blockContent);

                        Console.WriteLine($"Сохранен блок #{blockNumber} в {outputFilePath}");

                        blockNumber++;
                        startIndex = blockEnd + 6;
                    }

                    Console.WriteLine($"Всего сохранено {blockNumber - 1} блоков в папке '{outputDirectory}'");
                return blockNumber;
                     

            }
            catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка: " + ex.Message);
                return 0;
                }
            }

            static async Task CA_24(string mesto, ProgressBar Bar, float blocks)
            {
                string folderPath = mesto + @"\CA_22"; // Папка с файлами block_1, block_2 и т.д.
                string outputFile = mesto + @"\CA_24\all_responses.txt"; // Файл для сохранения всех ответов

                float a = 55 / blocks;
                
                try
                {
                    Console.WriteLine("Начинаем обработку блоков...");

                    // Получаем и сортируем файлы
                    var blockFiles = GetSortedBlockFiles(folderPath);

                    // Очищаем файл перед записью новых ответов
                    File.WriteAllText(outputFile, string.Empty);

                    // Обрабатываем каждый файл последовательно
                    foreach (var block in blockFiles)
                    {
                        string content = File.ReadAllText(block.Path);
                        Console.WriteLine($"Отправляем блок {block.Number}...");

                        string aiResponse = await GetAIResponse(content);
                        Console.WriteLine($"Получен ответ на блок {block.Number}");

                        // Сохраняем ответ в файл
                        File.AppendAllText(outputFile, $"Блок {block.Number}:\n{aiResponse}\n\n", Encoding.UTF8);

                        Bar.Value = 15 + a * block.Number;
                    }

                    Console.WriteLine($"Все ответы сохранены в файл: {Path.GetFullPath(outputFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                Console.WriteLine("Нажмите любую клавишу для выхода...");
                
            }

            static List<BlockFile> GetSortedBlockFiles(string folderPath)
            {
                var files = new List<BlockFile>();

                foreach (string filePath in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileName(filePath);
                    if (TryParseBlockNumber(fileName, out int blockNumber))
                    {
                        files.Add(new BlockFile(blockNumber, filePath));
                    }
                }

                files.Sort((a, b) => a.Number.CompareTo(b.Number));
                return files;
            }

            static bool TryParseBlockNumber(string fileName, out int blockNumber)
            {
                blockNumber = 0;
                const string prefix = "block_";

                if (!fileName.StartsWith(prefix))
                    return false;

                string numberPart = fileName.Substring(prefix.Length);
                int dotIndex = numberPart.IndexOf('.');
                if (dotIndex > 0)
                {
                    numberPart = numberPart.Substring(0, dotIndex);
                }

                return int.TryParse(numberPart, out blockNumber);
            }

            static async Task<string> GetAIResponse(string message)
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(20);

                string apiUrl = "http://127.0.0.1:1234/v1/chat/completions";

                var payload = new
                {
                    model = "codellama-7b-instruct",
                    messages = new[] { new { role = "user", content = message } }
                };

                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                string replyJson = await response.Content.ReadAsStringAsync();
                dynamic replyData = Newtonsoft.Json.JsonConvert.DeserializeObject(replyJson);
                return replyData?.choices?[0]?.message?.content ?? "Нет ответа";
            }

            class BlockFile
            {
                public int Number { get; }
                public string Path { get; }

                public BlockFile(int number, string path)
                {
                    Number = number;
                    Path = path;
                }
            }

            /* public static string MergeDocumentation(string format1, string format2)
             {
                 // Название класса
                 var classNameMatch = Regex.Match(format1, @"Класс `(.+?)`");
                 string className = classNameMatch.Success ? classNameMatch.Groups[1].Value : "UnknownClass";

                 // Назначение класса
                 var purposeMatch = Regex.Match(format2, @"\*\*Назначение класса:\*\*\s*(.+)");
                 string classPurpose = purposeMatch.Success ? purposeMatch.Groups[1].Value.Trim() : "Не указано";

                 var result = new StringBuilder();
                 result.AppendLine($"## Класс `{className}`\n");
                 result.AppendLine($"**Назначение класса:** {classPurpose}\n");

                 // Свойства
                 result.AppendLine("### Свойства");
                 var propertiesSection = Regex.Match(format2, @"#### Свойства\s*(.+?)(?=\n####|\n###|\Z)", RegexOptions.Singleline);
                 if (propertiesSection.Success)
                 {
                     result.AppendLine(propertiesSection.Groups[1].Value.Trim());
                 }
                 else
                 {
                     result.AppendLine("Нет свойств");
                 }

                 result.AppendLine("\n### Методы");

                 // Методы из format1
                 var methodMatches = Regex.Matches(format1, @"\*\*Метод\*\* `(.+?)`:\s*возвращает `(.+?)`");
                 foreach (Match match in methodMatches)
                 {
                     string fullMethodName = match.Groups[1].Value.Trim();                      // bol.acc.LoadUserData()
                     string returnType = match.Groups[2].Value.Trim();                          // void

                     // Извлекаем только короткое имя (после последней точки и до первой скобки)
                     var shortNameMatch = Regex.Match(fullMethodName, @"\.([^.()]+)\(");
                     string shortMethodName = shortNameMatch.Success ? shortNameMatch.Groups[1].Value : fullMethodName;

                     // Ищем описание по короткому имени
                     string pattern = $@"- {Regex.Escape(shortMethodName)}[^\n]*→[^\n]*\n((?:\s*[-–•●]?\s*Шаг.*\n?)*)";
                     var descMatch = Regex.Match(format2, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                     result.AppendLine($"#### `{fullMethodName}` → `{returnType}`");

                     if (descMatch.Success)
                     {
                         string firstLine = Regex.Match(format2, $@"- {Regex.Escape(shortMethodName)}[^\n]*→\s*(.+)").Groups[1].Value.Trim();
                         result.AppendLine(firstLine + "\n");

                         string stepsRaw = descMatch.Groups[1].Value.Trim();
                         if (!string.IsNullOrWhiteSpace(stepsRaw))
                         {
                             result.AppendLine("**Логика работы:**");
                             foreach (var step in Regex.Matches(stepsRaw, @"[-–•●]?\s*Шаг\s*\d+:.*"))
                             {
                                 result.AppendLine($"- {step.ToString().Trim()}");
                             }
                         }
                     }
                     else
                     {
                         result.AppendLine("Описание метода отсутствует");
                     }

                     result.AppendLine();
                 }

                 return result.ToString();
             }*/

            public static string MergeAndGenerateTOC(string file1, string file2)
            {
                var blocks = Regex.Split(file1, @"(?=### |\*\*Название класса:\*\*)")
                                   .Select(b => b.Trim()).Where(b => !string.IsNullOrWhiteSpace(b));

                var classData = new Dictionary<string, (string Purpose, HashSet<string> Properties, HashSet<string> Methods)>();

                foreach (var block in blocks)
                {
                    var classMatch = Regex.Match(block, @"(?:### |\*\*Название класса:\*\* )(?<name>[A-Za-z_][\w\d]*)");
                    if (!classMatch.Success) continue;
                    var className = classMatch.Groups["name"].Value.Trim();

                    var purposeMatch = Regex.Match(block, @"(?:Назначение класса:?\*\*?:?)\s*(?<text>.+?)(\n|$)");
                    var purpose = purposeMatch.Success ? purposeMatch.Groups["text"].Value.Trim() : "";

                    var propMatches = Regex.Matches(block, @"- [`\*]*([\w\d_]+)[`\*]*\s*→\s*(.+?)(?:\n|$)");

                    var methodMatches = Regex.Matches(block, @"- [`\\*]*([\w\d_]+)\\(.*?\\)[`\\*]*\s*→?\s*(.+?)(?:\\n|$)");

                    if (!classData.ContainsKey(className))
                        classData[className] = (purpose, new HashSet<string>(), new HashSet<string>());

                    if (!string.IsNullOrWhiteSpace(purpose))
                        classData[className] = (purpose, classData[className].Properties, classData[className].Methods);

                    foreach (Match prop in propMatches)
                        classData[className].Properties.Add($"- **Свойство** `{prop.Groups[1].Value}` : {prop.Groups[2].Value}");

                    foreach (Match method in methodMatches)
                        classData[className].Methods.Add($"- **Метод** `{method.Groups[1].Value}`: {method.Groups[2].Value}");
                }

                var classSections = Regex.Split(file2, @"(?=## Класс `[""]?[\w\.]+[""]?`)");

                var existingDocs = new Dictionary<string, string>();

                foreach (var section in classSections)
                {
                    var match = Regex.Match(section, @"## Класс `(?:[\w\.]*\.)?([A-Za-z_][\w\d]*)`");

                    if (!match.Success) continue;
                    var name = match.Groups[1].Value;
                    existingDocs[name] = section.Trim();
                }

                var final = new List<string>();
                var toc = new List<string> { "# Оглавление\n" };

                foreach (var kvp in classData.OrderBy(k => k.Key))
                {
                    var className = kvp.Key;
                    var doc = $"## Класс `{className}`\n";

                    if (!string.IsNullOrWhiteSpace(kvp.Value.Purpose))
                        doc += kvp.Value.Purpose + "\n\n";

                    if (kvp.Value.Properties.Count > 0)
                        doc += string.Join("\n", kvp.Value.Properties.OrderBy(p => p)) + "\n\n";

                    if (kvp.Value.Methods.Count > 0)
                        doc += string.Join("\n", kvp.Value.Methods.OrderBy(m => m)) + "\n";

                    if (existingDocs.ContainsKey(className))
                    {
                        var merged = MergeDescriptions(existingDocs[className], doc);
                        final.Add(merged);
                    }
                    else
                    {
                        final.Add(doc);
                    }

                    toc.Add($"- [{className}](#{className.ToLower()})");
                }

                return string.Join("\n\n", toc) + "\n\n" + string.Join("\n\n", final);
            }

            private static string MergeDescriptions(string existing, string updated)
            {
                var existingLines = new HashSet<string>(existing.Split('\n').Select(line => line.Trim()));
                var updatedLines = new HashSet<string>(updated.Split('\n').Select(line => line.Trim()));
                existingLines.UnionWith(updatedLines);
                return string.Join("\n", existingLines.Where(line => !string.IsNullOrWhiteSpace(line)));
            }

            static void CA_23(string mestoCA)
            {
                string format1 = File.ReadAllText(mestoCA + @"\CA_21\Documentation.md");

                string format2 = File.ReadAllText(mestoCA + @"\CA_24\all_responses.txt");


                string merged = MergeDescriptions(format1, format2);
                File.WriteAllText(mestoCA + @"\merged_output.md", merged);
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine(merged);
            }


            public async Task Main(string projCA, string mestoCA, ProgressBar Bar)
            {
                CA_21(projCA, mestoCA); //10%
                Bar.Value = 10;

                
                int blocks = CA_22(mestoCA);
                Bar.Value = 15;//15%

                await CA_24(mestoCA, Bar, blocks); // 70% / количество блоков 
                CA_23(mestoCA); //95
                Bar.Value = 95;
                TimeSpan.FromSeconds(5);
                Bar.Value = 100;
                 //100
            }

            
           /* public void zopa(string projCA, string mestoCA)
            {
            string projCAS = projCA;
            string mestoCAS = mestoCA;
                Main(projCAS, mestoCAS);
            }*/
    }
}
