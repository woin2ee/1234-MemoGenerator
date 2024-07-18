using MemoGenerator.Model.MemoGenerating;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.Text;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MemoGenerator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        StackPanel[] panels;

        private string identifyingComponent = "";
        private string paymentProofMethodComponent = "";
        private string documentsDeliveryRouteComponent = "";
        private string emailComponent = "";

        private TaxCalculatingModel taxCalculatingModel = new TaxCalculatingModel();
        private PaymentProofModel paymentProofModel = new PaymentProofModel();

        public MainWindow()
        {
            this.InitializeComponent();

            panels = new StackPanel[] { memoGeneratorPanel, taxCalculatorPanel };
            foreach (var panel in panels)
            {
                panel.Visibility = Visibility.Collapsed;
            }
            panels[0].Visibility = Visibility.Visible;

            dateErrorTextBox.Visibility = Visibility.Collapsed;
            invoiceDateErrorTextBox.Visibility = Visibility.Collapsed;

            var appWindow = WindowUtil.GetAppWindow(this);
            appWindow.Title = "시간을 아껴줘용";
            WindowUtil.CenterToScreen(this);
            appWindow.Resize(new SizeInt32 { Width = 1000, Height = 620 }); // 창 크기
            ((OverlappedPresenter)appWindow.Presenter).IsAlwaysOnTop = false;
            ((OverlappedPresenter)appWindow.Presenter).IsMaximizable = false; // 최대화 가능 여부
            ((OverlappedPresenter)appWindow.Presenter).IsResizable = false;
            //((OverlappedPresenter)appWindow.Presenter).Maximize(); // 실행 시 창이 최대화 상태로 나타남
            appWindow.SetPresenter(AppWindowPresenterKind.Default); // 화면 형태 설정

            // <a href="https://www.flaticon.com/kr/free-icons/" title="유용 아이콘">유용 아이콘 제작자: Maan Icons - Flaticon</a>
            this.AppWindow.SetIcon("C:\\Users\\y\\Desktop\\Visual Studio\\Projects\\MemoGenerator\\MemoGenerator\\Assets\\default-icon.ico");

            taxCalculatorPanel.DataContext = taxCalculatingModel;
            paymentProofStackPanel.DataContext = paymentProofModel;

            disableDeductionGroup();
        }

        private void changePanel(object sender, RoutedEventArgs e)
        {
            Button transitionButton = sender as Button;

            foreach (var panel in panels)
            {
                panel.Visibility = Visibility.Collapsed;
            }

            int tag = int.Parse(transitionButton.Tag.ToString());
            panels[tag].Visibility = Visibility.Visible;
        }

        private void updateMemoTextBlock()
        {
            const char componentPrefix = '[';
            const char componentSuffix = ']';

            List<String> components = new List<string>();
            if (!String.IsNullOrEmpty(identifyingComponent))
            {
                components.Add($"{componentPrefix}{identifyingComponent}{componentSuffix}");
            }
            if (!String.IsNullOrEmpty(paymentProofMethodComponent))
            {
                components.Add($"{componentPrefix}{paymentProofMethodComponent}{componentSuffix}");
            }
            if (!String.IsNullOrEmpty(documentsDeliveryRouteComponent))
            {
                components.Add($"{componentPrefix}{documentsDeliveryRouteComponent}{componentSuffix}");
            }

            string text = String.Join("-", components);

            if (!String.IsNullOrEmpty(emailComponent))
            {
                text += $" {emailComponent}";
            }

            memoTextBox.Text = text;
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(memoTextBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void updateIdentifyingComponent(object sender, TextChangedEventArgs e)
        {
            dateErrorTextBox.Visibility = Visibility.Collapsed;
            List<String> elements = new List<string>();

            DateTime date;
            if (DateTime.TryParseExact(dateTextBox.Text, "yyMMdd", null, System.Globalization.DateTimeStyles.None, out date))
            {
                elements.Add(date.ToString("yy'/'MM'/'dd"));
            }
            else if (DateTime.TryParseExact(dateTextBox.Text, "MMdd", null, System.Globalization.DateTimeStyles.None, out date))
            {
                elements.Add(date.ToString("MM'/'dd"));
            }
            else if (!String.IsNullOrEmpty(dateTextBox.Text))
            {
                dateErrorTextBox.Visibility = Visibility.Visible;
            }

            if (Int32.TryParse(amountTextBox.Text, out int amount) && amount != 0)
            {
                elements.Add(amount.ToString("C"));
            }

            identifyingComponent = String.Join(" ", elements);
            updateMemoTextBlock();
        }

        private void updatePaymentProofMethodComponent(object sender, RoutedEventArgs e)
        {
            // 함께사는 세상 옵션 추가
            // 분할 발행 기능

            paymentProofMethodComponent = paymentProofModel.memoComponent;
            updateMemoTextBlock();
        }

        private void updateDocumentsDeliveryRouteComponent(object sender, RoutedEventArgs e)
        {
            List<String> elements = new List<string>();

            string documentList = "";
            if (!String.IsNullOrEmpty(documentListTextBox.Text))
            {
                documentList = $"({documentListTextBox.Text})";
            }

            if (sendingEmailCheckBox.IsChecked == true && sendingRegisteredCheckBox.IsChecked == true)
            {
                elements.Add($"서류{documentList} 메일/등기");
            }
            else if (sendingEmailCheckBox.IsChecked == true)
            {
                elements.Add($"서류{documentList} 메일");
            }
            else if (sendingRegisteredCheckBox.IsChecked == true)
            {
                elements.Add($"서류{documentList} 등기");
            }

            if (elements.Count > 0)
            {
                elements.Add("발송");
            }

            documentsDeliveryRouteComponent = String.Join(" ", elements);
            updateMemoTextBlock();
        }

        private void updateEmailComponent(object sender, RoutedEventArgs e)
        {
            emailComponent = emailTextBox.Text;
            updateMemoTextBlock();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            taxCalculatingModel.initializeItemInfos();
            taxCalculatingModel.propertyChanged(null);
            disableDeductionGroup();
        }

        private void deductionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (deductionCheckBox.IsChecked == true) { enableDeductionGroup(); }
            else { disableDeductionGroup(); }
        }

        private void disableDeductionGroup()
        {
            deductionCheckBox.IsChecked = false;

            deductedTotalAmountTextBox.IsEnabled = false;
            deductingTargetComboBox.IsEnabled = false;
            deductionRateTextBox.IsEnabled = false;
            List<Control> extraItemStackPanelControls = new List<Control>();
            retrieveAllChildControls(extraItemStackPanel, extraItemStackPanelControls);
            foreach (var control in extraItemStackPanelControls)
            {
                control.IsEnabled = false;
            }

            extraItemTitleTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            deductedTotalAmountTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            deductionRateTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            deductingTargetTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);

            totalAmountStrikethrough.Visibility = Visibility.Collapsed;
            totalAmountTextBox.Background = new SolidColorBrush(Colors.Orange);

            itemsStrikethrough.Visibility = Visibility.Collapsed;
        }

        private void enableDeductionGroup()
        {
            deductionCheckBox.IsChecked = true;

            deductedTotalAmountTextBox.IsEnabled = true;
            deductingTargetComboBox.IsEnabled = true;
            deductionRateTextBox.IsEnabled = true;
            List<Control> extraItemStackPanelControls = new List<Control>();
            retrieveAllChildControls(extraItemStackPanel, extraItemStackPanelControls);
            foreach (var control in extraItemStackPanelControls)
            {
                control.IsEnabled = true;
            }

            extraItemTitleTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            deductedTotalAmountTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            deductionRateTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            deductingTargetTextBlock.Foreground = new SolidColorBrush(Colors.Black);

            totalAmountStrikethrough.Visibility = Visibility.Visible;
            totalAmountTextBox.Background = new SolidColorBrush(Colors.PapayaWhip);

            placeItemsStrikethrough();
        }

        private void placeItemsStrikethrough()
        {
            itemsStrikethrough.Visibility = Visibility.Visible;
            const int firstTop = 41;
            const int topToMiddle = 15;
            const int middleToBottom = 16;
            const int spacing = 13;
            int rowNumber = taxCalculatingModel.deductingRow + 1;
            int positionY = firstTop + (topToMiddle * rowNumber) + (middleToBottom * (rowNumber - 1)) + (spacing * (rowNumber - 1));
            Canvas.SetTop(itemsStrikethrough, positionY);
        }

        private void retrieveAllChildControls(Panel panel, in List<Control> store)
        {
            if (panel.Children.Count == 0) { return; }
            foreach (var element in panel.Children)
            {
                if (element is Panel)
                {
                    retrieveAllChildControls((Panel)element, store);
                }
                if (element is Control)
                {
                    store.Add((Control)element);
                }
            }
        }

        private void deductingTargetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            placeItemsStrikethrough();
        }
    }
}
