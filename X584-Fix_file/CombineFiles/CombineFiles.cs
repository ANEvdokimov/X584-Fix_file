using System.Collections.Generic;
using System.IO;

namespace X584_Fix_file
{
    /// <summary>
    /// Перечисление режимов совмежения файлов.
    /// </summary>
    enum CombineMode : uint
    {
        /// <summary>
        /// Подстановка к концу предыдущего файла (С учетом пустых строк).
        /// </summary>
        InEndOfPrevious,
        /// <summary>
        /// Подстановка поверх предыдущего файла.
        /// </summary>
        OverPrevious
    }

    /// <summary>
    /// Класс совмещения файлов.
    /// </summary>
    class CombineFiles
    {
        /// <summary>
        /// Список обработчиков файлов.
        /// </summary>
        private List<FileProcessing> listFileProcessing;

        /// <summary>
        /// Режим совмещения.
        /// </summary>
        private CombineMode mode;

        /// <summary>
        /// Тип текущих файлов (бинарные или текстовые).
        /// </summary>
        private bool isBin;

        /// <summary>
        /// Класс совмещения файлов.
        /// </summary>
        /// <param name="files">Список исходных файлов.</param>
        /// <param name="mode">Режим совмещения.</param>
        /// <param name="isBin">Тип исходных файлов.</param>
        public CombineFiles(List<FileStream> files, CombineMode mode, bool isBin)
        {
            this.mode = mode;
            this.isBin = isBin;
            listFileProcessing = new List<FileProcessing>();

            foreach (var file in files)
            {
                listFileProcessing.Add(new FileProcessing(file));
            }
        }

        /// <summary>
        /// Совмещение файлов.
        /// </summary>
        /// <returns>Экземпляр обработчика файла.</returns>
        public FileProcessing Combibe()
        {
            List<Line> resultListLines = new List<Line>();

            switch (mode)
            {
                default:
                case CombineMode.InEndOfPrevious:
                    foreach (var fileProcessing in listFileProcessing)
                    {
                        resultListLines.AddRange(fileProcessing.RefactoringFile(RefactoringMode.ModeForCombine));
                    }
                    break;
            }

            if (isBin)
            {
                return new FileProcessing(resultListLines, isBin);
            }
            else
            {
                return new FileProcessing(resultListLines, isBin, FileProcessing.GetEncoding(listFileProcessing[0].FileName));
            }
        }
    }
}
