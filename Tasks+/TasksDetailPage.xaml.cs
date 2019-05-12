using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Tasks_
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TasksDetailPage : Tasks_.Common.LayoutAwarePage
    {
        public TasksDetailPage()
        {
            this.InitializeComponent();
        }
        
        public async void MessageShow(string msg)
        {
            Windows.UI.Popups.MessageDialog msgb = new Windows.UI.Popups.MessageDialog(msg);
            await msgb.ShowAsync();
        }

        //FileName - то імя файла, в якому зберігається завдання до кожної категорії,
        //FileName має таке саме імя шо і категорія, тому легко створюється і передає параметр...
        public string FileName;
        public ImageBrush categoryBrush; //для GridViewItem  
        public SolidColorBrush taskcolor, whiteColor;


        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные о событиях, описывающие, каким образом была достигнута эта страница.  Свойство Parameter
        /// обычно используется для настройки страницы.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SetViualSettings(); //грузимо візуальні налаштування (фони приток і самої проги)
            FileName = e.Parameter.ToString() + ".txt";  //імя файлу то імя категорії яке ми передаємо з головної сторінки
            StorageFile file;
            string ss = "0";
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
                ss = await FileIO.ReadTextAsync(file);      //читаємо дані з файлу
            }
            catch (FileNotFoundException)       //якщо виникне виключення що файлу нема
            {
                file = null;                  //топрисвоєму йому null
            }
            if (file != null && ss != "")                //якшо не null то він існує + якшо в файлі шось є, то ми можемо
                await LoadChanges();       //завантажити параметри
        }

        #region Налаштування вигляду
        public async void SetViualSettings()
        {
            StorageFile file;
            try
            {
                // Відкриваємо та читаємо Settings,txt
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("Settings.txt");
            }
            catch (FileNotFoundException)
            {
                file = null;
            }
            if (file != null)
            {
                var result = await FileIO.ReadTextAsync(file);
                // Перетворюємо прочитане в JsonArray
                var Settings = JsonArray.Parse(result);
                // Перетворюємо JSON в настройки
                CreateSetting(Settings);
            }
        }

        private async void CreateSetting(JsonArray array)
        {
            foreach (var item in array)
            {
                var obj = item.GetObject();
                string fon = "";
                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;
                    switch (key)
                    {
                        case "Fon":                         //шукаємо Fon 
                            fon = val.GetString();   //зчитуємо значення воно може бути за замоуч, зобр і кольори
                            break;
                    }
                }

                //////вибираємо налаштування для фону проги
                if (fon == "За замовчуванням" || fon == "Default" || fon == "По умолчанию")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 29, 29, 29));
                    SolidColorBrush white = new SolidColorBrush(Windows.UI.Colors.White);
                    AppName.Foreground = white;             //коли фон білий треба поставити тексти і кнопку додати(що є білою) - чорним
                    TaskText.Foreground = white;
                    AddButton.Foreground = white;
                    Grid1.Background = color;   //робимо фон чорним
                }
                else if (fon == "Зображення" || fon == "Image" || fon == "Изображение")
                {
                    StorageFile f = await ApplicationData.Current.LocalFolder.GetFileAsync("Settings.txt");
                    string pictname = await FileIO.ReadTextAsync(f);
                    ImageBrush backBrush = new ImageBrush();              //фон
                    BitmapImage backImage = new BitmapImage();
                    backImage.UriSource = new Uri(ApplicationData.Current.LocalFolder.Path + @"\Fon.png", UriKind.Absolute);

                    backBrush.ImageSource = backImage;
                    backBrush.Stretch = Stretch.Fill;
                    Grid1.Background = backBrush;
                }
                else if (fon == "Білий" || fon == "White" || fon == "Белый")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.White);
                    SolidColorBrush black = new SolidColorBrush(Windows.UI.Colors.Black);
                    AppName.Foreground = black;             //коли фон білий треба поставити тексти і кнопку додати(що є білою) - чорним
                    TaskText.Foreground = black;
                    AddButton.Foreground = black;
                    Grid1.Background = color;   //робимо фон білим
                }
                else if (fon == "Жовтий" || fon == "Yellow" || fon == "Желтый")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Yellow);
                    Grid1.Background = color;   //робимо фон
                }
                else if (fon == "Зелений" || fon == "Green" || fon == "Зеленый")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Green);
                    Grid1.Background = color;   //робимо фон
                }
                else if (fon == "Оранджевий" || fon == "Orange" || fon == "Оранжевый")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Orange);
                    Grid1.Background = color;   //робимо фон
                }
                else if (fon == "Синій" || fon == "Blue" || fon == "Синий")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Blue);
                    Grid1.Background = color;   //робимо фон
                }
                else if (fon == "Фіолетовий" || fon == "Violet" || fon == "Фиолетовый")
                {
                    Color _back = Color.FromArgb(255, 165, 0, 153); //наш фіолетовий
                    SolidColorBrush color = new SolidColorBrush(_back);
                    Grid1.Background = color;   //робимо фон
                }
                else if (fon == "Чорний" || fon == "Black" || fon == "Черный")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Black);
                    SolidColorBrush white = new SolidColorBrush(Windows.UI.Colors.White);
                    AppName.Foreground = white;             //коли фон білий треба поставити тексти і кнопку додати(що є білою) - чорним
                    TaskText.Foreground = white;
                    AddButton.Foreground = white;
                    Grid1.Background = color;   //робимо фон чорним
                }
                else if (fon == "Червоний" || fon == "Red" || fon == "Красный")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Red);
                    Grid1.Background = color;   //робимо фон
                }
            }
        }
        #endregion

        public void GoBack(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// клік на ітемі
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LVClick(object sender, ItemClickEventArgs e)
        {
            CheckBox cb = (CheckBox)e.ClickedItem;
            cb.IsChecked = true;
            LV.Items.Remove(cb);

            SaveChanges();  //зберігаємо зміни
        }

        private void LVChecked(object sender, RoutedEventArgs e)    //коли чекед робимо то, що при звичайному кліку
        {
            CheckBox cb = (CheckBox)e.OriginalSource;
            cb.IsChecked = true;
            LV.Items.Remove(cb);
            SaveChanges();  //зберігаємо зміни
        }

        private void LV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LV.SelectedItem != null)    //якшо є вибраний елемент
            {
                EditSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
                DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible; //то показати нижній аппбар
                DownerAppBar.IsOpen = true;     //і відкрити його
            }
            else
                DownerAppBar.IsOpen = false;
        }
        
        /// <summary>
        /// Видалення ітема
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            LV.Items.Remove(LV.SelectedItem);           //видаляємо вибраний
            SaveChanges();  //після видалення зберігаємо зміни
        }

        private void DownerAppBar_Closed(object sender, object e)
        {
            LV.SelectedItem = null;     //коли скривається аппбар вибраних елементів немає бути тобто null
            DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed; //зробити нижній аппбар невидимим коли нема вибраних елементів
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #region Зчитування параметрів з файлу

        public async void SaveChanges()
        {
            if (LV.Items.Count > 0) //Якщо є ітеми то ми зберігаємо
            {
                string SavedSetting = "[\n";
                List<string> ContentOfLV = new List<string>();      //створюємо лист для контентів ітемів
                foreach (CheckBox cb in LV.Items)
                    ContentOfLV.Add(cb.Content.ToString());                     //додаємо в нього дані
                int i = 0;
                for (i = 0; i < ContentOfLV.Count - 1; i++)
                    SavedSetting += "{ \"Content\" : \"" + ContentOfLV[i] + "\" },\n";  //додаємо в строчку настройки в форматі JSON 
                SavedSetting += "{ \"Content\" : \"" + ContentOfLV[i] + "\" } \n ]";    //то саме тільки без коми і з ] вкінці
                try
                {
                    StorageFolder current = ApplicationData.Current.LocalFolder;
                    StorageFile file = await current.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);  //створюємо файл? якшо є то заміняємо
                    await FileIO.WriteTextAsync(file, SavedSetting);        //записуємо налаштування
                }
                catch
                { MessageShow("Error"); }
            }
            else                //якщо в LV НІЧОГО НЕМА ТО МИ ВИдаляємо файл з налаштуваннями
                try
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
                    await file.DeleteAsync();   //видаляємо файл
                }
                catch
                { MessageShow("Error"); }
        }

        public async Task LoadChanges()
        {
            LV.Items.Clear();       //очищуємо GV, шоб налаштування ненакладалися на ті шо були загружен перед тим
            // Відкриваємо та читаємо Settings,txt
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
            var result = await FileIO.ReadTextAsync(file);
            // Перетворюємо прочитане в JsonArray
            var Settings = JsonArray.Parse(result);
            // Перетворюємо JSON в настройки
            CreateSettings(Settings);
        }

        private void CreateSettings(JsonArray array)
        {
            foreach (var item in array)
            {
                var obj = item.GetObject();
                CheckBox cb = new CheckBox();
                cb.FontSize = 25;
                cb.Padding = new Thickness(5,-5, 0, 0);
                cb.Checked += LVChecked;
                //якщо taskcolor != null значить юзер вибрав білий колір як фон і ми присвоюємо фону чорний
                if (taskcolor != null)
                    cb.Foreground = taskcolor;
                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;
                    switch (key)
                    {
                        case "Content":                         //шукаємо Content 
                            cb.Content = val.GetString();   //і присвоюємо його зн. до  GVItem.Content
                            break;
                    }
                }
                LV.Items.Add(cb);
            }
        }

        #endregion

        #region Кліки, що обробляють редагування ітема
        
        /// <summary>
        /// клік на кнопку редагувати
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButtonClick(object sender, RoutedEventArgs e)
        {
            if (LV.SelectedItem != null)
            {
                CheckBox cb = (CheckBox)LV.SelectedItem;       //зчитуємо дані в залежності від вибраного Ітема
                NameFeed.Text = cb.Content.ToString();                         //даємо NameFeedу текст вибраного ітема

                SaveButton1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                SaveButton2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                //робимо видиму Stack Panel шо містить поле і кнопку для редагування
                ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        /// <summary>
        /// клік на кнопку зберегти то шо відредаговане
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditSaveButton(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)LV.SelectedItem;
            cb.Content = NameFeed.Text;
            cb.FontSize = 25;
            cb.Padding = new Thickness(5, -5, 0, 0);
            LV.SelectedItem = cb;                //заміняємо вибраний ітем нашим відредагованим ітемом

            //робимо панель невидимою, щоб небуло помилок,
            //при кліку на кнопку зберегти коли не клікнуто додати чи редагувати
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveChanges();  //після редаг зберігаємо зміни
        }
        



        #endregion

        #region Кліки, що обробляють додавання ітема

        /// <summary>
        /// кліка на плюсик = кнопку додати
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItem(object sender, RoutedEventArgs e)
        {
            SaveButton1.Visibility = Windows.UI.Xaml.Visibility.Collapsed; //робимо невидимою пуршу кнопку
            SaveButton2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //робимо видиму stackPanel шо мітить кнопки і поле для редагування
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            // є дві кнопки зберегти 1 для збереження редагування, 2 - для збереження контенту і додавання ітему
            //їм треба почергово міняти видимість взалежності від потреб користувача

            //відкриваємо аппбар
            DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            DownerAppBar.IsOpen = true;
            EditSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// клік на кнопку зберегти і додати
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSaveButton(object sender, RoutedEventArgs e)
        {
            CheckBox cb = new CheckBox();
            cb.Content = NameFeed.Text;           //присвоюємо контент ListViewItem(у)
            cb.Checked += LVChecked;
            cb.FontSize = 25;
            //якщо taskcolor != null значить юзер вибрав білий колір як фон і ми присвоюємо фону чорний
            if (taskcolor != null)
                cb.Foreground = taskcolor;
            cb.Padding = new Thickness(5,-5, 0, 0);
            LV.Items.Add(cb);                          //додаємо в GV

            //після додавання робимо невидимими кнопку 2
            SaveButton2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveButton1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //робимо панель невидимою, щоб небуло помилок,
            //при кліку на кнопку зберегти коли не клікнуто додати чи редагувати
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DownerAppBar.IsOpen = false;
            DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            SaveChanges();  //після редаг зберігаємо зміни
        }

        #endregion

        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            SetViualSettings(); //грузимо візуальні налаштування (фони приток і самої проги)
        }
    }
}
