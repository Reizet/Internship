using BudgetTracker.Data;
using BudgetTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BudgetTracker
{
	public partial class MainWindow : Window
	{
		private readonly DatabaseService _databaseService;

		public MainWindow()
		{
			InitializeComponent();

			_databaseService = new DatabaseService();

			TypeComboBox.SelectedIndex = 0;
			PeriodComboBox.SelectedIndex = 0;
			DatePicker.SelectedDate = DateTime.Now;

			LoadRecords();
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			if (TypeComboBox.SelectedItem == null ||
				string.IsNullOrWhiteSpace(CategoryTextBox.Text) ||
				string.IsNullOrWhiteSpace(AmountTextBox.Text) ||
				DatePicker.SelectedDate == null)
			{
				MessageBox.Show("Заповніть тип, категорію, суму та дату");
				return;
			}

			decimal amount;

			if (!decimal.TryParse(AmountTextBox.Text, out amount) || amount <= 0)
			{
				MessageBox.Show("Введіть коректну суму");
				return;
			}

			string type = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString();

			var record = new BudgetRecord
			{
				Type = type,
				Category = CategoryTextBox.Text.Trim(),
				Tag = TagTextBox.Text.Trim(),
				Amount = amount,
				Date = DatePicker.SelectedDate.Value,
				Description = DescriptionTextBox.Text.Trim()
			};

			try
			{
				_databaseService.AddRecord(record);
				ClearForm();
				LoadRecords();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Помилка при додаванні запису:\n" + ex.Message);
			}
		}

		private void LoadRecords()
		{
			try
			{
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

				List<BudgetRecord> records = _databaseService.GetRecords(from, now);

				RecordsDataGrid.ItemsSource = records;

				decimal income = records
					.Where(r => r.Type == "Дохід")
					.Sum(r => r.Amount);

				decimal expense = records
					.Where(r => r.Type == "Витрата")
					.Sum(r => r.Amount);

				decimal balance = income - expense;

				SummaryTextBlock.Text =
					"Доходи: " + income + " грн | " +
					"Витрати: " + expense + " грн | " +
					"Баланс: " + balance + " грн";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Помилка при завантаженні даних:\n" + ex.Message);
			}
		}

		private void ClearForm()
		{
			CategoryTextBox.Clear();
			TagTextBox.Clear();
			AmountTextBox.Clear();
			DescriptionTextBox.Clear();

			TypeComboBox.SelectedIndex = 0;
			DatePicker.SelectedDate = DateTime.Now;
		}

		

		private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				LoadRecords();
			}
		}
		private void ReportsButton_Click(object sender, RoutedEventArgs e)
		{
			ReportWindow reportWindow = new ReportWindow();
			reportWindow.ShowDialog();
		}
		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			BudgetRecord selectedRecord = RecordsDataGrid.SelectedItem as BudgetRecord;

			if (selectedRecord == null)
			{
				MessageBox.Show("Оберіть запис для видалення.");
				return;
			}

			MessageBoxResult result = MessageBox.Show(
				"Видалити вибраний запис?",
				"Підтвердження",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				try
				{
					_databaseService.DeleteRecord(selectedRecord.Id);
					LoadRecords();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Помилка при видаленні запису:\n" + ex.Message);
				}
			}
		}
	}
}