using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using X584_Fix_file;

namespace X584_Fix_file_Tests
{
    [TestClass]
    public class FileProcessingTest
    {
        [TestMethod]
        public void RefactoringFileText_MainTest()
        {
            List<Line> startListLines = new List<Line>
            {
                new Line(),
                new Line(),
                new Line(2, "Sample_0"),
                new Line(3, "Sample_1", "идти_на 2"),
                new Line(),
                new Line(),
                new Line(),
                new Line(7, "Sample_2", "иди_на 4"),
                new Line(8, "Sample_3"),
                new Line(9, "Sample_4", "Если ПАЛУ3 то 12 иначе 10"),
                new Line(10, "Sample_5", "идти_на 13"),
                new Line(11, "Sample_6"),
                new Line(12, "Sample_7"),
                new Line(13, "Sample_8", "Если !СДЛ1 то 8 иначе 15"),
                new Line(14, "Sample_9"),
                new Line(15, "Sample_10", "идти_на 0")
            };
            while (startListLines.Count < 1024)
            {
                startListLines.Add(new Line(startListLines.Count));
            }
            new FileProcessing(startListLines, false).Save(@"TempFiles\StartFile.txt");

            List<Line> expectedListLines = new List<Line>
            {
                new Line(0),
                new Line(1),
                new Line(2),
                new Line(3, "Sample_0"),
                new Line(4, "Sample_1", "идти_на 3"),
                new Line(5),
                new Line(6),
                new Line(7, "Sample_2", "идти_на 7"),
                new Line(8),
                new Line(9, "Sample_3"),
                new Line(10, "Sample_4", "Если ПАЛУ3 то 11 иначе 12"),
                new Line(11, "<ПУСТО>", "идти_на 14"),
                new Line(12, "Sample_5", "идти_на 15"),
                new Line(13, "Sample_6"),
                new Line(14, "Sample_7"),
                new Line(15, "Sample_8", "Если !СДЛ1 то 16 иначе 17"),
                new Line(16, "<ПУСТО>", "идти_на 9"),
                new Line(17, "<ПУСТО>", "идти_на 19"),
                new Line(18, "Sample_9"),
                new Line(19, "Sample_10", "идти_на 3")
            };
            while (expectedListLines.Count < 1024)
            {
                expectedListLines.Add(new Line(expectedListLines.Count));
            }
            new FileProcessing(expectedListLines, false).Save(@"TempFiles\ExpectedFile.txt");

            List<int> emptyLines = new List<int> { 5, 6, 8 };
            FileProcessing fileProcessing = new FileProcessing(@"TempFiles\StartFile.txt", emptyLines, 3, false);
            fileProcessing.RefactoringFile(RefactoringMode.FullMode);
            fileProcessing.Save(@"TempFiles\ResultFile.txt");

            FileAssert.AreEqual(@"TempFiles\ExpectedFile.txt", @"TempFiles\ResultFile.txt");
        }

        [TestMethod]
        public void RefactoringFileFullMode_Test()
        {
            List<Line> startListLines = new List<Line>
            {
                new Line(),
                new Line(),
                new Line(2, "Sample_0"),
                new Line(3, "Sample_1", "идти_на 2"),
                new Line(),
                new Line(),
                new Line(),
                new Line(7, "Sample_2", "иди_на 4"),
                new Line(8, "Sample_3"),
                new Line(9, "Sample_4", "Если ПАЛУ3 то 12 иначе 13"),
                new Line(10, "Sample_5"),
                new Line(11, "Sample_6"),
                new Line(12, "Sample_7"),
                new Line(13, "Sample_8", "Если !СДЛ1 то 8 иначе 15"),
                new Line(14, "Sample_9"),
                new Line(15, "Sample_10", "идти_на 0")
            };

            List<Line> expectedListLines = new List<Line>
            {
                new Line(0, "Sample_0"),
                new Line(1, "Sample_1", "идти_на 0"),
                new Line(2, "Sample_2", "идти_на 2"),
                new Line(3, "Sample_3"),
                new Line(4, "Sample_4", "Если ПАЛУ3 то 5 иначе 6"),
                new Line(5, "<ПУСТО>", "идти_на 9"),
                new Line(6, "<ПУСТО>", "идти_на 10"),
                new Line(7, "Sample_5"),
                new Line(8, "Sample_6"),
                new Line(9, "Sample_7"),
                new Line(10, "Sample_8", "Если !СДЛ1 то 11 иначе 12"),
                new Line(11, "<ПУСТО>", "идти_на 3"),
                new Line(12, "<ПУСТО>", "идти_на 14"),
                new Line(13, "Sample_9"),
                new Line(14, "Sample_10", "идти_на 0")
            };

            while (expectedListLines.Count < 1024)
            {
                expectedListLines.Add(new Line(expectedListLines.Count));
            }

            FileProcessing fileProcessing = new FileProcessing(startListLines, false, Encoding.Default);
            List<Line> resultListLines = fileProcessing.RefactoringFile(RefactoringMode.FullMode);

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEqual(expectedListLines, resultListLines);
        }

