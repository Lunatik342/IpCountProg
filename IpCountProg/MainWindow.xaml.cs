using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IpCountProg
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    { 
        private int _ipPosition;
        private int _countPosition;

        public MainWindow()
        {
            InitializeComponent();
            byte mask = 255;
            byte[] result = new byte[4];
            for (var i = 3; i >= 0; i--)
            {
                result[i] = 255;
            }
            for (var j = 3; j >= 0; j--)
            {
                for (int i = 0; i < 8; i++)
                {
                    mask = (byte)(mask << 1);
                    result[j] = mask;
                    var msk = new IpMask(result);
                    MaskComboBox.Items.Add(msk);
                }
                mask = 255;
            }
            DisplayIpAdr();
            DisplayMacAdr();
        }

        private void IpTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyNumbers(e.Text);
            var ipTextBox = sender as TextBox;
            if (ipTextBox != null)
                _ipPosition = ipTextBox.CaretIndex;
        }

        private void IpTextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!IsOnlyNumbers(text))
                {
                    e.CancelCommand();
                }
                if (IsNotByte(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static bool IsOnlyNumbers(string text)
        {
            var regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private bool IsNotByte(string number)
        {
            try
            {
                var n = Convert.ToInt32(number);
                return n > 255;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void IpTextBox1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var target = (TextBox)sender;
            if (IsNotByte((target.Text)))
            {
                var text = target.Text;
                target.Text = text.Remove(_ipPosition, 1);
                target.CaretIndex = _ipPosition;
            }
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mask = MaskComboBox.SelectedItem as IpMask;
            if (mask != null && IpTextBox1.Text != string.Empty && IpTextBox2.Text != string.Empty
                && IpTextBox3.Text != string.Empty && IpTextBox4.Text != string.Empty)
            {
                var ipAdress =
                    new MyIp(new[]
                    {
                        Convert.ToByte(IpTextBox1.Text), Convert.ToByte(IpTextBox2.Text),
                        Convert.ToByte(IpTextBox3.Text), Convert.ToByte(IpTextBox4.Text)
                    });
                DisplayData(ipAdress,mask);

            }
        }

        private void DisplayData(MyIp ip,IpMask mask)
        {
            var brAdr = ip.GetBroadcastAdress(mask);
            var netAdr = ip.GetNetworkAdress(mask);
            var l = new ObservableCollection<Representation>
            {
                new Representation { Name = "Ip adress",BinaryValue = ip.BinaryView(), Value = ip.ToString()},
                new Representation { Name = "Mask", BinaryValue = mask.BinaryView(),Value = mask.ToString()},
                new Representation { Name = "Broadcast adress", BinaryValue = brAdr.BinaryView(),Value = brAdr.ToString()},
                new Representation { Name = "Network adress", BinaryValue = netAdr.BinaryView(), Value = netAdr.ToString()},
            };
            IpRepresentationListView.ItemsSource = l;
            IpClassTextBox.Text = ip.GetIpClass();
            IpClassTextBox.IsEnabled = true;
            CountTextBox.Text = mask.GetNetworkCapacity().ToString();
            CountTextBox.IsEnabled = true;

        }

        private void DisplayIpAdr()
        {
            var adresses = MyIp.GetLocalIpAddress();
            var adressesArray = adresses as string[] ?? adresses.ToArray();
            if (adressesArray.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (var adress in adressesArray)
                {
                    sb.Append(adress + Environment.NewLine);
                }
                ComputerIpTextBox.Text = sb.ToString().TrimEnd('\r', '\n');
            }
            else
            {
                ComputerIpTextBox.Text = "нет";
            }
        }

        private void DisplayMacAdr()
        {
            var mac = MyIp.GetMacAddress();
            var physicalAddresses = mac as PhysicalAddress[] ?? mac.ToArray();
            if (physicalAddresses.Any())
            {
                var sb = new StringBuilder();
                foreach (var adress in physicalAddresses)
                {
                    sb.Append(adress + Environment.NewLine);
                }
                ComputerMacTextBox.Text = sb.ToString().TrimEnd('\r', '\n');
            }
            else
            {
                ComputerMacTextBox.Text = "нет";
            }
        }

        private void PcCountTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var target = (TextBox)sender;
            if (target.Text == string.Empty)
                return;
            target.Text = target.Text.Replace("  ", string.Empty);
            var n = Convert.ToInt64(target.Text);
            if (n > (double)int.MaxValue + 2)
            {
                var text = target.Text;
                target.Text = text.Remove(_countPosition, 1);
                target.CaretIndex = _countPosition;
            }

        }

        private void PcCountTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyNumbers(e.Text);
            var ipTextBox = sender as TextBox;
            if (ipTextBox != null)
                _countPosition = ipTextBox.CaretIndex;
        }

        private void PcCountTextBox_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (text == string.Empty)
                    return;
                if (!IsOnlyNumbers(text))
                {
                    e.CancelCommand();
                    return;
                }
                if (text != null)
                {
                    text = text.Replace("  ", string.Empty);
                    long n = Convert.ToInt64(text);
                    if (n > (double)int.MaxValue + 2)
                        e.CancelCommand();
                }
                else
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void CountButton_OnClick(object sender, RoutedEventArgs e)
        {
            var text = PcCountTextBox.Text;
            var mask = MyIp.CountMaskFromNumber(text);
            CountMaskTextBox.Text = "Маска: " + mask;
        }
    }
    public class Representation
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string BinaryValue { get; set; }
    }
}
