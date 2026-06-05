using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
                    File.Move(currentFilePath, newPath);
                    currentFilePath = newPath;
                    UpdateTabHeader();
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
                // Делаем окно прозрачным и всегда позади всех
                this.Background = System.Windows.Media.Brushes.Transparent;
                this.AllowsTransparency = true;
                this.WindowStyle = WindowStyle.None;
                this.Topmost = false;

                // Меняем все на черный фон с белым текстом
                ApplyDarkTheme();
            }
            else
            {
                // Возвращаем обычный вид
                this.Background = System.Windows.Media.Brushes.White;
                this.AllowsTransparency = false;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                // Возвращаем белый фон с черным текстом
                ApplyLightTheme();
            }
        }

        private void ApplyDarkTheme()
        {
            var allButtons = FindVisualChildren<Button>(this);
            foreach (var btn in allButtons)
            {
                btn.Background = System.Windows.Media.Brushes.Black;
                btn.Foreground = System.Windows.Media.Brushes.White;
                btn.BorderBrush = System.Windows.Media.Brushes.Gray;
            }

            FileTypeCombo.Background = System.Windows.Media.Brushes.Black;
            FileTypeCombo.Foreground = System.Windows.Media.Brushes.White;
            FileTypeCombo.BorderBrush = System.Windows.Media.Brushes.Gray;

            FileNameTextBox.Background = System.Windows.Media.Brushes.Black;
            FileNameTextBox.Foreground = System.Windows.Media.Brushes.White;
            FileNameTextBox.BorderBrush = System.Windows.Media.Brushes.Gray;

            MainRichTextBox.Background = System.Windows.Media.Brushes.Black;
            MainRichTextBox.Foreground = System.Windows.Media.Brushes.White;
            MainRichTextBox.BorderBrush = System.Windows.Media.Brushes.Gray;

            var tabItem = MainTabControl.Items[0] as TabItem;
            if (tabItem != null)
            {
                tabItem.Background = System.Windows.Media.Brushes.Black;
                tabItem.Foreground = System.Windows.Media.Brushes.White;
            }

            FixRadio.Foreground = System.Windows.Media.Brushes.White;
        }

        private void ApplyLightTheme()
        {
            var allButtons = FindVisualChildren<Button>(this);
            foreach (var btn in allButtons)
            {
                btn.Background = System.Windows.Media.Brushes.White;
                btn.Foreground = System.Windows.Media.Brushes.Black;
                btn.BorderBrush = System.Windows.Media.Brushes.LightGray;
            }

            FileTypeCombo.Background = System.Windows.Media.Brushes.White;
            FileTypeCombo.Foreground = System.Windows.Media.Brushes.Black;
            FileTypeCombo.BorderBrush = System.Windows.Media.Brushes.LightGray;

            FileNameTextBox.Background = System.Windows.Media.Brushes.White;
            FileNameTextBox.Foreground = System.Windows.Media.Brushes.Black;
            FileNameTextBox.BorderBrush = System.Windows.Media.Brushes.LightGray;

            MainRichTextBox.Background = System.Windows.Media.Brushes.White;
            MainRichTextBox.Foreground = System.Windows.Media.Brushes.Black;
            MainRichTextBox.BorderBrush = System.Windows.Media.Brushes.LightGray;

            var tabItem = MainTabControl.Items[0] as TabItem;
            if (tabItem != null)
            {
                tabItem.Background = System.Windows.Media.Brushes.White;
                tabItem.Foreground = System.Windows.Media.Brushes.Black;
            }

            FixRadio.Foreground = System.Windows.Media.Brushes.Black;
        }

        private System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
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