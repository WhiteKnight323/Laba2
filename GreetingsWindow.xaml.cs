using System.Windows;
using Microsoft.Win32;
using System.IO;
using System;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using WPFExcelView;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class GreetingsWindow : Window
    {
        bool needToLoad = true;
        int pageIndex = 1;
        int recPerPage;
        string path = "";
        private List<UBI> listUBI = new List<UBI>();
        private List<UBI> oldListUBI;
        public GreetingsWindow()
        {
            InitializeComponent();
            NumberOfRecords.Items.Add("15");
            NumberOfRecords.Items.Add("30");
            NumberOfRecords.Items.Add("50");
            NumberOfRecords.Items.Add("100");
            NumberOfRecords.SelectedItem = 15;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        public void OpenExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EXCEL Files (*.xlsx)|*.xlsx|EXCEL Files 2003 (*.xls)|*.xls|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != true)
            {
                MessageBox.Show("Ошибка! Вы не выбрали файла!");
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
                return;
            }
            path = openFileDialog.FileName;
            listUBI.Clear();
            ExcelConvert(openFileDialog.FileName);
            DrawTable();
        }
        public void DownloadFile()
        {
            string link = @"https://bdu.fstec.ru/files/documents/thrlist.xlsx";
            if (needToLoad)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel File (*.xlsx)|*.*";
                saveFileDialog.Title = "Куда нам его сохранять?";
                saveFileDialog.FileName = "thrlist.xlsx";
                if (saveFileDialog.ShowDialog() != true)
                {
                    MessageBox.Show("Ошибка! Вы не указали, куда сохранять файл!!!");
                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                    return;
                }
                path = saveFileDialog.FileName;
            }            
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(new Uri(link), path);
                client.Dispose();
            }
            needToLoad = false;
            oldListUBI = new List<UBI>(listUBI);
            listUBI.Clear();
            ExcelConvert(path);
            DrawTable();
        }
        private void DrawTable()
        {
            dataGrid1.ItemsSource = listUBI.Take(recPerPage);
            PageNumber.Content = $"{pageIndex} of {(listUBI.Count / recPerPage) + 1}";
            dataGrid1.ItemsSource = listUBI.Take(recPerPage);
            TableView();
        }
        private void TableView() // Этот метод нужен для отрисовки вида таблицы, в зависимости от вкладки
        {
            if (listUBI.Count == 0) // Эта проверка нужна потому, что Tab_Changed запускается при создании окна. А иначе имя столбцу нельзя присвоить, т.к. ДатаГрид не инициализировалась
            {
                return;
            }
            dataGrid1.Columns[0].Header = "Идентификатор угрозы";
            dataGrid1.Columns[1].Visibility = Visibility.Collapsed; // столбик состояние было/стало (тут он не нужен)
            dataGrid1.Columns[2].Header = "Наименование угрозы";
            dataGrid1.Columns[3].Header = "Описание угрозы";
            dataGrid1.Columns[4].Header = "Источник угрозы";
            dataGrid1.Columns[5].Header = "Объект воздействия угрозы";
            dataGrid1.Columns[6].Header = "Нарушение конфиденциальности";
            dataGrid1.Columns[7].Header = "Нарушение целостности";
            dataGrid1.Columns[8].Header = "Нарушение доступности";

            dataGrid1.Columns[0].Width = 150;
            dataGrid1.Columns[1].Width = 100;
            dataGrid1.Columns[2].Width = 600;
            dataGrid1.Columns[3].Width = 750;
            dataGrid1.Columns[4].Width = 500;
            dataGrid1.Columns[5].Width = 500;
            dataGrid1.Columns[6].Width = 200;
            dataGrid1.Columns[7].Width = 150;
            dataGrid1.Columns[8].Width = 150;
            if (TabCtrl.SelectedIndex == 0)
            {
                dataGrid1.RowHeight = 100;
                dataGrid1.Width = 3000;
                dataGrid1.Columns[1].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[3].Visibility = Visibility.Visible;
                dataGrid1.Columns[4].Visibility = Visibility.Visible;
                dataGrid1.Columns[5].Visibility = Visibility.Visible;
                dataGrid1.Columns[6].Visibility = Visibility.Visible;
                dataGrid1.Columns[7].Visibility = Visibility.Visible;
                dataGrid1.Columns[8].Visibility = Visibility.Visible;
            }
            if (TabCtrl.SelectedIndex == 1)
            {
                dataGrid1.RowHeight = 30;
                dataGrid1.Width = 1440;
                dataGrid1.Columns[2].Width = 1290;
                dataGrid1.Columns[1].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[3].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[4].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[5].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[6].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[7].Visibility = Visibility.Collapsed;
                dataGrid1.Columns[8].Visibility = Visibility.Collapsed;
            }
        }
        private void ExcelConvert(string fileNames)
        {
            int totalrow;
            int totalcolumn;
            ExcelPackage excel = new ExcelPackage(new FileInfo(fileNames));
            ExcelWorksheet worksheet = excel.Workbook.Worksheets[1];
            if (worksheet.Dimension == null || worksheet.Dimension.End.Column != 10) // Проверка, что у нас нужная таблица. В нужной таблице 10 столбцов.
            {
                MessageBox.Show("Ошибка! Вы скорее всего выбрали не тот документ!");
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
                return;
            }
            totalrow = worksheet.Dimension.End.Row + 1;
            totalcolumn = worksheet.Dimension.End.Column;
            for (int rowIndex = 3; rowIndex < totalrow; rowIndex++)
            {
                List<string> list = new List<string>();
                for (int i = 1; i < totalcolumn; i++)
                {
                    if (worksheet.Cells[rowIndex, i].Value == null)
                    {
                        list.Add("");
                    }
                    else
                    {
                        list.Add(worksheet.Cells[rowIndex, i].Value.ToString());
                    }
                }
                listUBI.Add(new UBI()
                {
                    Id = Int32.TryParse(list[0], out int result) ? $"УБИ.{result:000}" : "",
                    Name = list[1],
                    Description = list[2],
                    SourceOfThreat = list[3],
                    ObjectOfInfluence = list[4],
                    ViolationConf = list[5],
                    ViolationInteg = list[6],
                    ViolationAcc = list[7], // т.к. в условии ничего не сказано отобразить даты включения и изменения, я их не добавлял
                });
                list.Clear();
            }
        }
        private void Update_Button(object sender, RoutedEventArgs e)
        {
            UpdateWindow update = new UpdateWindow();
            if (!needToLoad)
            {
                MessageBox.Show("Ошибка! Вы скачали файл, у вас самая свежая база данных!!!");
                update.Close();
            }
            else
            {
                update.Show();
                DownloadFile();
                int countNew = listUBI.Count - oldListUBI.Count; 
                if (countNew != 0)
                {
                    for (int i = 0; i < countNew; i++)
                    {
                        oldListUBI.Add(new UBI() { Id = "" }); // Если в список на сайте добавили новые строки, то добавим недостающее количество пустых в список
                    }
                }
                List<UBI> listChanges = new List<UBI>();
                for (int i = 0; i < listUBI.Count; i++)
                {
                    bool b = false;
                    UBI was = new UBI();
                    UBI now = new UBI();
                    if (oldListUBI[i].Id == "")
                    {
                        now.Id = listUBI[i].Id;
                        now.Update = "Новая";
                        now.Name = listUBI[i].Name;
                        now.Description = listUBI[i].Description;
                        now.SourceOfThreat = listUBI[i].SourceOfThreat;
                        now.ObjectOfInfluence = listUBI[i].ObjectOfInfluence;
                        now.ViolationConf = listUBI[i].ViolationConf;
                        now.ViolationInteg = listUBI[i].ViolationInteg;
                        now.ViolationAcc = listUBI[i].ViolationAcc;
                        listChanges.Add(now);
                        continue;
                    }
                    if (oldListUBI[i].Name != listUBI[i].Name)
                    {
                        was.Name = oldListUBI[i].Name;
                        now.Name = listUBI[i].Name;
                        b = true;
                    }
                    else
                    {
                        was.Name = "";
                        now.Name = "";
                    }
                    if (oldListUBI[i].Description != listUBI[i].Description)
                    {
                        was.Description = oldListUBI[i].Description;
                        now.Description = listUBI[i].Description;
                        b = true;
                    }
                    else
                    {
                        was.Description = "";
                        now.Description = "";
                    }
                    if (oldListUBI[i].SourceOfThreat != listUBI[i].SourceOfThreat)
                    {
                        was.SourceOfThreat = oldListUBI[i].SourceOfThreat;
                        now.SourceOfThreat = listUBI[i].SourceOfThreat;
                        b = true;
                    }
                    else
                    {
                        was.SourceOfThreat = "";
                        now.SourceOfThreat = "";
                    }
                    if (oldListUBI[i].ObjectOfInfluence != listUBI[i].ObjectOfInfluence)
                    {
                        was.ObjectOfInfluence = oldListUBI[i].ObjectOfInfluence;
                        now.ObjectOfInfluence = listUBI[i].ObjectOfInfluence;
                        b = true;
                    }
                    else
                    {
                        was.ObjectOfInfluence = "";
                        now.ObjectOfInfluence = "";
                    }
                    if (oldListUBI[i].ViolationConf != listUBI[i].ViolationConf)
                    {
                        was.ViolationConf = oldListUBI[i].ViolationConf;
                        now.ViolationConf = listUBI[i].ViolationConf;
                        b = true;
                    }
                    else
                    {
                        was.ViolationConf = "";
                        now.ViolationConf = "";
                    }
                    if (oldListUBI[i].ViolationInteg != listUBI[i].ViolationInteg)
                    {
                        was.ViolationInteg = oldListUBI[i].ViolationInteg;
                        now.ViolationInteg = listUBI[i].ViolationInteg;
                        b = true;
                    }
                    else
                    {
                        was.ViolationInteg = "";
                        now.ViolationInteg = "";
                    }
                    if (oldListUBI[i].ViolationAcc != listUBI[i].ViolationAcc)
                    {
                        was.ViolationAcc = oldListUBI[i].ViolationAcc;
                        now.ViolationAcc = listUBI[i].ViolationAcc;
                        b = true;
                    }
                    else
                    {
                        was.ViolationAcc = "";
                        now.ViolationAcc = "";
                    }
                    if (b)
                    {
                        was.Id = listUBI[i].Id;
                        now.Id = "";
                        was.Update = "Было";
                        now.Update = "Стало";
                        listChanges.Add(was);
                        listChanges.Add(now);
                    }
                }
                if (listChanges.Count == 0)
                {
                    MessageBox.Show("Да впринципе и обновлять нечего... Все и так новое)))");
                    update.Close();
                }
                else
                {
                    update.dataGrid2.ItemsSource = listChanges;
                    update.dataGrid2.Columns[0].Header = "Идентификатор угрозы";
                    update.dataGrid2.Columns[1].Header = "Статус";
                    update.dataGrid2.Columns[2].Header = "Наименование угрозы";
                    update.dataGrid2.Columns[3].Header = "Описание угрозы";
                    update.dataGrid2.Columns[4].Header = "Источник угрозы";
                    update.dataGrid2.Columns[5].Header = "Объект воздействия угрозы";
                    update.dataGrid2.Columns[6].Header = "Нарушение конфиденциальности";
                    update.dataGrid2.Columns[7].Header = "Нарушение целостности";
                    update.dataGrid2.Columns[8].Header = "Нарушение доступности";
                    update.UpdateNote.Content = $"Вот список изменений! Обновлено {(listChanges.Count - countNew) / 2} строк, добавлено {countNew} строк";

                    update.dataGrid2.RowHeight = 100;
                    update.dataGrid2.Columns[0].Width = 150;
                    update.dataGrid2.Columns[1].Width = 100;
                    update.dataGrid2.Columns[2].Width = 600;
                    update.dataGrid2.Columns[3].Width = 750;
                    update.dataGrid2.Columns[4].Width = 500;
                    update.dataGrid2.Columns[5].Width = 500;
                    update.dataGrid2.Columns[6].Width = 200;
                    update.dataGrid2.Columns[7].Width = 150;
                    update.dataGrid2.Columns[8].Width = 150;
                }
            }
        }
        private void First_Button(object sender, System.EventArgs e)
        {
            pageIndex = 2;
            PrevPage();
        }
        private void Prev_Button(object sender, System.EventArgs e)
        {
            PrevPage();
        }
        private void Next_Button(object sender, System.EventArgs e)
        {
            NextPage();
        }
        private void Last_Button(object sender, System.EventArgs e)
        {
            pageIndex = (listUBI.Count / recPerPage);
            NextPage();
        }
        private void NextPage()
        {
            Prev.IsEnabled = true;
            First.IsEnabled = true;
            if (listUBI.Count >= (pageIndex * recPerPage))
            {
                dataGrid1.ItemsSource = null;
                if (listUBI.Skip(pageIndex * recPerPage).Take(recPerPage).Count() == 0)
                {

                    dataGrid1.ItemsSource = listUBI.Skip((pageIndex * recPerPage) - recPerPage).Take(recPerPage);
                }
                else
                {
                    dataGrid1.ItemsSource = listUBI.Skip(pageIndex * recPerPage).Take(recPerPage);
                    pageIndex++;
                    if (pageIndex == ((listUBI.Count / recPerPage) + 1))
                    {
                        Next.IsEnabled = false;
                        Last.IsEnabled = false;
                    }
                }
                PageNumber.Content = $"{pageIndex} of {(listUBI.Count / recPerPage) + 1}";
            }

            else
            {
                Next.IsEnabled = false;
                Last.IsEnabled = false;
            }
            TableView();
        }
        private void PrevPage()
        {
            Next.IsEnabled = true;
            Last.IsEnabled = true;
            if (pageIndex > 1)
            {
                pageIndex -= 1;
                dataGrid1.ItemsSource = null;
                if (pageIndex == 1)
                {
                    dataGrid1.ItemsSource = listUBI.Take(recPerPage);
                    Prev.IsEnabled = false;
                    First.IsEnabled = false;
                }
                else
                {
                    dataGrid1.ItemsSource = listUBI.Skip((pageIndex - 1) * recPerPage).Take(recPerPage);
                }
            }
            else
            {
                Prev.IsEnabled = false;
                First.IsEnabled = false;
            }
            PageNumber.Content = $"{pageIndex} of {(listUBI.Count / recPerPage) + 1}";
            TableView();
        }
        private void NumberOfRecords_Button(object sender, SelectionChangedEventArgs e)
        {
            pageIndex = 1;
            recPerPage = Convert.ToInt32(NumberOfRecords.SelectedItem);
            dataGrid1.ItemsSource = null;
            dataGrid1.ItemsSource = listUBI.Take(recPerPage);
            PageNumber.Content = $"{pageIndex} of {(listUBI.Count / recPerPage) + 1}";
            Next.IsEnabled = true;
            Last.IsEnabled = true;
            Prev.IsEnabled = false;
            First.IsEnabled = false;
            TableView();
        }
        private void Tab_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (listUBI.Count == 0)
            {
                return;
            }
            TableView();
        }
    }
}
