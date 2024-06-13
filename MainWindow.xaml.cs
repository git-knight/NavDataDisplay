using ExcelLibrary.SpreadSheet;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NavDataDisplay
{
    //class 

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rand = new Random();

        public ObservableCollection<NavDataFile> LogsList { get; set; } = new ObservableCollection<NavDataFile>();
        public ObservableCollection<string> DatesList { get; set; } = new ObservableCollection<string>();
        Dictionary<string, NavDataFile> Logs = new();
        DateRange Dt = new();
        DateRange ViewRange = new();
        DateRange ViewBorders = new();
        bool ViewingDist = false;
        int ViewDistFrom = 0, ViewDistTo = 0;
        int ViewDistLen, ViewDistStep = 125;
        DateTime dtCurr;

        int currPar = 0;

        Canvas CanvasCurr => tabs.SelectedIndex == 0 ? graphDraw1 : graphDraw2;

        double[] valMinPer = { 970, -10 };
        double[] valMaxPer = { 1002, 280 };
        double valMin = 960, valMax = 1002;
        int nStepsY = 30;

        int CurrentSelectedParam => currPar;

        double TotalGraphWidth = 2500;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            gridView1.Width = TotalGraphWidth;  

            aaaaaaaaa.ItemsSource = LogsList;

            DateStart.SelectedDate = new DateTime(2023, 10, 30);
            ViewRange = new DateRange(DateStart.SelectedDate.Value, DateStart.SelectedDate.Value + TimeSpan.FromDays(1));
            //LoadDataFile("AF_sensor_log.txt");
            //SaveEntries("AF_sensor_log.txt");
            //graphRanges[0] = new[] { -100.0, 100 };

            //LoadDataFile("AF_sensor_log.txt");
            //LoadDataFile("AF_sensor_log2.txt");
            UpdateViewBorders();
            ViewRange = ViewBorders;
            ViewDistFrom = 0;
            ViewDistTo = ViewDistLen;

            mapViewer.Source = new Uri(Directory.GetCurrentDirectory() + "/" + "map.html");
            mapViewer.Loaded += MapViewer_Loaded;
            //A();
            return;

        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            //var a = JsonSerializer.Deserialize<int>(e.WebMessageAsJson);
            if (int.TryParse(new string(e.WebMessageAsJson.Skip(1).TakeWhile(x=>char.IsDigit(x)).ToArray()), out var id))
                DrawPointInfoById(id);
        }

        private void MapViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //mapViewer.ExecuteScriptAsync(")
        }

        private async void mapViewer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void mapViewer_Initialized(object sender, EventArgs e)
        {



        }
        float scale = 1;

        

        void DrawYAxis()
        {
            cvYvalues.Children.Clear();


            var yHeight = gridView1.ActualHeight;
            if (yHeight == 0) return;

            var xLine = new Line()
            {
                Stroke = Brushes.Black,

                X1 = 15,
                X2 = 15,  // 150 too far
                Y1 = 0,
                Y2 = yHeight
            };

            var yPrev = -1000.0;
            //var yPerStep = 5;
            for (var iy = 0; iy < nStepsY; iy++)
            {
                var y = yHeight / nStepsY * iy;
                var myLine1 = new Line
                {
                    Stroke = (iy % 5 == 4) ? Brushes.LightGray : Brushes.Gainsboro,

                    X1 = 0,
                    X2 = 10000,
                    Y1 = y,
                    Y2 = y,

                    StrokeThickness = 1
                };
                graphDraw1.Children.Add(myLine1);

                //var yValue=

                if (y - yPrev > 30)
                {
                    var myLine2 = new Line
                    {
                        Stroke = Brushes.Gray,

                        X1 = 11,
                        X2 = 19,
                        Y1 = y,
                        Y2 = y,

                        StrokeThickness = 1
                    };
                    graphDraw1.Children.Add(myLine2);
                    var b = new TextBlock
                    {
                        Text = "" + $"{((valMax - valMin) / nStepsY * iy + valMin):F3}",
                        TextAlignment = TextAlignment.Center,
                        FontSize = 10.6,
                        Foreground = Brushes.Gray,
                    };
                    Canvas.SetLeft(b, 21);
                    Canvas.SetTop(b, y - 10);
                    cvYvalues.Children.Add(b);
                    yPrev = y;
                }
            }

            cvYvalues.Children.Add(xLine);
        }

        void DrawXAxis()
        {
            graphDraw1.Children.Clear();
            graphDrawXline.Children.Clear();
            DrawYAxis();

            var yT = graphDrawXline.Height - 10;
            var xLine = new Line()
            {
                Stroke = Brushes.Black,

                X1 = 0,
                X2 = 100000,  // 150 too far
                Y1 = yT,
                Y2 = yT
            };
            graphDrawXline.Children.Add(xLine);

            var step = ViewRange.Length / steps;
            for (var i = 0; i < steps; i++)
            {
                Line myLine1 = new Line
                {
                    Stroke = (i % 5 == 4) ? Brushes.LightGray : Brushes.Gainsboro,

                    X1 = i * 25 + 5,
                    X2 = i * 25 + 5,  // 150 too far
                    Y1 = 0,
                    Y2 = 1000,

                    StrokeThickness = 1
                };

                graphDraw1.Children.Add(myLine1);

                //var b = new TextBlock
                //{
                //    Text = "" + (i),
                //    TextAlignment = TextAlignment.Center,
                //};
                //Canvas.SetLeft(b, i * 25 + 5);
                //Canvas.SetTop(b, yT + 15);
                //graphDrawXline.Children.Add(b);

                var nFreq = 3;

                if (i % nFreq == 0)
                {
                    Line xMark = new Line
                    {
                        Stroke =Brushes.Black,

                        X1 = i * 25 + 5,
                        X2 = i * 25 + 5,  // 150 too far
                        Y1 = yT-5,
                        Y2 = yT+5,

                        StrokeThickness = 0.5
                    };

                    graphDrawXline.Children.Add(xMark);

                    var dtCurrent = ViewRange.Min + step * i;
                    var b1 = new TextBlock
                    {
                        Text = (ViewingDist) ? $"{i * ViewDistStep}" : dtCurrent.ToString("T"),
                        TextAlignment = TextAlignment.Center,
                        FontSize = 10.6,
                        Foreground = Brushes.Gray,
                    };
                    Canvas.SetLeft(b1, i * 25 + 5);
                    Canvas.SetTop(b1, yT - 18);

                    b1.RenderTransform = new RotateTransform(-22, 0, 1);

                    graphDrawXline.Children.Add(b1);
                }


            }






        }

        void SaveEntries(string path)
        {
            var entries = Logs[path];

            var tbl = new Workbook();
            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                var sheet = new Worksheet($"{dataFile.FileName}");
                tbl.Worksheets.Add(sheet);
                sheet.Cells[0, 0] = new Cell("Время");
                sheet.Cells[0, 1] = new Cell("Широта");
                sheet.Cells[0, 2] = new Cell("Долгота");
                sheet.Cells[0, 3] = new Cell("Давление");
                sheet.Cells[0, 4] = new Cell("Скорость");
                sheet.Cells[0, 5] = new Cell("Угол");

                var e = 1;
                foreach (var entry in data)
                {
                    //DateTime x;
                    //x.ToString("dd:FFFF");
                    sheet.Cells[e, 0] = new Cell(entry.Time.ToString("G"));
                    sheet.Cells[e, 1] = new Cell(entry.Lat, "#,#######0.0000000");
                    sheet.Cells[e, 2] = new Cell(entry.Lon, "#,#######0.0000000");
                    sheet.Cells[e, 3] = new Cell(entry.Atm, "#,####0.0000");
                    sheet.Cells[e, 4] = new Cell(entry.Speed, "#,####0.0000");
                    sheet.Cells[e, 5] = new Cell(entry.Angle, "#,##0.00");
                    e++;
                }
            }
            tbl.Save("export/data.xls");

        }

        void LoadDataFile(string path)
        {
            path = path.ToLower();
            if (Logs.Keys.Contains(path))
                return;

            var dtFile = new NavDataFile(path);

            Logs.Add(path, dtFile);
            LogsList.Add(dtFile);
            Dt.Update(dtFile.DataRange.Min);
            Dt.Update(dtFile.DataRange.Max);
            foreach(var dt in dtFile.Data.Keys)
            {
                var dtstr = dt.ToString("dd.M.yyyy");
                if (!DatesList.Contains(dtstr))
                    DatesList.Add(dtstr);
                dateaaaaaaaaa.ItemsSource = DatesList;
            }

            return;
        }

        struct PointerInfo
        {
            public List<NavDataEntry> Data { get; set; }
            public Ellipse Ellipse { get; set; }
            public TextBlock Text { get; set; }
        }
        List<PointerInfo> ellipses=new List<PointerInfo>();

        const int steps = 240;
        const double xmin = 30;
        const double ymin = 30;
        const double ymax = 400;
        const double graphYoffsetTop = 15;
        const double graphYoffsetBottom = 15;

        double ValueToGraphY(double value)
            => (value - valMin) / (valMax - valMin) * (gridView1.ActualHeight);// - graphYoffsetTop - graphYoffsetBottom);

        void UpdateViewBorders()
        {
            ViewDistTo = 1;
            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                ViewBorders = ViewBorders.Combine(dataFile.DataRange);
                ViewDistTo = Math.Max(ViewDistTo, dataFile.MaxDistance);
            }
            
        }

        double Lerp(double a, double b, double x) => a + (b - a) * x;

        void DrawDistGraph()
        {
            var xmax = GraphWidth - 15;

            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);

            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                var valPrev = 0.0;
                for (var iStep = 0; iStep < steps; iStep++)
                {
                    var distCurr = ViewDistStep * iStep;

                    var i1 = data.FindIndex(x => x.Distance > distCurr);
                    var val = data[i1].GetValueByGraphNumber(CurrentSelectedParam);
                    if (data[i1].Distance != distCurr)
                    {
                        val = Lerp(data[i1 - 1].GetValueByGraphNumber(CurrentSelectedParam), val,
                            (distCurr - data[i1 - 1].Distance) / (data[i1].Distance - data[i1 - 1].Distance));
                    }

                    if (iStep != 0)
                    {
                        Line myLineMid = new Line
                        {
                            Stroke = dataFile.Color,
                            StrokeThickness = 1,

                            X1 = xmin + (xmax - xmin) * (iStep - 1) / steps,
                            X2 = xmin + (xmax - xmin) * iStep / steps,  // 150 too far
                            Y1 = ValueToGraphY(valPrev),
                            Y2 = ValueToGraphY(val)
                        };
                        graphDraw1.Children.Add(myLineMid);
                    }

                    valPrev = val;

                }

                var pointerCircle = new Ellipse
                {
                    Fill = Brushes.Gray,
                    Width = 5,
                    Height = 5,
                };
                graphDraw1.Children.Add(pointerCircle);
                Canvas.SetLeft(pointerCircle, -100);

                var pointerText = new TextBlock
                {
                };
                graphDraw1.Children.Add(pointerText);
                Canvas.SetLeft(pointerText, -100);

                ellipses.Add(new PointerInfo
                {
                    Data = data,
                    Ellipse = pointerCircle,
                    Text = pointerText,
                });
            }
        }

        bool k = false;
        void RedrawGraph()
        {
            if (k == false && IsLoaded)
            {
                k = true;
                mapViewer.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            }
            if (cb_yaling.IsChecked == true)
                AlignY();

            var xmax = GraphWidth - 15;

            graphDraw1.Children.Clear();

            ellipses.Clear();

            if (LogsList.Count == 0)
                return;

            FixRangeOutOfBounds();


            dtFrom.Content = ViewRange.Min.ToString("G");
            dtTo.Content = ViewRange.Max.ToString("G");

            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            //var DtDataOnly = new DateRange(
            //    ViewRange.min
                //DateOnly.FromDateTime(Dt.Min).ToDateTime(TimeOnly.MinValue), 
                //DateOnly.FromDateTime(Dt.Max + TimeSpan.FromDays(1)).ToDateTime(TimeOnly.MinValue)
            //    );
            var step = ViewRange.Length / steps;


            var yHeight = gridView1.ActualHeight - graphYoffsetTop - graphYoffsetBottom;
            if (yHeight <= 0) return;

            DrawXAxis();

            if (ViewingDist)
            {
                DrawDistGraph();
                return;
            }

            foreach (var dataFile in LogsList.Where(x => x.Selected)) 
            {
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                /*var dtLast = DtDataOnly.Min;
                var dtRangeFirst = new DateRange(dtLast, dtLast + step);
                var allPointsFirst = data.Where(x => dtRangeFirst.Includes(x.Time)).ToArray();

                var vMinLast = allPointsFirst.Min(x => x.Speed);
                var vMaxLast = allPointsFirst.Max(x => x.Speed);
                var vMidLast = allPointsFirst.Sum(x => x.Speed) / allPointsFirst.Length;
                */

                for (var iStep = 1; iStep < steps; iStep++)  
                {
                    var dtCurrent = ViewRange.Min + step * iStep;
                    var dtRange = new DateRange(dtCurrent, dtCurrent + step);

                    //if (!dataFile.DataRange.Includes(dtLast, step))
                    //    continue;

                    //for (int data_set = 0; data_set < 1; data_set++)
                    {
                        var allPoints = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                        if (allPoints.Count() == 0)
                            continue;

                        var vMin = ValueToGraphY((allPoints.Min(x => x.GetValueByGraphNumber(CurrentSelectedParam))));
                        var vMax = ValueToGraphY(allPoints.Max(x => x.GetValueByGraphNumber(CurrentSelectedParam)));
                        var vMid = ValueToGraphY(allPoints.Sum(x => x.GetValueByGraphNumber(CurrentSelectedParam)) / allPoints.Length);
                        
                        dtRange = new DateRange(dtCurrent - step, dtCurrent);
                        var allPointsLast = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                        if (allPointsLast.Count() == 0)
                        {
                            // draw circle?
                        }
                        else
                        {

                            var vMinLast = ValueToGraphY(allPointsLast.Min(x => x.GetValueByGraphNumber(CurrentSelectedParam)));
                            var vMaxLast = ValueToGraphY(allPointsLast.Max(x => x.GetValueByGraphNumber(CurrentSelectedParam)));
                            var vMidLast = ValueToGraphY(allPointsLast.Sum(x => x.GetValueByGraphNumber(CurrentSelectedParam)) / allPointsLast.Length);
                            //vMinLast = 200 - vMinLast;
                            //vMaxLast = 200 - vMaxLast;
                            //vMidLast = 200 - vMidLast;

                            

                            Line myLineMid = new Line
                            {
                                Stroke = dataFile.Color,
                                StrokeThickness = 1,

                                X1 = xmin + (xmax - xmin) * (iStep - 1) / steps,
                                X2 = xmin + (xmax - xmin) * iStep / steps,  // 150 too far
                                Y1 = vMidLast,
                                Y2 = vMid
                            };
                            graphDraw1.Children.Add(myLineMid);
                            vMidLast = vMid;

                            Line myLineMin = new Line
                            {
                                Stroke = dataFile.Color,
                                Opacity = 0.7,
                                StrokeThickness = 1,

                                X1 = xmin + (xmax - xmin) * (iStep - 1) / steps,
                                X2 = xmin + (xmax - xmin) * iStep / steps,  // 150 too far
                                Y1 = vMinLast,
                                Y2 = vMin
                            };
                            graphDraw1.Children.Add(myLineMin);
                            vMinLast = vMin;

                            Line myLineMax = new Line
                            {
                                Stroke = dataFile.Color,
                                Opacity = 0.7,
                                StrokeThickness = 1,

                                X1 = xmin + (xmax - xmin) * (iStep - 1) / steps,
                                X2 = xmin + (xmax - xmin) * iStep / steps,  // 150 too far
                                Y1 = vMaxLast ,
                                Y2 =  vMax 
                            };
                            graphDraw1.Children.Add(myLineMax);
                        }

                    }

                }

                var pointerCircle = new Ellipse
                {
                    Fill = Brushes.Gray,
                    Width = 5,
                    Height = 5,
                };
                graphDraw1.Children.Add(pointerCircle);
                Canvas.SetLeft(pointerCircle, -100);

                var pointerText = new TextBlock
                {
                };
                graphDraw1.Children.Add(pointerText);
                Canvas.SetLeft(pointerText, -100);

                ellipses.Add(new PointerInfo
                {
                    Data = data,
                    Ellipse = pointerCircle,
                    Text = pointerText,
                });
            }

        }

        private void Map_Plus_Click(object sender, RoutedEventArgs e)
        {
            ViewRange = new DateRange(ViewRange.Min + ViewRange.Length * 0.25, ViewRange.Max - ViewRange.Length * 0.25);
            RedrawGraph();

            Map_Redraw_Click(null, null);

        }

        private void Map_Minus_Click(object sender, RoutedEventArgs e)
        {
            ViewRange = new DateRange(ViewRange.Min - ViewRange.Length * 0.25, ViewRange.Max + ViewRange.Length * 0.25);
            FixRangeOutOfBounds();
            RedrawGraph();
            Map_Redraw_Click(null, null);
        }

        // Сохранить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("export");
            if (Logs.Keys.Any())
                SaveEntries(Logs.Keys.First());
        }

        // Добавить файл
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "txt файлы данных|*.txt";
            //openFileDialog.InitialDirectory = Path.GetDirectoryName()
            if (openFileDialog.ShowDialog() ?? false)
            {
                //A();
                foreach (var file in openFileDialog.FileNames)
                {
                    LoadDataFile(file);
                }
                UpdateViewBorders();
                ViewRange = ViewBorders;
                RedrawGraph();
            }

        }

        void FixDistRangeOutOfBounds()
        {
            if (ViewDistTo - ViewDistFrom > ViewDistLen)
            {
                ViewDistFrom = 0;
                ViewDistTo = ViewDistLen;
            }
        }

        void FixRangeOutOfBounds()
        {
            if (LogsList.Count == 0)
                return;

            var from = false ? ViewBorders.Min : DateStart.SelectedDate!.Value;
            var to = false ? ViewBorders.Max : from + TimeSpan.FromDays(1);
            if (ViewRange.Length > (to - from)) 
                ViewRange = ViewBorders;
            else if (ViewRange.Max > to)
            {
                var r = ViewRange.Max - to;
                ViewRange.Min -= r;
                ViewRange.Max -= r;
            }
            else if (ViewRange.Min < from)
            {
                var r = from - ViewRange.Min;
                ViewRange.Min += r;
                ViewRange.Max += r;
            }

            FixDistRangeOutOfBounds();
        }

        private void Map_Move_Right(object sender, RoutedEventArgs e)
        {
            var step = ViewRange.Length * 0.4;
            ViewRange = new DateRange(ViewRange.Min + step, ViewRange.Max + step);
            FixRangeOutOfBounds();
            RedrawGraph();
        }

        private void Map_Move_Left(object sender, RoutedEventArgs e)
        {
            var step = ViewRange.Length * 0.4;
            ViewRange = new DateRange(ViewRange.Min - step, ViewRange.Max - step);
            FixRangeOutOfBounds();
            RedrawGraph();
        }

        private void DateStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewRange = new DateRange(DateStart.SelectedDate!.Value, DateStart.SelectedDate!.Value + TimeSpan.FromDays(1));
            RedrawGraph();
        }

        private void Map_Redraw_Click(object sender, RoutedEventArgs e)
        {
            ViewingDist = false;
            RedrawGraph();
            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            var step = ViewRange.Length / steps;
            var script = "resetArrows();\n";
            NavDataEntry lastPt = null;
            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                var coords = new List<NavDataEntry>();

                for (var iStep = 0; iStep < steps; iStep++)
                {
                    var dtCurrent = ViewRange.Min + step * iStep;
                    var dtRange = new DateRange(dtCurrent, dtCurrent + step);

                    var allPoints = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                    if (allPoints.Count() == 0)
                        continue;

                    coords.Add(allPoints.Last());
                    lastPt = coords.Last();
                }
                if (coords.Count > 1) {
                    script += "startPath();\n";
                    var c = ((SolidColorBrush)dataFile.Color).Color;
                    script += string.Join("\n", coords.Select(x => $"addArrow([{x.Lat}, {x.Lon}], '#{c.R.ToString("X2")}{c.G.ToString("X2")}{c.B.ToString("X2")}', {coords.IndexOf(x)});")) + "\n"; // dataFile.Data[day].Take(60)
                }
            }
            if (lastPt != null)
                script += $"yMap.setCenter([{lastPt.Lat}, {lastPt.Lon}]);\n";
            mapViewer.ExecuteScriptAsync(script);
            //"addArrow([55.7502, 37.6136], [55.7542, 37.6196])");
            // yMap.setCenter([45.0701, 38.9048]);addMarkColored(yMap, 45.0701, 38.9048, 14, 20, '#dd0000');");
            //RedrawMap();
        }

        private void Map_Redraw_Distance_Click(object sender, RoutedEventArgs e)
        {
            ViewingDist = true;
            RedrawGraph();
            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            var step = ViewRange.Length / steps;
            var script = "resetArrows();\n";
            NavDataEntry lastPt = null;
            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                var coords = new List<NavDataEntry>();

                for (var iStep = 0; iStep < steps; iStep++)
                {
                    var dtCurrent = ViewRange.Min + step * iStep;
                    var dtRange = new DateRange(dtCurrent, dtCurrent + step);

                    var allPoints = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                    if (allPoints.Count() == 0)
                        continue;

                    coords.Add(allPoints.Last());
                    lastPt = coords.Last();
                }
                if (coords.Count > 1)
                {
                    script += "startPath();\n";
                    script += string.Join("\n", coords.Select(x => $"addArrow([{x.Lat}, {x.Lon}]);")) + "\n"; // dataFile.Data[day].Take(60)
                }
            }
            if (lastPt != null)
                script += $"yMap.setCenter([{lastPt.Lat}, {lastPt.Lon}]);\n";
            mapViewer.ExecuteScriptAsync(script);
        }

        double GraphWidth => TotalGraphWidth;
        void DrawPointInfoById(int iStep)
        {
            var xmax = GraphWidth - 15;

            var step = ViewRange.Length / steps;

            ttt.Content = iStep;

            var dtCurrent = ViewRange.Min + step * iStep;
            var dtRange = new DateRange(dtCurrent, dtCurrent + step);

            foreach (var pt in ellipses)
            {

                var data = pt.Data;
                var allPoints = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                if (allPoints.Count() == 0)
                {
                    Canvas.SetLeft(pt.Ellipse, -100);
                    Canvas.SetLeft(pt.Text, -100);
                    continue;
                }

                var vMidValue = allPoints.Sum(x => x.GetValueByGraphNumber(CurrentSelectedParam)) / allPoints.Length;
                var vMid = ValueToGraphY(vMidValue);

                Canvas.SetLeft(pt.Ellipse, xmin + iStep * (xmax - xmin) / steps - 3);
                Canvas.SetLeft(pt.Text, xmin + iStep * (xmax - xmin) / steps);

                Canvas.SetTop(pt.Ellipse, vMid - 3);
                Canvas.SetTop(pt.Text, vMid - 30);
                pt.Text.Text = $"{allPoints.Length} точек\n{vMidValue}";

                ttt.Content = iStep + "  " + (ymin + vMid - 3);
            }

        }
        private void drawPointInfo(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var xmax = GraphWidth - 15;

            var iStep = (int)Math.Round((e.GetPosition(CanvasCurr).X - xmin) / (xmax - xmin) * steps);

            DrawPointInfoById(iStep);
        }

        private void ScrollViewer_Initialized(object sender, EventArgs e)
        {
        }

        private void graphDraw1_Initialized(object sender, EventArgs e)
        {
        }

        private void graphDraw1_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void gridView1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawGraph();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // show atm
            if (currPar == 0)
                return;

            currPar = 0;
            valMin = valMinPer[currPar];
            valMax = valMaxPer[currPar];

            RedrawGraph();
        }
        bool YAlighed => cb_yaling.IsChecked ?? false;

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // show speed
            if (currPar == 1)
                return;

            currPar = 1;
            valMin = valMinPer[currPar];
            valMax = valMaxPer[currPar];

            RedrawGraph();
        }

        void AlignY()
        {
            valMin = double.MaxValue;
            valMax = double.MinValue;

            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                var vals = data.Where(x => ViewRange.Includes(x.Time)).ToArray();
                if (vals.Length == 0)
                    continue;
                valMin = Math.Min(valMin, vals.Min(x => x.GetValueByGraphNumber(CurrentSelectedParam)));
                valMax = Math.Max(valMax, vals.Max(x => x.GetValueByGraphNumber(CurrentSelectedParam)));
            }
            if (valMax == double.MinValue)
            {
                valMin = valMinPer[currPar];
                valMax = valMaxPer[currPar];
                return;
            }

            var offset = valMax - valMin;
            valMin -= offset * 0.035;
            valMax += offset * 0.195;
        }

        private void graphDraw2_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mapViewer.Width = e.NewSize.Width;
            mapViewer.Height = e.NewSize.Height;
        }

        private void ScrollViewer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var xmax = GraphWidth - 15;
            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            var step = ViewRange.Length / steps;
            var iStepC = (int)Math.Round((e.GetPosition(CanvasCurr).X - xmin) / (xmax - xmin) * steps);
            if (iStepC >= 1 && iStepC < 120)
            {

                var allPoints = new NavDataEntry[0];
                foreach (var dataFile in LogsList.Where(x => x.Selected))
                {
                    if (!dataFile.Data.TryGetValue(day, out var data))
                        continue;

                    var coords = new List<NavDataEntry>();
                    for (var iStep = 0; iStep < steps; iStep++)
                    {
                        var dtCurrent = ViewRange.Min + step * iStep;
                        var dtRange = new DateRange(dtCurrent, dtCurrent + step);

                        var allPointsTmp = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                        if (allPointsTmp.Count() == 0)
                            continue;

                        if (iStep == iStepC)
                            allPoints = allPointsTmp;
                    }
                }
                var script = $"highlightPoints([{string.Join(",", allPoints.Select(x => x.ToJavascript()))}]);\n";
                mapViewer.ExecuteScriptAsync(script);
            }
        }

        private void DebugGraphShow(object sender, RoutedEventArgs e)
        {
            var g = new AGraph();
            g.Feed(LogsList[0]);
            var script = string.Join(", ", g.Marks.Select(x => $"[{x.Lat}, {x.Lon + 0.00015}, {x.Atm.ToString("#.##")}]"));
            script = $"highlight([{script}])";
            mapViewer.ExecuteScriptAsync(script);
        }

        private void dateaaaaaaaaa_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dateaaaaaaaaa.SelectedItem == null) 
                return;

            var dtStr = dateaaaaaaaaa.SelectedItem as string;
            var dt = DateTime.Parse(dtStr);


            ViewRange = new DateRange(dt, dt + TimeSpan.FromDays(1));
            RedrawGraph();
        }

        private void DebugGraphShowVisible(object sender, RoutedEventArgs e)
        {
            var day = DateStart.SelectedDate ?? new DateTime(2023, 10, 30);
            var step = ViewRange.Length / steps;

            var g = new AGraph();

            foreach (var dataFile in LogsList.Where(x => x.Selected))
            {
                var marks = new List<GraphMark>();
                if (!dataFile.Data.TryGetValue(day, out var data))
                    continue;

                /*var dtLast = DtDataOnly.Min;
                var dtRangeFirst = new DateRange(dtLast, dtLast + step);
                var allPointsFirst = data.Where(x => dtRangeFirst.Includes(x.Time)).ToArray();

                var vMinLast = allPointsFirst.Min(x => x.Speed);
                var vMaxLast = allPointsFirst.Max(x => x.Speed);
                var vMidLast = allPointsFirst.Sum(x => x.Speed) / allPointsFirst.Length;
                */

                var atms = new List<float>();
                for (var iStep = 1; iStep < steps; iStep++)
                {
                    var dtCurrent = ViewRange.Min + step * iStep;
                    var dtRange = new DateRange(dtCurrent, dtCurrent + step);

                    //if (!dataFile.DataRange.Includes(dtLast, step))
                    //    continue;

                    //for (int data_set = 0; data_set < 1; data_set++)
                    {
                        var allPoints = data.Where(x => dtRange.Includes(x.Time)).ToArray();
                        if (allPoints.Count() == 0)
                            continue;

                        var vMid = allPoints.Sum(x => x.Atm) / allPoints.Length;
                        marks.Add(new GraphMark(allPoints, vMid));
                    }
                }
                g.Feed(marks);
            }
            var script = string.Join(", ", g.Marks.Select(x => $"[{x.Lat}, {x.Lon + 0.00015}, {x.Atm.ToString("#.##")}]"));
            script = $"highlight([{script}])";
            mapViewer.ExecuteScriptAsync(script);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RedrawGraph();
        }

        private void cb_yaling_Unchecked(object sender, RoutedEventArgs e)
        {
            valMin = valMinPer[currPar];
            valMax = valMaxPer[currPar];
            RedrawGraph();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }
    }
}
