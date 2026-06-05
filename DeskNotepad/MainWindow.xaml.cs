using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;

namespace DeskNotepad
{
    public partial class MainWindow : Window
    {
        private string documentsFolder;
        private bool isFixed = false;
        private string currentFilePath;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            documentsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DeskNotepad");
            if (!Directory.Exists(documentsFolder))
                Directory.CreateDirectory(documentsFolder);

            currentFilePath = Path.Combine(documentsFolder, "filename.txt");

            if (!File.Exists(currentFilePath))
                File.WriteAllText(currentFilePath, "");

            string content = File.ReadAllText(currentFilePath);
            MainRichTextBox.Document.Blocks.Clear();
            MainRichTextBox.Document.Blocks.Add(new Paragraph(new Run(content)));
            FileNameTextBox.Text = Path.GetFileName(currentFilePath);
            UpdateTabHeader();
        }

        private void UpdateTabHeader()
        {
            var tabItem = MainTabControl.Items[0] as TabItem;
            if (tabItem != null)
                tabItem.Header = Path.GetFileName(currentFilePath);
        }

        private void SaveCurrentFile()
        {
            TextRange textRange = new TextRange(MainRichTextBox.Document.ContentStart, MainRichTextBox.Document.ContentEnd);
            File.WriteAllText(currentFilePath, textRange.Text);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();
            currentFilePath = Path.Combine(documentsFolder, $"note_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
            File.WriteAllText(currentFilePath, "");
            MainRichTextBox.Document.Blocks.Clear();
            MainRichTextBox.Document.Blocks.Add(new Paragraph(new Run("")));
            FileNameTextBox.Text = Path.GetFileName(currentFilePath);
            UpdateTabHeader();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = documentsFolder;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                currentFilePath = openFileDialog.FileName;
                string content = File.ReadAllText(currentFilePath);
                MainRichTextBox.Document.Blocks.Clear();
                MainRichTextBox.Document.Blocks.Add(new Paragraph(new Run(content)));
                FileNameTextBox.Text = Path.GetFileName(currentFilePath);
                UpdateTabHeader();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(currentFilePath))
            {
                File.Delete(currentFilePath);
                New_Click(sender, e);
            }
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();
            New_Click(sender, e);
        }

        private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newName = FileNameTextBox.Text;
            if (!string.IsNullOrEmpty(newName))
            {
                if (!newName.EndsWith(".txt"))
                    newName += ".txt";

                string newPath = Path.Combine(documentsFolder, newName);
                if (!File.Exists(newPath) || newPath == currentFilePath)
                {
                    try
                    {
                        File.Move(currentFilePath, newPath);
                        currentFilePath = newPath;
                        UpdateTabHeader();
                    }
                    catch { }
                }
            }
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveCurrentFile();
        }

        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            isFixed = !isFixed;

            if (isFixed)
            {
                // Делаем фон прозрачным, убираем рамку, отключаем поверх всех
                WindowBorder.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                WindowBorder.BorderBrush = Brushes.Transparent;
                this.Topmost = false;

                ApplyDarkTheme();
            }
            else
            {
                // Возвращаем белый фон и рамку
                WindowBorder.Background = Brushes.White;
                WindowBorder.BorderBrush = Brushes.Gray;

                ApplyLightTheme();
            }
        }

        private void MoveWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isFixed)
                this.DragMove();
        }

        private void HiddenButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ты нашел ебанутую кнопку на 3600 пикселей!");
        }

        private void ApplyDarkTheme()
        {
            var allButtons = FindVisualChildren<Button>(this);
            foreach (var btn in allButtons)
            {
                btn.Background = Brushes.Black;
                btn.Foreground = Brushes.White;
                btn.BorderBrush = Brushes.Gray;
            }

            FileTypeCombo.Background = Brushes.Black;
            FileTypeCombo.Foreground = Brushes.White;
            FileTypeCombo.BorderBrush = Brushes.Gray;

            FileNameTextBox.Background = Brushes.Black;
            FileNameTextBox.Foreground = Brushes.White;
            FileNameTextBox.BorderBrush = Brushes.Gray;

            MainRichTextBox.Background = Brushes.Black;
            MainRichTextBox.Foreground = Brushes.White;
            MainRichTextBox.BorderBrush = Brushes.Gray;

            var tabItem = MainTabControl.Items[0] as TabItem;
            if (tabItem != null)
            {
                tabItem.Background = Brushes.Black;
                tabItem.Foreground = Brushes.White;
            }

            FixRadio.Foreground = Brushes.White;
        }

        private void ApplyLightTheme()
        {
            var allButtons = FindVisualChildren<Button>(this);
            foreach (var btn in allButtons)
            {
                btn.Background = Brushes.White;
                btn.Foreground = Brushes.Black;
                btn.BorderBrush = Brushes.LightGray;
            }

            FileTypeCombo.Background = Brushes.White;
            FileTypeCombo.Foreground = Brushes.Black;
            FileTypeCombo.BorderBrush = Brushes.LightGray;

            FileNameTextBox.Background = Brushes.White;
            FileNameTextBox.Foreground = Brushes.Black;
            FileNameTextBox.BorderBrush = Brushes.LightGray;

            MainRichTextBox.Background = Brushes.White;
            MainRichTextBox.Foreground = Brushes.Black;
            MainRichTextBox.BorderBrush = Brushes.LightGray;

            var tabItem = MainTabControl.Items[0] as TabItem;
            if (tabItem != null)
            {
                tabItem.Background = Brushes.White;
                tabItem.Foreground = Brushes.Black;
            }

            FixRadio.Foreground = Brushes.Black;
        }

        private System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    yield return typedChild;

                foreach (var grandChild in FindVisualChildren<T>(child))
                    yield return grandChild;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveCurrentFile();
            base.OnClosing(e);
        }
    }
}