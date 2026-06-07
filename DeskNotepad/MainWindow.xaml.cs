using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace DeskNotepad
{
    public partial class MainWindow : Window
    {
        private string documentsFolder;
        private bool isFixed = false;

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

            string lastFilesPath = Path.Combine(documentsFolder, ".lastfiles");
            List<string> filesToOpen = new List<string>();

            // Пытаемся прочитать .lastfiles
            if (File.Exists(lastFilesPath))
            {
                var lines = File.ReadAllLines(lastFilesPath);
                foreach (var line in lines)
                {
                    string filePath = line.Trim();
                    // Проверяем, существует ли файл и находится ли он в правильной папке
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath) && filePath.StartsWith(documentsFolder))
                    {
                        filesToOpen.Add(filePath);
                    }
                }
            }

            // Если .lastfiles есть и файлы из него существуют - открываем их
            if (filesToOpen.Count > 0)
            {
                foreach (var file in filesToOpen)
                {
                    LoadFileToTab(file);
                }
            }
            else
            {
                // Иначе открываем все существующие .txt файлы
                var existingFiles = Directory.GetFiles(documentsFolder, "*.txt");
                if (existingFiles.Length == 0)
                {
                    CreateNewFile();
                }
                else
                {
                    LoadFileToTab(existingFiles[0]);
                    
                }
            }
        }

        private void SaveLastFilesOrder()
        {
            string lastFilesPath = Path.Combine(documentsFolder, ".lastfiles");
            List<string> filePaths = new List<string>();

            foreach (TabItem tab in MainTabControl.Items)
            {
                if (tab.Tag is string filePath && File.Exists(filePath))
                {
                    filePaths.Add(filePath);
                }
            }

            File.WriteAllLines(lastFilesPath, filePaths);
        }
        private void CreateNewFile()
        {
            string fileName = $"note_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            string filePath = Path.Combine(documentsFolder, fileName);
            File.WriteAllText(filePath, "");
            LoadFileToTab(filePath);
        }

        private void LoadFileToTab(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string content = File.ReadAllText(filePath);

            TabItem tabItem = new TabItem();

            // Заставляем таб растягиваться
            tabItem.HorizontalAlignment = HorizontalAlignment.Stretch;
            tabItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            // Header с кнопкой закрытия
            StackPanel headerPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock headerText = new TextBlock() { Text = fileName, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            Button closeButton = new Button()
            {
                Content = "✕",
                Width = 18,
                Height = 18,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontSize = 10
            };
            closeButton.Click += (s, e) => CloseTab(tabItem, filePath);
            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(closeButton);
            tabItem.Header = headerPanel;

            // Контент таба - растягиваем на всю ширину
            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(49) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Изменено на Star
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });

            Button deleteButton = new Button()
            {
                Content = "Delete",
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 22,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 60,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.LightGray
            };
            Grid.SetColumn(deleteButton, 2);
            deleteButton.Click += (s, e) => DeleteFile(tabItem, filePath);

            Button closeTabButton = new Button()
            {
                Content = "Close",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(1, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 58,
                Height = 22,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.LightGray
            };
            Grid.SetColumn(closeTabButton, 3);
            closeTabButton.Click += (s, e) => CloseTab(tabItem, filePath);

            TextBox nameBox = new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                Text = fileName,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 22,
                Margin = new Thickness(10, 0, 15, 0),
                Background = Brushes.White,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.LightGray
            };
            Grid.SetColumn(nameBox, 1);
            nameBox.TextChanged += (s, e) =>
            {
                string newName = nameBox.Text;
                if (!string.IsNullOrEmpty(newName))
                {
                    if (!newName.EndsWith(".txt"))
                        newName += ".txt";

                    string newPath = Path.Combine(documentsFolder, newName);
                    if (!File.Exists(newPath) || newPath == filePath)
                    {
                        try
                        {
                            File.Move(filePath, newPath);
                            filePath = newPath;
                            headerText.Text = newName;
                            tabItem.Tag = filePath;
                        }
                        catch { }
                    }
                }
            };

            Label nameLabel = new Label()
            {
                Content = "Name",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 49,
                Height = 26,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black
            };
            Grid.SetColumn(nameLabel, 0);

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Background = Brushes.White;
            richTextBox.Foreground = Brushes.Black;
            richTextBox.BorderBrush = Brushes.LightGray;
            richTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            richTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(richTextBox, 1);
            Grid.SetColumnSpan(richTextBox, 4);

            if (!string.IsNullOrEmpty(content))
            {
                richTextBox.Document = new FlowDocument(new Paragraph(new Run(content)));
            }
            else
            {
                richTextBox.Document = new FlowDocument(new Paragraph(new Run("")));
            }

            richTextBox.TextChanged += (s, e) =>
            {
                string currentPath = tabItem.Tag as string;
                if (!string.IsNullOrEmpty(currentPath))
                {
                    TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                    File.WriteAllText(currentPath, textRange.Text);
                }
            };

            grid.Children.Add(deleteButton);
            grid.Children.Add(closeTabButton);
            grid.Children.Add(nameBox);
            grid.Children.Add(nameLabel);
            grid.Children.Add(richTextBox);

            tabItem.Content = grid;
            tabItem.Tag = filePath;

            MainTabControl.Items.Add(tabItem);
            MainTabControl.SelectedItem = tabItem;
        }
        private void CloseTab(TabItem tabItem, string filePath)
        {
            SaveCurrentTab(tabItem);

            if (MainTabControl.Items.Count == 1)
            {
                MainTabControl.Items.Remove(tabItem);
                CreateNewFile();
            }
            else
            {
                MainTabControl.Items.Remove(tabItem);
            }

            SaveLastFilesOrder(); // Сохраняем порядок после закрытия
        }

        private void DeleteFile(TabItem tabItem, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                CloseTab(tabItem, filePath);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentTab();
            CreateNewFile();
            SaveLastFilesOrder(); // Сохраняем порядок после создания
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentTab();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = documentsFolder;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                bool alreadyOpen = false;
                foreach (TabItem tab in MainTabControl.Items)
                {
                    if (tab.Tag as string == openFileDialog.FileName)
                    {
                        MainTabControl.SelectedItem = tab;
                        alreadyOpen = true;
                        break;
                    }
                }

                if (!alreadyOpen)
                {
                    LoadFileToTab(openFileDialog.FileName);
                    SaveLastFilesOrder(); // Сохраняем порядок после открытия
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveAllTabs();
            SaveLastFilesOrder(); // Сохраняем порядок перед закрытием
            base.OnClosing(e);
        }
        private void SaveCurrentTab(TabItem tab = null)
        {
            TabItem currentTab = tab ?? (MainTabControl.SelectedItem as TabItem);
            if (currentTab != null && currentTab.Content is Grid grid)
            {
                var richTextBox = FindVisualChild<RichTextBox>(grid);
                if (richTextBox != null && currentTab.Tag is string filePath)
                {
                    TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                    File.WriteAllText(filePath, textRange.Text);
                }
            }
        }

        private void SaveAllTabs()
        {
            foreach (TabItem tab in MainTabControl.Items)
            {
                SaveCurrentTab(tab);
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isFixed && e.ClickCount == 1)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isFixed)
                this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isFixed)
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAllTabs();
            Application.Current.Shutdown();
        }

        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            isFixed = !isFixed;

            if (isFixed)
            {
                TitleBar.Visibility = Visibility.Collapsed;
                WindowBorder.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                WindowBorder.BorderBrush = Brushes.Transparent;
                WindowBorder.CornerRadius = new CornerRadius(0);
                this.Topmost = false;

                ApplyDarkTheme();
            }
            else
            {
                TitleBar.Visibility = Visibility.Visible;
                WindowBorder.Background = Brushes.White;
                WindowBorder.BorderBrush = Brushes.Gray;
                WindowBorder.CornerRadius = new CornerRadius(5);

                ApplyLightTheme();
            }
        }

        private void ApplyDarkTheme()
        {
            // Создаем стиль для TabItem
            var style = new Style(typeof(TabItem));

            // Триггер для обычного состояния
            var normalTrigger = new Trigger { Property = TabItem.IsSelectedProperty, Value = false };
            normalTrigger.Setters.Add(new Setter(TabItem.BackgroundProperty, Brushes.Black));
            normalTrigger.Setters.Add(new Setter(TabItem.ForegroundProperty, Brushes.White));
            style.Triggers.Add(normalTrigger);

            // Триггер для выбранного состояния
            var selectedTrigger = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(TabItem.BackgroundProperty, Brushes.DarkGray));
            selectedTrigger.Setters.Add(new Setter(TabItem.ForegroundProperty, Brushes.White));
            style.Triggers.Add(selectedTrigger);

            // Триггер для наведения мыши
            var mouseOverTrigger = new Trigger { Property = TabItem.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(TabItem.BackgroundProperty, Brushes.Gray));
            mouseOverTrigger.Setters.Add(new Setter(TabItem.ForegroundProperty, Brushes.White));
            style.Triggers.Add(mouseOverTrigger);

            // Применяем стиль ко всем TabItem
            foreach (TabItem tab in MainTabControl.Items)
            {
                tab.Style = style;
            }

            MainTabControl.Background = Brushes.Black;
            MainTabControl.Foreground = Brushes.White;
            foreach (TabItem tab in MainTabControl.Items)
            {
                tab.Background = Brushes.Black;
                tab.Foreground = Brushes.White;
                if (tab.Header is StackPanel panel)
                {
                    foreach (var child in panel.Children)
                    {
                        if (child is TextBlock textBlock)
                        {
                            textBlock.Foreground = Brushes.White;
                        }
                    }
                }
                var selectedTab = MainTabControl.SelectedItem as TabItem;
                if (selectedTab != null)
                {
                    selectedTab.Background = Brushes.DarkGray;
                    selectedTab.Foreground = Brushes.White;
                }
                if (tab.Content is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is Button btn)
                        {
                            btn.Background = Brushes.Black;
                            btn.Foreground = Brushes.White;
                            btn.BorderBrush = Brushes.Gray;
                        }
                        else if (child is TextBox txt)
                        {
                            txt.Background = Brushes.Black;
                            txt.Foreground = Brushes.White;
                            txt.BorderBrush = Brushes.Gray;
                        }
                        else if (child is RichTextBox rtb)
                        {
                            rtb.Background = Brushes.Black;
                            rtb.Foreground = Brushes.White;
                            rtb.BorderBrush = Brushes.Gray;
                        }
                        else if (child is Label lbl)
                        {
                            lbl.Foreground = Brushes.White;
                            lbl.Background = Brushes.Black;

                        }
                    }
                }
            }


            OpenButton.Background = Brushes.Black;
            OpenButton.Foreground = Brushes.White;
            OpenButton.BorderBrush = Brushes.Gray;



            NewButton.Background = Brushes.Black;
            NewButton.Foreground = Brushes.White;
            NewButton.BorderBrush = Brushes.Gray;

            var newButton = FindVisualChild<Button>(this, "New_Click");
            if (newButton != null)
            {
                newButton.Background = Brushes.Black;
                newButton.Foreground = Brushes.White;
                newButton.BorderBrush = Brushes.Gray;
            }

            FixRadio.Foreground = Brushes.White;


        }

        private void ApplyLightTheme()
        {
            foreach (TabItem tab in MainTabControl.Items)
            {
                tab.Background = Brushes.White;
                tab.Foreground = Brushes.Black;

                if (tab.Content is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is Button btn)
                        {
                            btn.Background = Brushes.White;
                            btn.Foreground = Brushes.Black;
                            btn.BorderBrush = Brushes.LightGray;
                        }
                        else if (child is TextBox txt)
                        {
                            txt.Background = Brushes.White;
                            txt.Foreground = Brushes.Black;
                            txt.BorderBrush = Brushes.LightGray;
                        }
                        else if (child is RichTextBox rtb)
                        {
                            rtb.Background = Brushes.White;
                            rtb.Foreground = Brushes.Black;
                            rtb.BorderBrush = Brushes.LightGray;
                        }
                        else if (child is Label lbl)
                        {
                            lbl.Foreground = Brushes.Black;
                        }
                    }
                }
            }


            OpenButton.Background = Brushes.White;
            OpenButton.Foreground = Brushes.Black;
            OpenButton.BorderBrush = Brushes.LightGray;

            var newButton = FindVisualChild<Button>(this, "New_Click");
            if (newButton != null)
            {
                newButton.Background = Brushes.White;
                newButton.Foreground = Brushes.Black;
                newButton.BorderBrush = Brushes.LightGray;
            }

            FixRadio.Foreground = Brushes.Black;
        }

        private T FindVisualChild<T>(DependencyObject parent, string name = null) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && (string.IsNullOrEmpty(name) || typedChild.Name == name))
                    return typedChild;

                var result = FindVisualChild<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
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
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFixed)
            {
                // Делаем все вкладки черными
                foreach (TabItem tab in MainTabControl.Items)
                {
                    tab.Background = Brushes.Black;
                    tab.Foreground = Brushes.White;
                    // Принудительно обновляем
                    tab.InvalidateVisual();
                }

                // Выделенную делаем серой, но с небольшим таймаутом
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var selectedTab = MainTabControl.SelectedItem as TabItem;
                    if (selectedTab != null)
                    {
                        selectedTab.Background = Brushes.DarkGray;
                        selectedTab.Foreground = Brushes.White;
                        selectedTab.InvalidateVisual();
                    }
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }
    }
}