using BudgetTracker.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace BudgetTracker.Data
{
	public class DatabaseService
	{
		private readonly string _connectionString =
			"Host=localhost;Port=5432;Database=budget tracker;Username=postgres;Password=123456";

		public void AddRecord(BudgetRecord record)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();

				string sql = @"
                    INSERT INTO public.records
                    (type, category, tag, amount, record_date, description)
                    VALUES
                    (@type, @category, @tag, @amount, @record_date, @description);
                ";

				using (var command = new NpgsqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@type", record.Type);
					command.Parameters.AddWithValue("@category", record.Category);
					command.Parameters.AddWithValue("@tag", record.Tag ?? "");
					command.Parameters.AddWithValue("@amount", record.Amount);
					command.Parameters.AddWithValue("@record_date", record.Date.Date);
					command.Parameters.AddWithValue("@description", record.Description ?? "");

					command.ExecuteNonQuery();
				}
			}
		}

		public List<BudgetRecord> GetRecords(DateTime from, DateTime to)
		{
			var records = new List<BudgetRecord>();

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();

				string sql = @"
                    SELECT id, type, category, tag, amount, record_date, description
                    FROM public.records
                    WHERE record_date BETWEEN @from AND @to
                    ORDER BY record_date DESC, id DESC;
                ";

				using (var command = new NpgsqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@from", from.Date);
					command.Parameters.AddWithValue("@to", to.Date);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							records.Add(new BudgetRecord
							{
								Id = reader.GetInt32(0),
								Type = reader.GetString(1),
								Category = reader.GetString(2),
								Tag = reader.IsDBNull(3) ? "" : reader.GetString(3),
								Amount = reader.GetDecimal(4),
								Date = reader.GetDateTime(5),
								Description = reader.IsDBNull(6) ? "" : reader.GetString(6)
							});
						}
					}
				}
			}

			return records;
		}

		public void DeleteRecord(int id)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();

				string sql = "DELETE FROM public.records WHERE id = @id;";

				using (var command = new NpgsqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@id", id);
					command.ExecuteNonQuery();
				}
			}
		}
		public List<ReportItem> GetReportData(DateTime from, DateTime to, string type, string category)
		{
			var items = new List<ReportItem>();

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();

				string sql = @"
            SELECT 
                record_date::text AS label,
                SUM(amount) AS total_amount
            FROM public.records
            WHERE record_date BETWEEN @from AND @to
              AND type = @type
              AND (@category = 'Усі категорії' OR category = @category)
            GROUP BY record_date
            ORDER BY record_date;
        ";

				using (var command = new NpgsqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@from", from.Date);
					command.Parameters.AddWithValue("@to", to.Date);
					command.Parameters.AddWithValue("@type", type);
					command.Parameters.AddWithValue("@category", category);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							items.Add(new ReportItem
							{
								Label = reader.GetString(0),
								TotalAmount = reader.GetDecimal(1)
							});
						}
					}
				}
			}

			return items;
		}

		public List<string> GetCategories()
		{
			var categories = new List<string>();

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();

				string sql = @"
            SELECT DISTINCT category
            FROM public.records
            ORDER BY category;
        ";

				using (var command = new NpgsqlCommand(sql, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						categories.Add(reader.GetString(0));
					}
				}
			}

			return categories;
		}
	}
}