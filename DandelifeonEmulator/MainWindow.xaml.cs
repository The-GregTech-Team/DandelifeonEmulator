using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DandelifeonEmulator.Blocks;

namespace DandelifeonEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Controller Controller { get; set; }
        private static readonly Color White = Color.FromRgb(0xFF, 0xFF, 0xFF);
        private static readonly Color Black = Color.FromRgb(0x50, 0x50, 0x50);
        private static readonly Color Yellow = Color.FromRgb(255, 221, 20);
        private static readonly Color Purple = Color.FromRgb(0xFF, 0x45, 0xC4);
        private static readonly Color PurpleLight = Color.FromRgb(20, 0, 235);
        //private static readonly Color PurpleLight = Color.FromArgb(74, 255, 69, 196);

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            // 初始化方块控制器
            Controller = new Controller();

            // 初始化方块列表
            Controller.Rects.Clear();
            for (var i = 0; i < 25; i++) Controller.Rects.Add(new Rectangle[25]);
            Controller.Blocks.Clear();
            for (var i = 0; i < 25; i++) Controller.Blocks.Add(new IBlock[25]);

            // 初始化网格
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            for (var i = 0; i < 25; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            }

            // 初始化所有方块
            MainGrid.Children.Clear();
            for (var i = 0; i < 25; i++)
            {
                for (var j = 0; j < 25; j++)
                {
                    var rect = new Rectangle
                    {
                        Height = 20,
                        Width = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Fill = new SolidColorBrush(White),
                        Stroke = new SolidColorBrush(Purple)
                    };
                    MainGrid.Children.Add(rect);
                    // 设置列
                    Grid.SetColumn(rect, i);
                    // 设置行
                    Grid.SetRow(rect, j);
                    // 设置方块列表
                    Controller.Rects[i][j] = rect;
                    var block = new Air { X = i, Y = j };
                    Controller.UpdateBlock(block);
                    rect.ToolTip = block.Name;
                    // 鼠标高亮
                    rect.MouseEnter += (sender, args) =>
                    {
                        ((Rectangle)sender).Stroke = new SolidColorBrush(PurpleLight);
                    };
                    rect.MouseLeave += (sender, args) =>
                    {
                        ((Rectangle)sender).Stroke = new SolidColorBrush(Purple);
                    };
                    rect.MouseDown += (sender, args) =>
                    {

                        var thisblock = Controller.GetBlockFromRect((Rectangle)sender);
                        for (var i1 = 11; i1 <= 13; i1++)
                        {
                            for (var k = 11; k <= 13; k++)
                            {
                                if (thisblock.X == i1 && thisblock.Y == k)
                                {
                                    return;
                                }
                            }

                        }

                        var num = ((int)thisblock.BlockType + 1) % 3;
                        var newblock = Controller.ToBlock((BlockType)num, thisblock);
                        Controller.UpdateBlock(newblock);
                        Controller.UpdateRect(newblock);
                    };

                }
            }


            Controller.InitCenter();

            // 初始化文字
            _step = 0;
            Step.Text = "0";
            Cells.Text = "0";
            Mana.Text = "0";
            ChangeModeButton.IsChecked = false;
            Controller.manaMax = 50000;
        }

        

        private void Run(object sender, RoutedEventArgs e)
        {
            var count = Controller.Blocks.SelectMany(controllerBlock => controllerBlock).OfType<Cell>().Count();
            Cells.Text = count.ToString();

            Controller.Run();
            _step++;
            Step.Text = _step.ToString();
            Mana.Text = Controller.Mana.ToString();
        }

        private bool _running;
        private int _step;
        private async void RunToEnd(object sender, RoutedEventArgs e)
        {
            // 重置
            _step = 0;
            Controller.Mana = 0;
            // 细胞总数
            var count = Controller.Blocks.SelectMany(controllerBlock => controllerBlock).OfType<Cell>().Count();
            Cells.Text = count.ToString();
            // 保存
            Save(null, null);

            _running = true;
            while (!Controller.Isfinal && _running)
            {
                Controller.Run();
                _step++;
                Step.Text = _step.ToString();
                Mana.Text = Controller.Mana.ToString();
                await Task.Delay(100);
            }
            Controller.Isfinal = false;
            _running = false;
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            _running = false;
            Init();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _running = false;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(Controller.Save());
            try
            {
                SaveTextBox.Text = Controller.Save();
            }
            catch (Exception exception)
            {
                MainSnackbar.MessageQueue.Enqueue(exception.Message);
            }
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            try
            {
                Controller.Load(SaveTextBox.Text);

            }
            catch (Exception exception)
            {
                MainSnackbar.MessageQueue.Enqueue(exception.Message);
            }
        }

        private void OrigindMode(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton) sender;
            if (button.IsChecked.Value)
            {
                Controller.ManaPerGen = 3750;
                Controller.manaMax = 500000;
            }
            else
            {
                Controller.ManaPerGen = 150;
                Controller.manaMax = 50000;

            }

        }
    }
}