        [TestMethod]
        public void RefactoringFileFullModeWithReplaceEmptyMI_Test()
        {
            List<Line> startListLines = new List<Line>
            {
                new Line(),
                new Line(),
                new Line(2, "Sample_0"),
                new Line(3, "Sample_1", "идти_на 2"),
                new Line(),
                new Line(),
                new Line(),
                new Line(7, "Sample_2", "иди_на 4"),
                new Line(8, "Sample_3"),
                new Line(9, "Sample_4", "Если ПАЛУ3 то 12 иначе 13"),
                new Line(10, "Sample_5"),
                new Line(11, "Sample_6"),
                new Line(12, "Sample_7"),
                new Line(13, "Sample_8", "Если !СДЛ1 то 8 иначе 15"),
                new Line(14, "Sample_9"),
                new Line(15, "Sample_10", "идти_на 0")
            };

            List<Line> expectedListLines = new List<Line>
            {
                new Line(0, "Sample_0"),
                new Line(1, "Sample_1", "идти_на 0"),
                new Line(2, "Sample_2", "идти_на 2"),
                new Line(3, "Sample_3"),
                new Line(4, "Sample_4", "Если ПАЛУ3 то 5 иначе 6"),
                new Line(5, "РР := РР + П (П=0)", "идти_на 9"),
                new Line(6, "РР := РР + П (П=0)", "идти_на 10"),
                new Line(7, "Sample_5"),
                new Line(8, "Sample_6"),
                new Line(9, "Sample_7"),
                new Line(10, "Sample_8", "Если !СДЛ1 то 11 иначе 12"),
                new Line(11, "РР := РР + П (П=0)", "идти_на 3"),
                new Line(12, "РР := РР + П (П=0)", "идти_на 14"),
                new Line(12, "Sample_9"),
                new Line(13, "Sample_10", "идти_на 0")
            };

            while (expectedListLines.Count < 1024)
            {
                expectedListLines.Add(new Line(expectedListLines.Count, "РР := РР + П (П=0)"));
            }

            FileProcessing fileProcessing = new FileProcessing(startListLines, false, Encoding.Default, true);
            List<Line> resultListLines = fileProcessing.RefactoringFile(RefactoringMode.FullMode);

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEqual(expectedListLines, resultListLines);
        }

        [TestMethod]
        public void RefactoringFileSwapLinesMode_Test()
        {
            List<Line> startListLines = new List<Line>
            {
                new Line(),
                new Line(),
                new Line(2, "Sample_0"),
                new Line(3, "Sample_1", "идти_на 2"),
                new Line(),
                new Line(),
                new Line(),
                new Line(7, "Sample_2", "иди_на 4")
            };

            List<Line> expectedListLines = new List<Line>
            {
                new Line(0, "Sample_0"),
                new Line(1, "Sample_1", "идти_на 0"),
                new Line(2, "Sample_2", "иди_на 4")
            };

            FileProcessing fileProcessing = new FileProcessing(startListLines, false, Encoding.Default);
            List<Line> resultListLines = fileProcessing.RefactoringFile(RefactoringMode.SwapLines);

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEqual(expectedListLines, resultListLines);
        }

        [TestMethod]
        public void RefactoringFileSwapLinesTwoIf_Test()
        {
            List<Line> startListLines = new List<Line>
            {
                new Line(0, "Sample_0"),
                new Line(1, "Sample_1"),
                new Line(2, "Sample_2"),
                new Line(3, "Sample_3", "Если СДЛ1 то 4 иначе 7"),
                new Line(4, "Sample_4", "Если П то 5 иначе 8"),
                new Line(5, "Sample_5"),
                new Line(6, "Sample_6"),
                new Line(7, "Sample_7"),
                new Line(8, "Sample_8"),
            };

            List<Line> expectedListLines = new List<Line>
            {
                new Line(0, "Sample_0"),
                new Line(1, "Sample_1"),
                new Line(2, "Sample_2"),
                new Line(3, "Sample_3", "Если СДЛ1 то 4 иначе 5"),
                new Line(4, "<ПУСТО>", "идти_на 6"),
                new Line(5, "<ПУСТО>", "идти_на 10"),
                new Line(6, "Sample_4", "Если П то 7 иначе 8"),
                new Line(7, "Sample_5", "идти_на 9"),
                new Line(8, "<ПУСТО>", "идти_на 11"),
                new Line(9, "Sample_6"),
                new Line(10, "Sample_7"),
                new Line(11, "Sample_8"),
            };
            while (expectedListLines.Count < 1024)
            {
                expectedListLines.Add(new Line(expectedListLines.Count, "<ПУСТО>"));
            }

            FileProcessing fileProcessing = new FileProcessing(startListLines, false, Encoding.Default);
            List<Line> resultListLines = fileProcessing.RefactoringFile(RefactoringMode.FullMode);

            Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert.AreEqual(expectedListLines, resultListLines);
        }
    }
}
