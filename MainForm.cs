using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using HtmlAgilityPack;
using System.Net;
using System.Text;

namespace WeatherDiary
{
	public partial class MainForm : Form
	{
		public struct Time
		{
			public String title;
			public String cloud;
			public String temp;
			public String windVelocity;
			public String windDirection;
			public String press;
		}
		public struct Year
		{
			public int num;
			public Month[] months;

		}
		public struct Month
		{
			public int num;
			public Day[] days;

		}
		public struct Day
		{
			public int num;
			public int temp;
			public String cloud;
			public int press;
			public int windVelocity;
			public String windDirection;
			public String prec;
		}
		Year[] years;
		public MainForm()
		{
			InitializeComponent();
			years = new Year[0];
		}

		private void Open()
		{
			if (!File.Exists("Дневник погоды.xml"))
			{
				MessageBox.Show("Файл 'Дневник погоды.xml' не был найден!");
				return;
			}
			FileStream fileWeatherDiary = File.Open("Дневник погоды.xml", FileMode.Open);
			XmlSerializer readerWeatherDiary = new XmlSerializer(typeof(Year[]));
			years = (Year[])readerWeatherDiary.Deserialize(fileWeatherDiary);
			fileWeatherDiary.Close();
		}

		private void Save()
		{
			FileStream fileWeatherDiary = File.Open("Дневник погоды.xml", FileMode.Create);
			XmlSerializer writerWeatherDiary = new XmlSerializer(typeof(Year[]));
			writerWeatherDiary.Serialize(fileWeatherDiary, years);
			fileWeatherDiary.Close();
		}

