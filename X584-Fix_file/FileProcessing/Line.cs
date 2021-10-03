using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace X584_Fix_file
{
    public class Line
    {
        /// <summary>
        /// Номер строки в эмуляторе X584.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Текст микроинструкции Х584.
        /// </summary>
        public string MicroInstruction { get; set; }

        /// <summary>
        /// Двоичное представление микроинструкции Х584.
        /// </summary>
        public List<byte> MicroInstructionBin { get; set; }

        /// <summary>
        /// Комментарий в Х584.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Строки, связанные переходами с данной строки (на пример: идти_на Y).
        /// </summary>
        public List<Line> References { get; }

        /// <summary>
        /// Проверка, пуста ли строка (строка считается пустой, если в ней отсуствуют МИ и
        /// комментарий, интерпритируемый эмулятором Х584).
        /// </summary>
        /// <param name="isBin">Тип текущего файла (Бинарный или текстовый).</param>
        /// <returns>Строка пуста.</returns>
        public bool IsEmpty(bool isBin)
        {
            Regex patternEmptyCommand = new Regex(@"<ПУСТО>");
            Regex patternIf = new Regex(@"^Если", RegexOptions.IgnoreCase);
            Regex patternGoTo = new Regex(@"^идти_на", RegexOptions.IgnoreCase);

            if (isBin)
            {
                if ((!patternIf.IsMatch(Comment) && !patternGoTo.IsMatch(Comment) && (MicroInstructionBin[0] == 0x9a && MicroInstructionBin[1] == 0x00)) ||
                    (!patternIf.IsMatch(Comment) && !patternGoTo.IsMatch(Comment) && (MicroInstructionBin[0] == 0x88 && MicroInstructionBin[1] == 0x20)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (!patternIf.IsMatch(Comment) && !patternGoTo.IsMatch(Comment) && patternEmptyCommand.IsMatch(MicroInstruction))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public void ReplaceEmptyMicroInstruction(bool isBin)
        {
            if (isBin)
            {
                if (MicroInstructionBin[0] == 0x9a && MicroInstructionBin[1] == 0x00)
                {
                    MicroInstructionBin = new List<byte>
                    {
                        0x88, 0x20
                    };
                }
            }
            else
            {
                if (MicroInstruction == "<ПУСТО>")
                {
                    MicroInstruction = "РР := РР + П (П=0)";
                }
            }
        }

        /// <summary>
        /// Модель строки эмулятора Х584.
        /// </summary>
        /// <param name="number">Номер строки в эмуляторе Х584 (Используется только при работе с текстовым файлом и
        /// нужен только для нумерациии строк в результирующем текстовом файле).</param>
        /// <param name="microInstruction">Текст микроинструкции Х584 (Используется только при работе с текстовым файлом).</param>
        /// <param name="comment">Текстовый комментарий.</param>
        /// <param name="microInstruction1">Первый байт индентификатора микроинструкции.</param>
        /// <param name="microInstruction2">Второй байт индентификатора микроинструкции.</param>
        public Line(int number = -1, string microInstruction = "<ПУСТО>", string comment = "", byte microInstruction1 = 0x9a, byte microInstruction2 = 0x00)
        {
            Number = number;
            MicroInstruction = microInstruction;
            Comment = comment;
            References = new List<Line>();
            MicroInstructionBin = new List<byte>
            {
                microInstruction1,
                microInstruction2
            };
        }

        /// <summary>
        /// Исправляет в комментарии "иди_на" на "идти_на".
        /// </summary>
        public void FixGoTo()
        {
            Regex badCommandIf = new Regex(@"^иди_на", RegexOptions.IgnoreCase);
            Comment = badCommandIf.Replace(Comment, "идти_на");
        }

        /// <summary>
        /// Замена старых номеров строк в комментарии на новые, которые были получены в ходе преобразования исходного файла.
        /// </summary>
        public void ReplaceNumberInComment()
        {
            switch (References.Count)
            {
                case 1:
                    Comment = string.Format(Comment, References[0].Number);
                    break;
                case 2:
                    Comment = string.Format(Comment, References[0].Number, References[1].Number);
                    break;
            }
        }

        /// <summary>
        /// Возвращает строковое представление модели строки эмулятора Х584.
        /// </summary>
        /// <returns>Строка с номером, МИ и комментарием.</returns>
        public override string ToString()
        {
            return $"{Number}.\t{MicroInstruction}\t{Comment}";
        }

        /// <summary>
        /// Возвращает массив байт, характеризующий строку эмулятора Х584.
        /// </summary>
        /// <returns>Индентификатор МИ и комментарий в байтовом виде.</returns>
        public byte[] ToBytes()
        {
            if (Comment.Length > 0)
            {
                byte[] comment = Encoding.GetEncoding(1251).GetBytes(Comment);
                byte[] result = new byte[2 + 1 + comment.Length];
                MicroInstructionBin.CopyTo(result);
                result[2] = (byte)comment.Length;
                comment.CopyTo(result, 3);

                return result;
            }
            else
            {
                byte[] result = new byte[2 + 1];
                MicroInstructionBin.CopyTo(result);
                result[2] = 0x00;

                return result;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Line line &&
                   MicroInstruction == line.MicroInstruction &&
                   MicroInstructionBin.SequenceEqual(line.MicroInstructionBin) &&
                   Comment == line.Comment;
        }

        public override int GetHashCode()
        {
            var hashCode = 1769111065;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MicroInstruction);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<byte>>.Default.GetHashCode(MicroInstructionBin);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Comment);
            return hashCode;
        }
    }
}
