using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace X584_Fix_file
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Название исходного файла.
        /// </summary>
        private string fileName;

        /// <summary>
        /// Сдвиг относительно начала в результирующем файле.
        /// </summary>
        private int offset;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки открытия файла.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Файл x584|*.x584|Текстовый файл|*.txt|Все файлы|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                lbl_fileName.Content = "Файл: " + openFileDialog.FileName;
                fileName = openFileDialog.FileName;

                btn_transform.IsEnabled = true;
            }
        }

        /// <summary>
        /// Обработчик кнопки форматирования и сохранения файла.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_transform_Click(object sender, RoutedEventArgs e)
        {
            FileProcessing fileProcessing;

            if (!int.TryParse(tb_offset.Text, out offset))
            {
                MessageBox.Show("Сдвиг должен быть задан числом");
                return;
            }

            List<int> emptyLines;

            try
            {
                emptyLines = GetEmptyLines();
                fileProcessing = new FileProcessing(fileName, emptyLines, offset, (bool)cb_EmptyLineMode.IsChecked);
                fileProcessing.RefactoringFile(RefactoringMode.FullMode);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            Regex fileType = new Regex(".txt$", RegexOptions.IgnoreCase);
            if (fileType.IsMatch(fileName))
            {
                saveFileDialog.Filter = "Текстовый файл|*.txt";
            }
            else
            {
                saveFileDialog.Filter = "Файл x584|*.x584";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                fileProcessing.Save(saveFileDialog.FileName);
            }
            fileProcessing.CloseFile();
        }

        /// <summary>
        /// Возвращает номера в результирующем списке, на которые нужно поставить пустые строки.
        /// </summary>
        /// <returns>Список новеров результирующего списка, на которые нужно поставить пустые строки.</returns>
        private List<int> GetEmptyLines()
        {
            string numbers = tb_EmptyLines.Text;
            if (numbers.Length == 0)
            {
                return new List<int>();
            }

            Regex.Replace(numbers, " ", string.Empty);
            string[] arrayNumbers = numbers.Split(',');
            List<int> intNumbers = new List<int>();

            for (int index = 0; index < arrayNumbers.Length; index++)
            {
                int number = int.Parse(arrayNumbers[index]);
                if ((number >= offset) && (intNumbers.IndexOf(number) == -1))
                {
                    intNumbers.Add(number);
                }
            }

            return intNumbers;
        }

        private void Btn_combine_Click(object sender, RoutedEventArgs e)
        {
            var CombineForm = new CombineWindow();
            CombineForm.Show();
        }
    }
}