		private void SetDayInfo()
		{
			Day day = new Day();
			Month month = new Month();
			Year year = new Year();
			try
			{
				day.num = Calendar.SelectionRange.Start.Day;
				day.temp = Convert.ToInt32(tbTemp.Text);
				day.cloud = cbCloud.Text;
				day.press = Convert.ToInt32(tbPress.Text);
				day.windVelocity = Convert.ToInt32(tbWind.Text);
				day.windDirection = cbWind.Text;
				day.prec = cbPrec.Text;
				month.num = Calendar.SelectionRange.Start.Month;
				month.days = new Day[1];
				month.days[0] = day;
				year.num = Calendar.SelectionRange.Start.Year;
				year.months = new Month[1];
				year.months[0] = month;
			}
			catch
			{
				MessageBox.Show("Проверьте правильность заполнения всех полей!");
				return;
			}
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num == Calendar.SelectionRange.Start.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == Calendar.SelectionRange.Start.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								if (years[i].months[j].days[k].num == Calendar.SelectionRange.Start.Day)
								{
									years[i].months[j].days[k].temp = Convert.ToInt32(tbTemp.Text);
									years[i].months[j].days[k].cloud = cbCloud.Text;
									years[i].months[j].days[k].press = Convert.ToInt32(tbPress.Text);
									years[i].months[j].days[k].windVelocity = Convert.ToInt32(tbWind.Text);
									years[i].months[j].days[k].windDirection = cbWind.Text;
									years[i].months[j].days[k].prec = cbPrec.Text;
									return;
								}
							}
							Array.Resize(ref years[i].months[j].days, years[i].months[j].days.Length + 1);
							years[i].months[j].days[years[i].months[j].days.Length - 1] = day;
							return;
						}
					}
					Array.Resize(ref years[i].months, years[i].months.Length + 1);
					years[i].months[years[i].months.Length - 1] = month;
					return;
				}
			}
			Array.Resize(ref years, years.Length + 1);
			years[years.Length - 1] = year;
			return;
		}

		private Day FindDay(DateTime dt)
		{
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num == dt.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == dt.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								if (years[i].months[j].days[k].num == dt.Day)
								{
									return years[i].months[j].days[k];
								}
							}
						}
					}
				}
			}
			return new Day();
		}

		private void GetDayInfo()
		{
			Day day = FindDay(Calendar.SelectionRange.Start);
			String str = "";
			if (day.temp > 0)
			{
				str = "+";
			}
			if (day.temp < 0)
			{
				str = "-";
			}
			lblTempDay.Text = str + day.temp.ToString() + "°С";
			lblCloudDay.Text = day.cloud;
			lblPressDay.Text = day.press.ToString() + " мм.рт.ст.";
			lblWindDay.Text = day.windVelocity.ToString() + " м/с; " + day.windDirection;
			lblPrecDay.Text = day.prec;
			tbTemp.Text = day.temp.ToString();
			cbCloud.Text = day.cloud;
			tbPress.Text = day.press.ToString();
			tbWind.Text = day.windVelocity.ToString();
			cbWind.Text = day.windDirection;
			cbPrec.Text = day.prec;
		}

		private void GetDayInYearInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			DateTime dt = Convert.ToDateTime(Calendar.SelectionRange.Start.Day.ToString() +
				"." + Calendar.SelectionRange.Start.Month.ToString() + "." + cbDayInYear.Text);
			Day day = FindDay(dt);
			String str = "";
			if (day.temp > 0)
			{
				str = "+";
			}
			if (day.temp < 0)
			{
				str = "-";
			}
			lblTempDayInYear.Text = str + day.temp.ToString() + "°С";
			lblCloudDayInYear.Text = day.cloud;
			lblPressDayInYear.Text = day.press.ToString() + " мм.рт.ст.";
			lblWindDayInYear.Text = day.windVelocity.ToString() + " м/с; " + day.windDirection;
			lblPrecDayInYear.Text = day.prec;
		}

		private void GetMonthInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num == Calendar.SelectionRange.Start.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == Calendar.SelectionRange.Start.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								temp += (double)years[i].months[j].days[k].temp / (double)years[i].months[j].days.Length;
								press += (double)years[i].months[j].days[k].press / (double)years[i].months[j].days.Length;
								velocity += (double)years[i].months[j].days[k].windVelocity / (double)years[i].months[j].days.Length;
							}
						}
					}
				}
			}
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempMonth.Text = str + temp.ToString("##.00") + "°С";
			lblPressMonth.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindMonth.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetMonthInYearInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			DateTime dt = Convert.ToDateTime(Calendar.SelectionRange.Start.Day.ToString() +
				"." + Calendar.SelectionRange.Start.Month.ToString() + "." + cbMonthInYear.Text);
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num == dt.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == dt.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								temp += (double)years[i].months[j].days[k].temp / (double)years[i].months[j].days.Length;
								press += (double)years[i].months[j].days[k].press / (double)years[i].months[j].days.Length;
								velocity += (double)years[i].months[j].days[k].windVelocity / (double)years[i].months[j].days.Length;
							}
						}
					}
				}
			}
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempMonthInYear.Text = str + temp.ToString("##.00") + "°С";
			lblPressMonthInYear.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindMonthInYear.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetYearInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			int days = 0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num == Calendar.SelectionRange.Start.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						for (int k = 0; k < years[i].months[j].days.Length; k++)
						{
							temp += (double)years[i].months[j].days[k].temp;
							press += (double)years[i].months[j].days[k].press;
							velocity += (double)years[i].months[j].days[k].windVelocity;
							days++;
						}
					}
				}
			}
			temp = temp / (double)days;
			press = press / (double)days;
			velocity = velocity / (double)days;
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempYear.Text = str + temp.ToString("##.00") + "°С";
			lblPressYear.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindYear.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetYearInYearInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			DateTime dt = Convert.ToDateTime(Calendar.SelectionRange.Start.Day.ToString() +
				"." + Calendar.SelectionRange.Start.Month.ToString() + "." + cbYearInYear.Text);
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			int days = 0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num == dt.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						for (int k = 0; k < years[i].months[j].days.Length; k++)
						{
							temp += (double)years[i].months[j].days[k].temp;
							press += (double)years[i].months[j].days[k].press;
							velocity += (double)years[i].months[j].days[k].windVelocity;
							days++;
						}
					}
				}
			}
			temp = temp / (double)days;
			press = press / (double)days;
			velocity = velocity / (double)days;
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempYearInYear.Text = str + temp.ToString("##.00") + "°С";
			lblPressYearInYear.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindYearInYear.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetDayForecastInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			int days = 0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num != Calendar.TodayDate.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == Calendar.TodayDate.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								if (years[i].months[j].days[k].num == Calendar.TodayDate.Day)
								{
									temp += (double)years[i].months[j].days[k].temp;
									press += (double)years[i].months[j].days[k].press;
									velocity += (double)years[i].months[j].days[k].windVelocity;
									days++;
								}
							}
						}
					}
				}
			}
			temp = temp / (double)days;
			press = press / (double)days;
			velocity = velocity / (double)days;
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempDayForecast.Text = str + temp.ToString("##.00") + "°С";
			lblPressDayForecast.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindDayForecast.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetMonthForecastInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			int days = 0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num != Calendar.TodayDate.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == Calendar.TodayDate.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								temp += (double)years[i].months[j].days[k].temp;
								press += (double)years[i].months[j].days[k].press;
								velocity += (double)years[i].months[j].days[k].windVelocity;
								days++;
							}
						}
					}
				}
			}
			temp = temp / (double)days;
			press = press / (double)days;
			velocity = velocity / (double)days;
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempMonthForecast.Text = str + temp.ToString("##.00") + "°С";
			lblPressMonthForecast.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindMonthForecast.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetYearForecastInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			int days = 0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num != Calendar.TodayDate.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						for (int k = 0; k < years[i].months[j].days.Length; k++)
						{
							temp += (double)years[i].months[j].days[k].temp;
							press += (double)years[i].months[j].days[k].press;
							velocity += (double)years[i].months[j].days[k].windVelocity;
							days++;
						}
					}
				}
			}
			temp = temp / (double)days;
			press = press / (double)days;
			velocity = velocity / (double)days;
			String str = "";
			if (temp > 0.0)
			{
				str = "+";
			}
			if (temp < 0.0)
			{
				str = "-";
			}
			lblTempYearForecast.Text = str + temp.ToString("##.00") + "°С";
			lblPressYearForecast.Text = press.ToString("###.00") + " мм.рт.ст.";
			lblWindYearForecast.Text = velocity.ToString("##.00") + " м/с";
		}

		private void GetDayMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			int min = 1000;
			int max = -1000;
			int mini = 0, minj = 0, mink = 0;
			int maxi = 0, maxj = 0, maxk = 0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num != Calendar.SelectionRange.Start.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == Calendar.SelectionRange.Start.Month)
						{
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								if (years[i].months[j].days[k].num == Calendar.SelectionRange.Start.Day)
								{
									if (years[i].months[j].days[k].temp > max)
									{
										max = years[i].months[j].days[k].temp;
										maxi = i; maxj = j; maxk = k;
									}
									if (years[i].months[j].days[k].temp < min)
									{
										min = years[i].months[j].days[k].temp;
										mini = i; minj = j; mink = k;
									}
								}
							}
						}
					}
				}
			}
			String str1 = "";
			if (years[mini].months[minj].days[mink].temp > 0.0)
			{
				str1 = "+";
			}
			if (years[mini].months[minj].days[mink].temp < 0.0)
			{
				str1 = "-";
			}
			lblTempDayMin.Text = str1 + years[mini].months[minj].days[mink].temp.ToString() + "°С";
			lblPressDayMin.Text = years[mini].months[minj].days[mink].press.ToString();
			lblWindDayMin.Text = years[mini].months[minj].days[mink].windVelocity.ToString() + " м/с";
			String str2 = "";
			if (years[maxi].months[maxj].days[maxk].temp > 0.0)
			{
				str2 = "+";
			}
			if (years[maxi].months[maxj].days[maxk].temp < 0.0)
			{
				str2 = "-";
			}
			lblTempDayMax.Text = str2 + years[maxi].months[maxj].days[maxk].temp.ToString() + "°С";
			lblPressDayMax.Text = years[maxi].months[maxj].days[maxk].press.ToString();
			lblWindDayMax.Text = years[maxi].months[maxj].days[maxk].windVelocity.ToString() + " м/с";
		}

		private void GetMonthMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double mintemp = 1000.0, minpress = 0.0, minvelocity = 0.0;
			double maxtemp = -1000.0, maxpress = 0.0, maxvelocity = 0.0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num != Calendar.SelectionRange.Start.Year)
				{
					for (int j = 0; j < years[i].months.Length; j++)
					{
						if (years[i].months[j].num == Calendar.SelectionRange.Start.Month)
						{
							double temp = 0.0;
							double press = 0.0;
							double velocity = 0.0;
							for (int k = 0; k < years[i].months[j].days.Length; k++)
							{
								temp += (double)years[i].months[j].days[k].temp / (double)years[i].months[j].days.Length;
								press += (double)years[i].months[j].days[k].press / (double)years[i].months[j].days.Length;
								velocity += (double)years[i].months[j].days[k].windVelocity / (double)years[i].months[j].days.Length;
							}
							if (temp > maxtemp)
							{
								maxtemp = temp;
								maxpress = press; maxvelocity = velocity;
							}
							if (temp < mintemp)
							{
								mintemp = temp;
								minpress = press; minvelocity = velocity;
							}
						}
					}
				}
			}
			String str1 = "";
			if (mintemp > 0.0)
			{
				str1 = "+";
			}
			if (mintemp < 0.0)
			{
				str1 = "-";
			}
			lblTempMonthMin.Text = str1 + mintemp.ToString("##.00") + "°С";
			lblPressMonthMin.Text = minpress.ToString("###.00");
			lblWindMonthMin.Text = minvelocity.ToString("##.00") + " м/с";
			String str2 = "";
			if (maxtemp > 0.0)
			{
				str2 = "+";
			}
			if (maxtemp < 0.0)
			{
				str2 = "-";
			}
			lblTempMonthMax.Text = str2 + maxtemp.ToString("##.00") + "°С";
			lblPressMonthMax.Text = maxpress.ToString("###.00");
			lblWindMonthMax.Text = maxvelocity.ToString("##.00") + " м/с";
		}

		private void GetYearMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double mintemp = 1000.0, minpress = 0.0, minvelocity = 0.0;
			double maxtemp = -1000.0, maxpress = 0.0, maxvelocity = 0.0;
			for (int i = 0; i < years.Length; i++)
			{
				if (years[i].num != Calendar.SelectionRange.Start.Year)
				{
					double temp = 0.0;
					double press = 0.0;
					double velocity = 0.0;
					int days = 0;
					for (int j = 0; j < years[i].months.Length; j++)
					{
						for (int k = 0; k < years[i].months[j].days.Length; k++)
						{
							temp += (double)years[i].months[j].days[k].temp;
							press += (double)years[i].months[j].days[k].press;
							velocity += (double)years[i].months[j].days[k].windVelocity;
							days++;
						}
					}
					temp = temp / (double)days;
					press = press / (double)days;
					velocity = velocity / (double)days;
					if (temp > maxtemp)
					{
						maxtemp = temp;
						maxpress = press; maxvelocity = velocity;
					}
					if (temp < mintemp)
					{
						mintemp = temp;
						minpress = press; minvelocity = velocity;
					}
				}
			}
			String str1 = "";
			if (mintemp > 0.0)
			{
				str1 = "+";
			}
			if (mintemp < 0.0)
			{
				str1 = "-";
			}
			lblTempYearMin.Text = str1 + mintemp.ToString("##.00") + "°С";
			lblPressYearMin.Text = minpress.ToString("###.00");
			lblWindYearMin.Text = minvelocity.ToString("##.00") + " м/с";
			String str2 = "";
			if (maxtemp > 0.0)
			{
				str2 = "+";
			}
			if (maxtemp < 0.0)
			{
				str2 = "-";
			}
			lblTempYearMax.Text = str2 + maxtemp.ToString("##.00") + "°С";
			lblPressYearMax.Text = maxpress.ToString("###.00");
			lblWindYearMax.Text = maxvelocity.ToString("##.00") + " м/с";
		}

		private void GetAbsoluteMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			int mintemp = 1000, minpress = 1000, minvelocity = 1000;
			int maxtemp = -1000, maxpress = -1000, maxvelocity = -1000;
			for (int i = 0; i < years.Length; i++)
			{
				for (int j = 0; j < years[i].months.Length; j++)
				{
					for (int k = 0; k < years[i].months[j].days.Length; k++)
					{
						if ((years[i].num != Calendar.TodayDate.Year) && (years[i].months[j].num != Calendar.TodayDate.Month) && (years[i].months[j].days[k].num != Calendar.TodayDate.Day))
						{
							if (years[i].months[j].days[k].temp > maxtemp)
							{
								maxtemp = years[i].months[j].days[k].temp;
							}
							if (years[i].months[j].days[k].temp < mintemp)
							{
								mintemp = years[i].months[j].days[k].temp;
							}
							if (years[i].months[j].days[k].press > maxpress)
							{
								maxpress = years[i].months[j].days[k].press;
							}
							if (years[i].months[j].days[k].press < minpress)
							{
								minpress = years[i].months[j].days[k].press;
							}
							if (years[i].months[j].days[k].windVelocity > maxvelocity)
							{
								maxvelocity = years[i].months[j].days[k].windVelocity;
							}
							if (years[i].months[j].days[k].windVelocity < minvelocity)
							{
								minvelocity = years[i].months[j].days[k].windVelocity;
							}
						}
					}
				}
			}
			String str1 = "";
			if (mintemp > 0.0)
			{
				str1 = "+";
			}
			if (mintemp < 0.0)
			{
				str1 = "-";
			}
			lblMinTemp.Text = str1 + mintemp.ToString() + "°С";
			lblMinPress.Text = minpress.ToString();
			lblMinWind.Text = minvelocity.ToString() + " м/с";
			String str2 = "";
			if (maxtemp > 0.0)
			{
				str2 = "+";
			}
			if (maxtemp < 0.0)
			{
				str2 = "-";
			}
			lblMaxTemp.Text = str2 + maxtemp.ToString() + "°С";
			lblMaxPress.Text = maxpress.ToString();
			lblMaxWind.Text = maxvelocity.ToString() + " м/с";
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			Open();
			for (int i = 0; i < years.Length; i++)
			{
				cbDayInYear.Items.Add(years[i].num);
				cbMonthInYear.Items.Add(years[i].num);
				cbYearInYear.Items.Add(years[i].num);
			}
			cbDayInYear.Sorted = true;
			cbMonthInYear.Sorted = true;
			cbYearInYear.Sorted = true;
			if (years.Length != 0)
			{
				cbDayInYear.SelectedIndex = 0;
				cbMonthInYear.SelectedIndex = 0;
				cbYearInYear.SelectedIndex = 0;
			}
			GetDayInfo();
			GetDayInYearInfo();
			GetMonthInfo();
			GetMonthInYearInfo();
			GetYearInfo();
			GetYearInYearInfo();
			GetDayForecastInfo();
			GetMonthForecastInfo();
			GetYearForecastInfo();
			GetDayMinMaxInfo();
			GetMonthMinMaxInfo();
			GetYearMinMaxInfo();
			GetAbsoluteMinMaxInfo();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			SetDayInfo();
			GetDayInfo();
			GetDayInYearInfo();
			GetMonthInfo();
			GetMonthInYearInfo();
			GetYearInfo();
			GetYearInYearInfo();
			GetDayForecastInfo();
			GetMonthForecastInfo();
			GetYearForecastInfo();
			GetDayMinMaxInfo();
			GetMonthMinMaxInfo();
			GetYearMinMaxInfo();
			GetAbsoluteMinMaxInfo();
			Save();
		}

		private void Calendar_DateSelected(object sender, DateRangeEventArgs e)
		{
			GetDayInfo();
			GetDayInYearInfo();
			GetMonthInfo();
			GetMonthInYearInfo();
			GetYearInfo();
			GetYearInYearInfo();
			GetDayMinMaxInfo();
			GetMonthMinMaxInfo();
			GetYearMinMaxInfo();
			GetAbsoluteMinMaxInfo();
		}

		private void cbDayInYear_SelectedIndexChanged(object sender, EventArgs e)
		{
			GetDayInYearInfo();
		}

		private void cbMonthInYear_SelectedIndexChanged(object sender, EventArgs e)
		{
			GetMonthInYearInfo();
		}

		private void cbYearInYear_SelectedIndexChanged(object sender, EventArgs e)
		{
			GetYearInYearInfo();
		}

		private void WeatherToForm(Time[] time)
		{
			try
			{
				tbTemp.Text = Convert.ToInt32(time[4].temp).ToString();
				tbPress.Text = Convert.ToInt32(time[4].press).ToString();
				tbWind.Text = Convert.ToInt32(time[4].windVelocity).ToString();
			}
			catch
			{

			}
			cbWind.Text = time[4].windDirection.ToLower();
			int clear = 0, cloud = 0, mainlyCloud = 0;
			bool rain = false, snow = false, storm = false, hail = false;
			for (int i = 2; i < 7; i++)
			{
				if (time[i].cloud.ToLower().Contains("ясно"))
				{
					clear++;
				}
				if (time[i].cloud.ToLower().Contains("облачно"))
				{
					cloud++;
				}
				if (time[i].cloud.ToLower().Contains("пасмурно"))
				{
					mainlyCloud++;
				}
				if (time[i].cloud.ToLower().Contains("дождь"))
				{
					rain = true;
				}
				if (time[i].cloud.ToLower().Contains("снег"))
				{
					snow = true;
				}
				if (time[i].cloud.ToLower().Contains("гроза"))
				{
					storm = true;
				}
				if (time[i].cloud.ToLower().Contains("град"))
				{
					hail = true;
				}
			}
			if ((clear > 2) && (mainlyCloud == 0))
			{
				cbCloud.Text = "ясно";
			}
			else
			{
				if ((mainlyCloud > 2) && (clear == 0))
				{
					cbCloud.Text = "пасмурно";
				}
				else
				{
					cbCloud.Text = "облачно";
				}
			}
			if (!rain && !snow && !storm && !hail)
			{
				cbPrec.Text = "";
			}
			if (rain && !snow && !storm && !hail)
			{
				cbPrec.Text = "дождь";
			}
			if (!rain && snow && !storm && !hail)
			{
				cbPrec.Text = "снег";
			}
			if (!rain && !snow && storm && !hail)
			{
				cbPrec.Text = "гроза";
			}
			if (!rain && !snow && !storm && hail)
			{
				cbPrec.Text = "град";
			}
			if (rain && snow && !storm && !hail)
			{
				cbPrec.Text = "дождь со снегом";
			}
			if (rain && !snow && storm && !hail)
			{
				cbPrec.Text = "дождь с грозой";
			}
		}

		private String ValidateAndCorrect(String value)
		{
			return WebUtility.HtmlDecode(value);
		}

		private String Trim(String text)
		{
			return text.Trim(new char[] { ' ', '\r', '\n' });
		}

		private String GetHtml(String adress)
		{
			WebRequest.DefaultWebProxy = new WebProxy();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(adress);
			request.Proxy = null;
			request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
			request.Headers.Add("Accept-Language: ru-Ru,ru;q=0.5");
			request.Headers.Add("Accept-Charset: Windows-1251,utf-8;q=0.7,*;q=0.7");
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				string encoding = response.Headers["Content-Type"].ToString().Split(new string[] { "charset=" }, StringSplitOptions.RemoveEmptyEntries)[1];
				StringBuilder sb = new StringBuilder();
				byte[] buf = new byte[8192];
				int b = 0;
				Stream resStream = response.GetResponseStream();
				do
				{
					b = resStream.Read(buf, 0, buf.Length);
					if (b != 0)
					{
						sb.Append(Encoding.GetEncoding(encoding).GetString(buf, 0, b));
					}
				} while (b > 0);
				return sb.ToString();
			}
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			String htmlCode = GetHtml("http://beta.gismeteo.ru/weather-kirov-4292/#/wind.precipitation.pressure.humidity/");
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(htmlCode);

			HtmlNodeCollection divTime = null;
			HtmlNodeCollection divCloud = null;
			HtmlNodeCollection divTemp = null;
			HtmlNodeCollection divWind = null;
			HtmlNodeCollection divPress = null;
			try
			{
				divTime = doc.DocumentNode.SelectNodes("//*[contains(@class,'_line timeline clearfix')]");
				divCloud = doc.DocumentNode.SelectNodes("//*[contains(@class,'_line iconline clearfix')]");
				divTemp = doc.DocumentNode.SelectNodes("//*[contains(@class,'_line templine clearfix')]");
				divWind = doc.DocumentNode.SelectNodes("//*[contains(@class,'_line windline js_wind clearfix')]");
				divPress = doc.DocumentNode.SelectNodes("//*[contains(@class,'_line nil pressureline_wrap js_pressure clearfix')]");
			}
			catch
			{

			}

			Time[] time = new Time[9];
			for (int i = 0; i < 9; i++)
			{
				time[i] = new Time();
				time[i].title = divTime[0].ChildNodes[i].ChildNodes[0].FirstChild.InnerText + ":" + divTime[0].ChildNodes[i].ChildNodes[0].LastChild.InnerText;
				time[i].cloud = divCloud[0].ChildNodes[i].ChildNodes[1].InnerText;
				time[i].temp = divTemp[0].ChildNodes[0].ChildNodes[i + 1].Attributes["data-air"].Value;
				time[i].windVelocity = divWind[0].ChildNodes[i + 1].ChildNodes[1].Attributes["data-value"].Value;
				time[i].windDirection = "";
				for (int j = 0; j < divWind[0].ChildNodes[i + 1].ChildNodes[1].ChildNodes.Count; j++)
				{
					for (int k = 0; k < divWind[0].ChildNodes[i + 1].ChildNodes[1].ChildNodes[j].Attributes.Count; k++)
					{
						if (divWind[0].ChildNodes[i + 1].ChildNodes[1].ChildNodes[j].Attributes[k].Value == "item_unit")
						{
							if (divWind[0].ChildNodes[i + 1].ChildNodes[1].ChildNodes[j].InnerText == "штиль")
							{
								time[i].windDirection = "ш";
								break;
							}
							else
							{
								time[i].windDirection = divWind[0].ChildNodes[i + 1].ChildNodes[1].ChildNodes[j].InnerText;
								break;
							}
						}
					}
				}
				time[i].press = divPress[0].ChildNodes[1].ChildNodes[0].ChildNodes[i + 1].Attributes["data-pressure"].Value;
			}

			WeatherToForm(time);
		}
	}
}