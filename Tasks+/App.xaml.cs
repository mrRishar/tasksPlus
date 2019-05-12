using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Callisto.Controls;
using Windows.UI;
using Windows.UI.ApplicationSettings;

// Шаблон пустого приложения задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234227

namespace Tasks_
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        private Color _back = Color.FromArgb(255, 165, 0, 153);
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// если приложение запускается для открытия конкретного файла, отображения
        /// результатов поиска и т. д.
        /// </summary>
        /// <param name="args">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние за ранее приостановленного приложения
                }

                //меню налаштувань і про прогу
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Если стек навигации не восстанавливается для перехода к первой странице,
                // настройка новой страницы путем передачи необходимой информации в качестве параметра
                // навигации
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Обеспечение активности текущего окна
            Window.Current.Activate();            
            
        }

        void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            string aboutText = "Про програму", preftext = "Налаштування";
            string[] lang = Windows.Globalization.ApplicationLanguages.Languages.ToArray<string>();
            if (lang[0].Contains("en"))
            {
                aboutText = "About";
                preftext = "Preferences";
            }
            else if(lang[0].Contains("ru"))
            {
                aboutText = "О программе";
                preftext = "Настройки";
            }

            // Add an About command
            var about = new SettingsCommand("about", aboutText, (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new AboutUserControl();
                settings.HeaderBrush = new SolidColorBrush(_back);
                settings.ContentBackgroundBrush = new SolidColorBrush(_back);
                settings.HeaderText = aboutText;
                settings.IsOpen = true;
            });

            args.Request.ApplicationCommands.Add(about);

            // Add a Preferences command
            var preferences = new SettingsCommand("preferences", preftext, (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new PreferencesUserControl();
                settings.HeaderBrush = new SolidColorBrush(_back);
                settings.ContentBackgroundBrush = new SolidColorBrush(_back);
                settings.HeaderText = preftext;
                settings.IsOpen = true;
            });
            args.Request.ApplicationCommands.Add(preferences);
        }
              

        /// <summary>
        /// Вызывается при приостановке выполнения приложения. Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }
}
