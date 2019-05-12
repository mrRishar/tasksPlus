using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.Runtime.Serialization.Json;    //для зберігання і обробки даних
using Windows.Data.Json;                   //------------------------------
using Windows.Storage;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Tasks_
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Tasks_.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public ImageBrush categoryBrush; //для GridViewItem  
        public SolidColorBrush Categorycolor, whiteColor;

        public async void MessageShow(string msg)
        {
            Windows.UI.Popups.MessageDialog msgb = new Windows.UI.Popups.MessageDialog(msg);
            await msgb.ShowAsync();
        }       

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные о событиях, описывающие, каким образом была достигнута эта страница.  Свойство Parameter
        /// обычно используется для настройки страницы.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SetViualSettings(); //грузимо візуальні налаштування (фони приток і самої проги)
            StorageFile file;
            string ss="0";
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("Categories.txt");
                ss = await FileIO.ReadTextAsync(file);      //читаємо дані з файлу
            }
            catch (FileNotFoundException)       //якщо виникне виключення що файлу нема
            {
                file = null;                  //топрисвоєму йому null
            } 
            if(file != null && ss != "")                //якшо не null то він існує + якшо в файлі шось є, то ми можемо
                await LoadChanges();       //завантажити параметри
        }

        #region Інші події

        /// <summary>
        /// Видаляємо вибраний Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            if (GV.Items.Count > 0 && GV.SelectedItem != null)
            {
                StorageFile file;
                GridViewItem gvi = (GridViewItem)GV.SelectedItem;
                TextBlock txt = (TextBlock)gvi.Content;
                string FileName = txt.Text + ".txt";    //отримуємо імя файлу, яке дорівнює імені категої, що видаляється
                try
                {
                    file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
                    await file.DeleteAsync();   //видаляємо файл
                }
                catch { }
                GV.Items.Remove(GV.SelectedItem);
                SaveChanges();  //після видалення зберігаємо зміни
            }
            else
                try
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("Categories.txt");
                    await file.DeleteAsync();   //видаляємо файл
                }
                catch { }
        }    
        
        // Перехід на нову сторінку і передача параметрів
        private void CategoryClick(object sender, ItemClickEventArgs e)
        {
            SaveChanges();      //зберігаємо зміни перед відкриванням
            TextBlock txt = (TextBlock)e.ClickedItem;
            string CategoryName = txt.Text;
            //MessageShow(ss);
            this.Frame.Navigate(typeof(TasksDetailPage), CategoryName);
        }
        
        private void GV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (GV.SelectedItem != null)    //якшо є вибраний елемент
            {
                EditSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
                DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible; //то показати нижній аппбар
                DownerAppBar.IsOpen = true;     //і відкрити його
            }
            else
                DownerAppBar.IsOpen = false;
        }

        private void DownerAppBar_Closed(object sender, object e)
        {
            GV.SelectedItem = null;     //коли скривається аппбар вибраних елементів немає бути тобто null
            DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed; //зробити нижній аппбар невидимим коли нема вибраних елементів
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
                //якшо файлу нема то треба зробити налаштування за замоучуванням,
                //для фону проги робити нічого нетреба а для категорій присвоюємо картинку
                categoryBrush = new ImageBrush();              //фон для категорії за замоучуванням
                BitmapImage categoryImage = new BitmapImage();
                categoryImage.UriSource = new Uri("ms-appx:/Assets/back.png", UriKind.Absolute);
                categoryBrush.ImageSource = categoryImage;
                categoryBrush.Stretch = Stretch.Fill;
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
                string fon = "", cat = "";
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
                        case "CatFon":                         //шукаємо Content 
                            cat = val.GetString();   //і присвоюємо його зн. до  GVItem.Content
                            break;
                    }
                }

                //////вибираємо налаштування для фону плиток(категорії)
                if (cat == "За замовчуванням" || cat == "Default" || cat == "По умолчанию")
                {
                    categoryBrush = new ImageBrush();              //фон для категорії за замоучуванням
                    BitmapImage categoryImage = new BitmapImage();
                    categoryImage.UriSource = new Uri("ms-appx:/Assets/back.png", UriKind.Absolute);
                    categoryBrush.ImageSource = categoryImage;
                    categoryBrush.Stretch = Stretch.Fill;

                    Categorycolor = null;   //присвоюємо лоя того щоб відобразити картинку. P.S. ДИВИСЯ ЦИКЛ В НИЗУ
                }
                else if (cat == "Зображення" || cat == "Image" || cat == "Изображение")
                {
                    categoryBrush = new ImageBrush();              //фон для категорії той шо вибрав юзер
                    BitmapImage categoryImage = new BitmapImage();
                    categoryImage.UriSource = new Uri(ApplicationData.Current.LocalFolder.Path + @"\CatFon.png", UriKind.Absolute);
                    categoryBrush.ImageSource = categoryImage;
                    categoryBrush.Stretch = Stretch.Fill;

                    Categorycolor = null;       //присвоюємо лоя того щоб відобразити картинку. P.S. ДИВИСЯ ЦИКЛ В НИЗУ
                }
                else if (cat == "Білий" || cat == "White" || cat == "Белый")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.White);
                    SolidColorBrush black = new SolidColorBrush(Windows.UI.Colors.Black);
                    foreach (GridViewItem gvi in GV.Items)
                    {
                        TextBlock txt = (TextBlock)gvi.Content;
                        txt.Foreground = black;
                    }
                    whiteColor = null;
                }
                else if (cat == "Жовтий" || cat == "Yellow" || cat == "Желтый")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.Yellow);
                }
                else if (cat == "Зелений" || cat == "Green" || cat == "Зеленый")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.Green);
                }
                else if (cat == "Оранджевий" || cat == "Orange" || cat == "Оранжевый")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.Orange);
                }
                else if (cat == "Синій" || cat == "Blue" || cat == "Синий")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.Blue);
                }
                else if (cat == "Фіолетовий" || cat == "Violet" || cat == "Фиолетовый")
                {
                    Color _back = Color.FromArgb(255, 165, 0, 153); //наш фіолетовий
                    Categorycolor = new SolidColorBrush(_back);
                }
                else if (cat == "Чорний" || cat == "Black" || cat == "Черный")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.Black);
                    whiteColor = new SolidColorBrush(Windows.UI.Colors.White);
                    foreach (GridViewItem gvi in GV.Items)
                    {
                        TextBlock txt = (TextBlock)gvi.Content;
                        txt.Foreground = whiteColor;
                    }
                }
                else if (cat == "Червоний" || cat == "Red" || cat == "Красный")
                {
                    Categorycolor = new SolidColorBrush(Windows.UI.Colors.Red);
                }

                //ТОЙ САМИЙ ЦИКЛ
                foreach (GridViewItem gvi in GV.Items)   //задаємо фон для категорії
                    if (Categorycolor != null)
                        gvi.Background = Categorycolor;
                    else
                    {
                        SolidColorBrush black = new SolidColorBrush(Windows.UI.Colors.Black);
                        TextBlock txt = (TextBlock)gvi.Content;
                        txt.Foreground = black;     //присвоюємо тексту чорний колір
                        gvi.Background = categoryBrush;
                    }
                #region Фон проги
                //////вибираємо налаштування для фону проги
                if (fon == "За замовчуванням" || fon == "Default" || fon == "По умолчанию")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 29, 29, 29));
                    SolidColorBrush white = new SolidColorBrush(Windows.UI.Colors.White);
                    AppName.Foreground = white;             //коли фон білий треба поставити тексти і кнопку додати(що є білою) - чорним
                    CategoryText.Foreground = white;
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
                    CategoryText.Foreground = black;
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
                    CategoryText.Foreground = white;
                    AddButton.Foreground = white;
                    Grid1.Background = color;   //робимо фон чорним
                }
                else if (fon == "Червоний" || fon == "Red" || fon == "Красный")
                {
                    SolidColorBrush color = new SolidColorBrush(Windows.UI.Colors.Red);
                    Grid1.Background = color;   //робимо фон
                }
                #endregion
            }
        }
        #endregion

        #endregion

        #region Зчитування параметрів з файлу

        public async void SaveChanges()
        {
            if (GV.Items.Count > 0)     //Якщо є ітеми то зберігаємо 
            {
                string SavedSetting = "[\n";
                List<string> ContentOfGV = new List<string>();      //створюємо лист для контентів ітемів
                foreach (GridViewItem gvi in GV.Items)
                {
                    TextBlock txt = (TextBlock)gvi.Content;
                    ContentOfGV.Add(txt.Text);                     //додаємо в нього дані
                }
                int i = 0;
                for (i = 0; i < ContentOfGV.Count - 1; i++)
                    SavedSetting += "{ \"Content\" : \"" + ContentOfGV[i] + "\" },\n";  //додаємо в строчку настройки в форматі JSON 
                SavedSetting += "{ \"Content\" : \"" + ContentOfGV[i] + "\" } \n ]";    //то саме тільки без коми і з ] вкінці

                try
                {
                    StorageFolder current = ApplicationData.Current.LocalFolder;
                    StorageFile file = await current.CreateFileAsync("Categories.txt", CreationCollisionOption.ReplaceExisting);  //створюємо файл? якшо є то заміняємо
                    await FileIO.WriteTextAsync(file, SavedSetting);        //записуємо налаштування
                }
                catch
                { }//MessageShow("Error"); }
            }
            else                        //якщо ні то видаляємо 
                try
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("Categories.txt");
                    await file.DeleteAsync();   //видаляємо файл
                }
                catch { }
        }

        public async Task LoadChanges()
        {
            GV.Items.Clear();       //очищуємо GV, шоб налаштування ненакладалися на ті шо вже є

            // Відкриваємо та читаємо Settings,txt
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync("Categories.txt");            
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
                TextBlock txt = new TextBlock();
                txt.TextAlignment = TextAlignment.Center;   //параметри кодного текст блоку як ітема
                txt.TextWrapping = TextWrapping.Wrap;

                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;
                    switch (key)
                    {
                        case "Content":                         //шукаємо Content 
                            txt.Text = val.GetString();   //і присвоюємо його зн. до  GVItem.Content
                            break;
                    }
                }
                GridViewItem gvi = new GridViewItem();      //створюємо GridViewItem щоб загрузити в нього задній фон
                gvi.Content = txt;                          //контенту присвоюємо txt
               // gvi.Content += numberOfTasks;

                //коли картинка = 0, значить її нема, тобто юзер вибрав фоном якийсь колір
                if (categoryBrush != null)
                    gvi.Background = categoryBrush;                  //присвоюємо фон
                else
                {
                    gvi.Background = Categorycolor;                  //присвоюємо фон як колір
                    //коли ми ставимо чорний колір як фон не відображається контент категорії
                    //тому при виборі !чорний! ми задаємо в лінійці 240 значення whiteColor
                    //якщо воно не null значить ми його задали і значить колір фону чорним тому
                    //контент робимо білим:-)
                    if (whiteColor != null)
                        gvi.Foreground = whiteColor;
                }
                GV.Items.Add(gvi);      //додавання категорії
            }
        }     
    
        #endregion

        #region Кліки, що обробляють додавання ітема    
    
        /// <summary>
        /// Клік на кнопку додати
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItem(object sender, RoutedEventArgs e)
        {
            SaveButton1.Visibility = Windows.UI.Xaml.Visibility.Collapsed; //робимо невидимою пуршу кнопку
            SaveButton2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //робимо видиму stackPanel шо мітить кнопки і поле для редагування
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //і невидимою 2 що містить кнопки для редагування
            EditSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            // є дві кнопки зберегти 1 для збереження редагування, 2 - для збереження контенту і додавання ітему
            //їм треба почергово міняти видимість взалежності від потреб користувача
            DownerAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            DownerAppBar.IsOpen = true;             //відкриваємо аппбар
        }

        /// <summary>
        /// Клік на кнопку зберегти і додати, яка задає контент
        /// і додає ітем
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSaveButton(object sender, RoutedEventArgs e)
        {
            TextBlock txt = new TextBlock();            //створюємо textblock
            txt.Text = NameFeed.Text;                   //присвоюємо контент
            txt.TextWrapping = TextWrapping.Wrap;       //задаємо властивості
            txt.TextAlignment = TextAlignment.Center;

            GridViewItem gvi = new GridViewItem();
            gvi.Content = txt;

            //коли картинка = 0, значить її нема, тобто юзер вибрав фоном якийсь колір
            if (categoryBrush != null)
                gvi.Background = categoryBrush;                  //присвоюємо фон
            else
            {
                gvi.Background = Categorycolor;                  //присвоюємо фон як колір
                //коли ми ставимо чорний колір як фон не відображається контент категорії
                //тому при виборі !чорний! ми задаємо в лінійці 240 значення whiteColor
                //якщо воно не null значить ми його задали і значить колір фону чорним тому
                //контент робимо білим:-)
                if (whiteColor != null)
                    gvi.Foreground = whiteColor;
            }

            GV.Items.Add(gvi);                          //додаємо в GV

            //після додавання робимо невидимими кнопку 2
            SaveButton2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveButton1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //робимо панель невидимою, щоб небуло помилок,
            //при кліку на кнопку зберегти коли не клікнуто додати чи редагувати
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            EditSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //ховаємо аппбар
            DownerAppBar.IsOpen = false;

            SaveChanges();  //після редаг зберігаємо зміни
        }
        #endregion

        #region Кліки, що обробляють редагування ітема

        /// клік, що зберігає відредаговані дані в SelectedItem в GV
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditSaveButton(object sender, RoutedEventArgs e)
        {
            
            GridViewItem gvi = (GridViewItem)GV.SelectedItem;
            TextBlock txt = (TextBlock)gvi.Content;     //зчитуємо дані вибраного ітема
            //отримуємо імя вибраного ітема(старе імя) потрібне для перейменування файлу, 
            //щоб передати на нову сторінку як параметр

            string OldName = txt.Text + ".txt";
            txt.Text = NameFeed.Text;                       //заміняємо потрібним текстом
            txt.TextWrapping = TextWrapping.Wrap;           //влачтивості 
            txt.TextAlignment = TextAlignment.Center;       //            текстблоку

            gvi.Background = categoryBrush;
            gvi.Content = txt;
            GV.SelectedItem = gvi;                //заміняємо вибраний ітем нашим відредагованим текстблоком

            StorageFile file;           
            string NewName = txt.Text + ".txt";    //отримуємо імя файлу, яке дорівнює імені категої, що видаляється
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(OldName);
                await file.RenameAsync(NewName);   //перейминовуємо
            }
            catch { }

            //робимо панель невидимою, щоб небуло помилок,
            //при кліку на кнопку зберегти коли не клікнуто додати чи редагувати
            ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveChanges();  //після редаг зберігаємо зміни
        }

        /// Редагування вибраного Item
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButtonClick(object sender, RoutedEventArgs e)
        {
            if (GV.SelectedItem != null)
            {
                GridViewItem gvi = (GridViewItem)GV.SelectedItem;
                TextBlock txt = (TextBlock)gvi.Content;   //зчитуємо дані в залежності від вибраного Ітема
                NameFeed.Text = txt.Text;                         //даємо NameFeedу текст вибраного ітема

                SaveButton1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                SaveButton2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                //робимо видиму Stack Panel шо містить поле і кнопку для редагування
                ColapsedSP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        #endregion
                
        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            //коли ми переходимо в налаштування фокус сторінки втрачається,
            //а коли вертаємося виконується дана функці і ми в ній грузимо наші візуальні зміни,
            //які ми робимо в самих налаштуваннях
            SetViualSettings();
        }

    }
}