using BudgetTracker.Data;
using BudgetTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BudgetTracker
{
	public partial class ReportWindow : Window
	{
		private readonly DatabaseService _databaseService;
		public ReportWindow()
		{
			InitializeComponent();

			_databaseService = new DatabaseService();

			PeriodComboBox.SelectedIndex = 0;
			TypeComboBox.SelectedIndex = 0;

			LoadCategories();

			GenerateReport();
		}

		private void LoadCategories()
		{
			CategoryComboBox.Items.Clear();
			CategoryComboBox.Items.Add("Усі категорії");

			List<string> categories = _databaseService.GetCategories();

			foreach (string category in categories)
			{
				CategoryComboBox.Items.Add(category);
			}

			CategoryComboBox.SelectedIndex = 0;
		}

		private void ReportParameter_Changed(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				GenerateReport();
			}
		}

		private void GenerateReport()
		{
			if (PeriodComboBox.SelectedItem == null ||
				TypeComboBox.SelectedItem == null ||
				CategoryComboBox.SelectedItem == null)
			{
				return;
			}

			DateTime now = DateTime.Now;
			DateTime from;

			string selectedPeriod = ((ComboBoxItem)PeriodComboBox.SelectedItem).Content.ToString();

			if (selectedPeriod == "За місяць")
			{
				from = now.AddMonths(-1);
			}
			else if (selectedPeriod == "За пів року")
			{
				from = now.AddMonths(-6);
			}
			else
			{
				from = now.AddYears(-1);
			}

			string type = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString();
			string category = CategoryComboBox.SelectedItem.ToString();

			List<ReportItem> reportData = _databaseService.GetReportData(from, now, type, category);

			DrawChart(reportData, type, category, selectedPeriod);
		}

		private void DrawChart(List<ReportItem> data, string type, string category, string period)
		{
			ChartCanvas.Children.Clear();

			if (data == null || data.Count == 0)
			{
				SummaryTextBlock.Text = "Немає даних";
				return;
			}

			double canvasWidth = ChartCanvas.ActualWidth;
			double canvasHeight = ChartCanvas.ActualHeight;

			if (canvasWidth == 0)
				canvasWidth = 900;

			if (canvasHeight == 0)
				canvasHeight = 400;

			double marginLeft = 60;
			double marginBottom = 50;
			double marginTop = 30;
			double marginRight = 30;

			double chartWidth = canvasWidth - marginLeft - marginRight;
			double chartHeight = canvasHeight - marginTop - marginBottom;

			decimal maxAmount = data.Max(x => x.TotalAmount);

			double columnWidth = chartWidth / data.Count;
			double barWidth = columnWidth * 0.55;

			Line yAxis = new Line
			{
				X1 = marginLeft,
				Y1 = marginTop,
				X2 = marginLeft,
				Y2 = marginTop + chartHeight,
				Stroke = Brushes.Black,
				StrokeThickness = 1
			};

			Line xAxis = new Line
			{
				X1 = marginLeft,
				Y1 = marginTop + chartHeight,
				X2 = marginLeft + chartWidth,
				Y2 = marginTop + chartHeight,
				Stroke = Brushes.Black,
				StrokeThickness = 1
			};

			ChartCanvas.Children.Add(yAxis);
			ChartCanvas.Children.Add(xAxis);

			for (int i = 0; i < data.Count; i++)
			{
				double barHeight = (double)(data[i].TotalAmount / maxAmount) * chartHeight;

				double x = marginLeft + i * columnWidth + (columnWidth - barWidth) / 2;
				double y = marginTop + chartHeight - barHeight;

				Rectangle bar = new Rectangle
				{
					Width = barWidth,
					Height = barHeight,
					Fill = type == "Витрата" ? Brushes.IndianRed : Brushes.SeaGreen
				};

				Canvas.SetLeft(bar, x);
				Canvas.SetTop(bar, y);
				ChartCanvas.Children.Add(bar);

				TextBlock amountText = new TextBlock
				{
					Text = data[i].TotalAmount.ToString("0.##"),
					FontSize = 11
				};

				Canvas.SetLeft(amountText, x);
				Canvas.SetTop(amountText, y - 20);
				ChartCanvas.Children.Add(amountText);

				TextBlock labelText = new TextBlock
				{
					Text = DateTime.Parse(data[i].Label).ToString("dd.MM"),
					FontSize = 11
				};

				Canvas.SetLeft(labelText, x);
				Canvas.SetTop(labelText, marginTop + chartHeight + 5);
				ChartCanvas.Children.Add(labelText);
			}

			decimal total = data.Sum(x => x.TotalAmount);

			SummaryTextBlock.Text =
				"Звіт: " + type +
				" | Категорія: " + category +
				" | Період: " + period +
				" | Загальна сума: " + total + " грн";
		}
	}
}