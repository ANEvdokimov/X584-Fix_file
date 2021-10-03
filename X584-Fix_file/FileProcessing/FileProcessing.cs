using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using X584_Fix_file.SimpleHelpers;

namespace X584_Fix_file
{
    /// <summary>
    /// Перечисления режимов обработки файлов.
    /// </summary>
    public enum RefactoringMode : uint
    {
        SwapLines,
        InsertEmptyLinesToBeginning,
        InsertEmptyLines,
        InsertEmptyLinesToEnd,
        FullMode,
        FixGoTo,
        FixCommentsWithIf,
        ReplaceNumbersInComments,
        FullModeWithoutInsertEmptyLines,
        ModeForCombine
    }

    /// <summary>
    /// Обработчик текстовых и бинарных файлов программы Х584.
    /// </summary>
    public class FileProcessing
    {
        /// <summary>
        /// Исходный файл.
        /// </summary>
        private FileStream file;

        /// <summary>
        /// Название исходного файла.
        /// </summary>
        public string FileName
        {
            get => file.Name;
        }

        /// <summary>
        /// Список строк эмулятора Х584.
        /// </summary>
        private List<Line> resultListLines;

        /// <summary>
        /// Кодировка исходного текстового файла.
        /// </summary>
        private Encoding encoding;

        /// <summary>
        /// Сдвиг относительно начала в результирующем файле.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// Номера списка строк, в которые нужно вставить пустые строки при форматировании.
        /// </summary>
        private List<int> emptyLines;

        /// <summary>
        /// Тип активного файла (бинарный или текстовый).
        /// </summary>
        private bool IsBin { get; }

        private bool EmptyLineMode { get; }

        /// <summary>
        /// Обработчик текстовых или бинарных файлов.
        /// </summary>
        /// <param name="fileName">Имя исходного файла.</param>
        /// <param name="emptyLines">Номера списка строк, в которые нужно вставить пустые строки при форматировании.</param>
        /// <param name="offset">Сдвиг относительно начала.</param>
        public FileProcessing(string fileName, List<int> emptyLines, int offset = 0, bool emptyLineMode = false)
        {
            resultListLines = new List<Line>();
            this.emptyLines = emptyLines;
            this.offset = offset;
            EmptyLineMode = emptyLineMode;

            file = new FileStream(fileName, FileMode.Open);

            Regex fileType = new Regex(".txt$", RegexOptions.IgnoreCase);
            if (fileType.IsMatch(fileName))
            {
                IsBin = false;
                StreamReader textFile = OpenTextFile();
                ReadLines(textFile);
            }
            else
            {
                IsBin = true;
                byte[] text = OpenBinFile();
                ParseBinText(text);
            }
        }

        /// <summary>
        /// Обработчик текстовых или бинарных файлов.
        /// </summary>
        /// <param name="file">Исходный файл.</param>
        public FileProcessing(FileStream file)
        {
            emptyLines = new List<int>();

            resultListLines = new List<Line>();
            this.file = file;

            Regex fileType = new Regex(".txt$", RegexOptions.IgnoreCase);
            if (fileType.IsMatch(file.Name))
            {
                IsBin = false;
                StreamReader textFile = OpenTextFile();
                ReadLines(textFile);
            }
            else
            {
                IsBin = true;
                byte[] text = OpenBinFile();
                ParseBinText(text);
            }
        }

        /// <summary>
        /// Обработчик текстовых или бинарных файлов.
        /// </summary>
        /// <param name="resultListLines">Список строк, используемых в эмуляторе Х584.</param>
        /// <param name="isBin">Тип файла (бинарный или текстовый).</param>
        /// <param name="encoding">Кодировка файла.</param>
        /// <param name="emptyLineMode">Замена <ПУСТО> на РР=РР+П</ПУСТО></param>
        public FileProcessing(List<Line> resultListLines, bool isBin, Encoding encoding, bool emptyLineMode = false)
        {
            emptyLines = new List<int>();
            EmptyLineMode = emptyLineMode;

            this.encoding = encoding;
            this.resultListLines = resultListLines;
            IsBin = isBin;
        }

