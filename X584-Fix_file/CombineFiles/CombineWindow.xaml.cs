using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace X584_Fix_file
{
    /// <summary>
    /// Логика взаимодействия для CombineWindow.xaml
    /// </summary>
    public partial class CombineWindow : Window
    {
        /// <summary>
        /// Экземпляр класса совмещения файлов.
        /// </summary>
        private CombineFiles combineFiles;

        /// <summary>
        /// Тип активных файлов (бинарные или текстовые).
        /// </summary>
        private bool isBin;

        public CombineWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки открытия файлов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (dg_table.Items.Count == 0)
            {
                openFileDialog.Filter = "Файл x584|*.x584|Текстовый файл|*.txt";
            }
            else
            {
                if (isBin)
                {
                    openFileDialog.Filter = "Файлы x584|*.x584";
                }
                else
                {
                    openFileDialog.Filter = "Текстовый файл|*.txt";
                }
            }

            if (openFileDialog.ShowDialog() == true)
            {
                Regex fileType = new Regex(".txt$", RegexOptions.IgnoreCase);
                if (fileType.IsMatch(openFileDialog.FileNames[0]))
                {
                    isBin = false;
                }
                else
                {
                    isBin = true;
                }

                foreach (var fileName in openFileDialog.FileNames)
                {
                    dg_table.Items.Add(new FileStream(fileName, FileMode.Open));
                }
            }

            if (dg_table.Items.Count > 1)
            {
                btn_combine.IsEnabled = true;
            }
        }

        /// <summary>
        /// Обработчик кнопки совмежения файлов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_combine_Click(object sender, RoutedEventArgs e)
        {
            FileProcessing resultFile;
            List<FileStream> files = new List<FileStream>();
            foreach (var fileStream in dg_table.Items)
            {
                files.Add((FileStream)fileStream);
            }

            try
            {
                combineFiles = new CombineFiles(files, CombineMode.InEndOfPrevious, isBin);
                resultFile = combineFiles.Combibe();
                //resultFile.RefactoringFile(RefactoringMode.ReplaceNumbersInComments);
                resultFile.RefactoringFile(RefactoringMode.InsertEmptyLinesToEnd);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if (!isBin)
            {
                saveFileDialog.Filter = "Текстовый файл|*.txt";
            }
            else
            {
                saveFileDialog.Filter = "Файл x584|*.x584";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                resultFile.Save(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Обработчик кнопки очистки таблицы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileStream fileStream in dg_table.Items)
            {
                fileStream.Close();
            }
            dg_table.Items.Clear();
            btn_combine.IsEnabled = false;
        }
    }
}