        /// <summary>
        /// Обработчик текстовых или бинарных файлов.
        /// </summary>
        /// <param name="resultListLines">Список строк, используемых в эмуляторе Х584.</param>
        /// <param name="isBin">Тип файла (бинарный или текстовый).</param>
        public FileProcessing(List<Line> resultListLines, bool isBin, bool emptyLineMode = false)
        {
            emptyLines = new List<int>();
            EmptyLineMode = emptyLineMode;

            encoding = Encoding.Default;
            this.resultListLines = resultListLines;
            IsBin = isBin;
        }

        /// <summary>
        /// Открыть бинарный файл.
        /// </summary>
        /// <returns>Массив байт исходного файла.</returns>
        private byte[] OpenBinFile()
        {
            BinaryReader reader = new BinaryReader(file);
            byte[] byteArray = reader.ReadBytes((int)file.Length);
            file.Close();
            return byteArray;
        }

        /// <summary>
        /// Разобрать содержимое исходного бинарного файла по строкам эмулятора Х584.
        /// </summary>
        /// <param name="text">Массив байт исходного файла.</param>
        private void ParseBinText(byte[] text)
        {
            string prefix = Encoding.GetEncoding(1251).GetString(text, 0, 4);
            if (prefix != "X584")
            {
                throw new Exception("Бинарный файл неверного формата.");
            }

            for (int index = 4; index < text.Length;)
            {
                byte microInstruction1 = text[index++];
                byte microInstruction2 = text[index++];

                int length = text[index++];

                string comment = Encoding.GetEncoding(1251).GetString(text, index, length);
                index += length;

                Line line = new Line(-1, "<ПУСТО>", comment, microInstruction1, microInstruction2);
                resultListLines.Add(line);
            }
        }

        /// <summary>
        /// Открытие исходного текстового файла (Автоматический подбор кодировки).
        /// </summary>
        /// <returns>Открытый файл.</returns>
        private StreamReader OpenTextFile()
        {
            encoding = GetEncoding(file.Name);
            return new StreamReader(file, encoding);
        }

        /// <summary>
        /// Получить кодировку текстового файла.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Кодировка файла.</returns>
        public static Encoding GetEncoding(string fileName)
        {
            Encoding encoding = FileEncoding.DetectFileEncoding(fileName);

            //костыль
            if (encoding.HeaderName == "koi8-r")
            {
                encoding = Encoding.Default;
            }
            //end костыль

            return encoding;
        }

        /// <summary>
        /// Разбор содержимого исходного текстового файла на строки.
        /// </summary>
        /// <param name="textFile">Исходный текстовый файл.</param>
        /// <returns>Список строк эмулятора Х584.</returns>
        private List<Line> ReadLines(StreamReader textFile)
        {
            string lineString;

            while (!textFile.EndOfStream)
            {
                lineString = textFile.ReadLine();

                string[] lines = lineString.Split('\t');

                int number = int.Parse(Regex.Replace(lines[0], @".$", string.Empty).ToString());

                resultListLines.Add(new Line(number, lines[1], lines[2]));
            }

            textFile.Close();
            file.Close();

            return resultListLines;
        }

        public List<Line> RefactoringFile(RefactoringMode mode) => RefactoringFile(resultListLines, mode);

        /// <summary>
        /// Форматирование файла.
        /// </summary>
        /// <param name="mode">Режим обработки.</param>
        /// <returns>Список строк эмулятора Х584.</returns>
        public List<Line> RefactoringFile(List<Line> listLines, RefactoringMode mode)
        {
            switch (mode)
            {
                case RefactoringMode.FixGoTo:
                    FixGoToInComments(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.SwapLines:
                    ParseComments(listLines);
                    SwapLines(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.InsertEmptyLines:
                    if (resultListLines == null)
                    {
                        ParseComments(listLines);
                        SwapLines(listLines);
                    }
                    InsertEmptyLines(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.InsertEmptyLinesToBeginning:
                    if (resultListLines == null)
                    {
                        ParseComments(listLines);
                        SwapLines(listLines);
                    }
                    InsertEmptyLinesToBeginning(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.InsertEmptyLinesToEnd:
                    if (resultListLines == null)
                    {
                        ParseComments(listLines);
                        SwapLines(listLines);
                    }
                    InsertEmptyLinesToEnd(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.FixCommentsWithIf:
                    FixCommentsWithIf(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.FullMode:
                    FixGoToInComments(listLines);
                    ParseComments(listLines);
                    SwapLines(listLines);
                    InsertEmptyLinesToBeginning(listLines);
                    FixCommentsWithIf(listLines);
                    InsertEmptyLines(listLines);
                    InsertEmptyLinesToEnd(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.FullModeWithoutInsertEmptyLines:
                    FixGoToInComments(listLines);
                    ParseComments(listLines);
                    SwapLines(listLines);
                    FixCommentsWithIf(listLines);
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
                case RefactoringMode.ModeForCombine:
                    FixGoToInComments(listLines);
                    ParseComments(listLines);
                    SwapLines(listLines);
                    FixCommentsWithIf(listLines);
                    break;
                default:
                case RefactoringMode.ReplaceNumbersInComments:
                    ReassignNumbers(listLines);
                    ReplaceNumbersInComment(listLines);
                    break;
            }

            if (EmptyLineMode)
            {
                ReplaceEmpty(listLines);
            }


            return resultListLines;
        }

        private void ReplaceEmpty(List<Line> listLines)
        {
            foreach (var line in listLines)
            {
                line.ReplaceEmptyMicroInstruction(IsBin);
            }
        }

        private void FixCommentsWithIf(List<Line> listLines)
        {
            Regex commandIf = new Regex(@"^Если", RegexOptions.IgnoreCase);

            for (int index = 0; index < listLines.Count; index++)
            {
                if (commandIf.IsMatch(listLines[index].Comment))
                {
                    try
                    {
                        if (!((listLines[index].References[0] == listLines[index + 1]) && !commandIf.IsMatch(listLines[index + 1].Comment)))
                        {
                            InsertFirstBufferLineAfterIf(listLines, index);
                        }
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        InsertFirstBufferLineAfterIf(listLines, index);
                    }

                    try
                    {
                        if (!(listLines[index].References[1] == listLines[index + 2]))
                        {
                            InsertSecondBufferLineAfterIf(listLines, index);
                        }
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                            InsertSecondBufferLineAfterIf(listLines, index);
                    }
                }
            }
        }

        private void InsertFirstBufferLineAfterIf(List<Line> listLines, int index)
        {
            Line bufferLine = new Line(-1, "<ПУСТО>", "идти_на {0}");
            bufferLine.References.Add(listLines[index].References[0]);
            listLines.Insert(index + 1, bufferLine);

            listLines[index].References[0] = bufferLine;
        }

        private void InsertSecondBufferLineAfterIf(List<Line> listLines, int index)
        {
            Regex connandGoTo = new Regex(@"^идти_на", RegexOptions.IgnoreCase);

            if (!connandGoTo.IsMatch(listLines[index + 1].Comment))
            {
                listLines[index + 1].Comment = "идти_на {0}";
                listLines[index + 1].References.Add(listLines[index + 2]);
            }
            Line bufferLine = new Line(-1, "<ПУСТО>", "идти_на {0}");
            bufferLine.References.Add(listLines[index].References[1]);
            listLines.Insert(index + 2, bufferLine);

            listLines[index].References[1] = bufferLine;
        }

        private void ReplaceNumbersInComment(List<Line> listLines)
        {
            foreach (var line in listLines)
            {
                line.ReplaceNumberInComment();
            }
        }

        /// <summary>
        /// Вызавает метод исправления комментария для каждой строки.
        /// </summary>
        private void FixGoToInComments(List<Line> listLines)
        {
            foreach (var line in listLines)
            {
                line.FixGoTo();
            }
        }

        /// <summary>
        /// Вставка количества, равного отступу от начала, строк в начало итогового списка.
        /// </summary>
        private void InsertEmptyLinesToBeginning(List<Line> listLines)
        {
            if (listLines.Count + offset > 1024)
            {
                throw new Exception("В итоговом файле больше 1024 строк");
            }
            for (int i = 0; i < offset; i++)
            {
                listLines.Insert(0, new Line());
            }
        }

        /// <summary>
        /// Вставка строк в конец итогового списка (до 1024).
        /// </summary>
        private void InsertEmptyLinesToEnd(List<Line> listLines)
        {
            if (listLines.Count > 1024)
            {
                throw new Exception("В итоговом файле больше 1024 строк");
            }

            for (int i = listLines.Count; i < 1024; i++)
            {
                listLines.Add(new Line());
            }
        }

        /// <summary>
        /// Разбор текстового комментария на команды эмулятора Х584.
        /// </summary>
        private void ParseComments(List<Line> listLines)
        {
            Regex number = new Regex(@"\s\d+");
            Regex commandGoTo = new Regex(@"^идти_на", RegexOptions.IgnoreCase);
            Regex commandIf = new Regex(@"^Если", RegexOptions.IgnoreCase);

            foreach (Line line in listLines)
            {
                if (commandGoTo.IsMatch(line.Comment) || commandIf.IsMatch(line.Comment))
                {
                    line.References.Clear();
                    MatchCollection matchCollection = number.Matches(line.Comment);

                    for (int j = 0; j < matchCollection.Count; j++)
                    {
                        try
                        {
                            int i = int.Parse(matchCollection[j].Value);
                            for (; i < 1024; i++)
                            {
                                if (!listLines[i].IsEmpty(IsBin))
                                {
                                    line.Comment = line.Comment.Replace(matchCollection[j].Value, " {" + j + "}");
                                    line.References.Add(listLines[i]);
                                    break;
                                }
                            }

                            if (i == 1024)
                            {
                                line.Comment = "";
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            line.Comment = "";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Удаление пустых строк (строка считается пустой, если в ней отсуствуют МИ и
        /// комментарий, интерпритируемый эмулятором Х584).
        /// </summary>
        private void SwapLines(List<Line> listLines)
        {
            for (int index = 0; index < listLines.Count;)
            {
                if (listLines[index].IsEmpty(IsBin))
                {
                    listLines.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        /// <summary>
        /// Вставка пустых строк на заданные номера в итоговом списке.
        /// </summary>
        private void InsertEmptyLines(List<Line> listLines)
        {
            emptyLines = emptyLines.Distinct().ToList();
            emptyLines.Sort();

            foreach (var emptyLine in emptyLines)
            {
                if (emptyLine > 0)
                {
                    if (!listLines[emptyLine - 1].IsEmpty(IsBin))
                    {
                        AddReferences(emptyLine - 1);
                    }
                }

                listLines.Insert(emptyLine, new Line());
            }
        }

        /// <summary>
        /// Добавление ссылки на строку, связанную с текущей. 
        /// </summary>
        /// <param name="index">Номер текущей строки.</param>
        private void AddReferences(int index)
        {
            if (index != -1)
            {
                Regex patternIf = new Regex(@"^Если", RegexOptions.IgnoreCase);
                Regex patternGoTo = new Regex(@"^идти_на", RegexOptions.IgnoreCase);

                if (!patternGoTo.IsMatch(resultListLines[index].Comment) && !patternIf.IsMatch(resultListLines[index].Comment))
                {
                    resultListLines[index].Comment = "идти_на {0}";
                    resultListLines[index].References.Add(resultListLines[index + 1]);
                }
            }
        }

        /// <summary>
        /// Сохранение результата форматирования в файл.
        /// </summary>
        /// <param name="fileName">Имя файла назначения.</param>
        public void Save(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Create);

            if (IsBin)
            {
                byte[] prefix = new byte[] { 0x0058, 0x0035, 0x0038, 0x0034 }; //x584
                fileStream.Write(prefix, 0, prefix.Length);

                foreach (Line line in resultListLines)
                {
                    byte[] lineBytes = line.ToBytes(); // Если убрать строку, то крашит (хз)
                    fileStream.Write(lineBytes, 0, lineBytes.Length);
                }
            }
            else
            {
                using (StreamWriter file = new StreamWriter(fileStream, encoding))
                {
                    foreach (Line line in resultListLines)
                    {
                        file.WriteLine(line.ToString());
                    }
                }
            }

            fileStream.Close();
        }

        /// <summary>
        /// Присвоение номеров строкам (Номер используется только при работе с текстовым файлом и
        /// нужен только для нумерациии строк в результирующем текстовом файле).
        /// </summary>
        private void ReassignNumbers(List<Line> listLines)
        {
            for (int index = 0; index < listLines.Count; index++)
            {
                listLines[index].Number = index;
            }
        }

        /// <summary>
        /// Закрытие исходного файла.
        /// </summary>
        public void CloseFile()
        {
            file.Close();
        }
    }
}
