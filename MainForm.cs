using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WeatherDiary
{
	public partial class MainForm : Form
	{
		public class Time
		{
			public string title;
			public string cloud;
			public string temp;
			public string windVelocity;
			public string windDirection;
			public string press;
		}

		public class Year
		{
			public int num;
			public Month[] months;

		}

		public class Month
		{
			public int num;
			public Day[] days;

		}

		public class Day
		{
			public int num;
			public int temp;
			public string cloud;
			public int press;
			public int windVelocity;
			public string windDirection;
			public string prec;
		}

		private Year[] years = new Year[0];

		public MainForm()
		{
			InitializeComponent();
		}

		private void Open()
		{
			if (!File.Exists("Дневник погоды.xml"))
			{
				MessageBox.Show(@"Файл 'Дневник погоды.xml' не был найден!");
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
			Day newDay = new Day();
			Month newMonth = new Month();
			Year newYear = new Year();
			try
			{
				newDay.num = Calendar.SelectionRange.Start.Day;
				newDay.temp = Convert.ToInt32(tbTemp.Text);
				newDay.cloud = cbCloud.Text;
				newDay.press = Convert.ToInt32(tbPress.Text);
				newDay.windVelocity = Convert.ToInt32(tbWind.Text);
				newDay.windDirection = cbWind.Text;
				newDay.prec = cbPrec.Text;
				newMonth.num = Calendar.SelectionRange.Start.Month;
				newMonth.days = new Day[1];
				newMonth.days[0] = newDay;
				newYear.num = Calendar.SelectionRange.Start.Year;
				newYear.months = new Month[1];
				newYear.months[0] = newMonth;
			}
			catch
			{
				MessageBox.Show(@"Проверьте правильность заполнения всех полей!");
				return;
			}
			foreach (Year year in years)
			{
				if (year.num != Calendar.SelectionRange.Start.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != Calendar.SelectionRange.Start.Month) continue;
					foreach (Day day in month.days)
					{
						if (day.num != Calendar.SelectionRange.Start.Day) continue;
						day.temp = Convert.ToInt32(tbTemp.Text);
						day.cloud = cbCloud.Text;
						day.press = Convert.ToInt32(tbPress.Text);
						day.windVelocity = Convert.ToInt32(tbWind.Text);
						day.windDirection = cbWind.Text;
						day.prec = cbPrec.Text;
						return;
					}
					Array.Resize(ref month.days, month.days.Length + 1);
					month.days[month.days.Length - 1] = newDay;
					return;
				}
				Array.Resize(ref year.months, year.months.Length + 1);
				year.months[year.months.Length - 1] = newMonth;
				return;
			}
			Array.Resize(ref years, years.Length + 1);
			years[years.Length - 1] = newYear;
		}

		private Day FindDay(DateTime dt)
		{
			foreach (Year year in years)
			{
				if (year.num != dt.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != dt.Month) continue;
					foreach (Day day in month.days)
					{
						if (day.num == dt.Day)
						{
							return day;
						}
					}
				}
			}
			return new Day();
		}

		private void GetDayInfo()
		{
			Day day = FindDay(Calendar.SelectionRange.Start);
			lblTempDay.Text = (day.temp > 0 ? "+" : "") + $@"{day.temp}°С";
			lblCloudDay.Text = day.cloud;
			lblPressDay.Text = $@"{day.press} мм.рт.ст.";
			lblWindDay.Text = $@"{day.windVelocity} м/с; {day.windDirection}";
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
			DateTime dt = Convert.ToDateTime(Calendar.SelectionRange.Start.Day +
				"." + Calendar.SelectionRange.Start.Month + "." + cbDayInYear.Text);
			Day day = FindDay(dt);
			lblTempDayInYear.Text = (day.temp > 0 ? "+" : "") + $@"{day.temp}°С";
			lblCloudDayInYear.Text = day.cloud;
			lblPressDayInYear.Text = $@"{day.press} мм.рт.ст.";
			lblWindDayInYear.Text = $@"{day.windVelocity} м/с; {day.windDirection}";
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
			foreach (Year year in years)
			{
				if (year.num != Calendar.SelectionRange.Start.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != Calendar.SelectionRange.Start.Month) continue;
					foreach (Day day in month.days)
					{
						temp += (double)day.temp / month.days.Length;
						press += (double)day.press / month.days.Length;
						velocity += (double)day.windVelocity / month.days.Length;
					}
				}
			}
			lblTempMonth.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressMonth.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindMonth.Text = velocity.ToString("##.00") + @" м/с";
		}

		private void GetMonthInYearInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			DateTime dt = Convert.ToDateTime(Calendar.SelectionRange.Start.Day +
				"." + Calendar.SelectionRange.Start.Month + "." + cbMonthInYear.Text);
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			foreach (Year year in years)
			{
				if (year.num != dt.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != dt.Month) continue;
					foreach (Day day in month.days)
					{
						temp += (double)day.temp / month.days.Length;
						press += (double)day.press / month.days.Length;
						velocity += (double)day.windVelocity / month.days.Length;
					}
				}
			}
			lblTempMonthInYear.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressMonthInYear.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindMonthInYear.Text = velocity.ToString("##.00") + @" м/с";
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
			foreach (Year year in years)
			{
				if (year.num != Calendar.SelectionRange.Start.Year) continue;
				foreach (Month month in year.months)
				{
					foreach (Day day in month.days)
					{
						temp += day.temp;
						press += day.press;
						velocity += day.windVelocity;
						days++;
					}
				}
			}
			temp /= days;
			press /= days;
			velocity /= days;
			lblTempYear.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressYear.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindYear.Text = velocity.ToString("##.00") + @" м/с";
		}

		private void GetYearInYearInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			DateTime dt = Convert.ToDateTime(Calendar.SelectionRange.Start.Day +
				"." + Calendar.SelectionRange.Start.Month + "." + cbYearInYear.Text);
			double temp = 0.0;
			double press = 0.0;
			double velocity = 0.0;
			int days = 0;
			foreach (Year year in years)
			{
				if (year.num != dt.Year) continue;
				foreach (Month month in year.months)
				{
					foreach (Day day in month.days)
					{
						temp += day.temp;
						press += day.press;
						velocity += day.windVelocity;
						days++;
					}
				}
			}
			temp /= days;
			press /= days;
			velocity /= days;
			lblTempYearInYear.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressYearInYear.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindYearInYear.Text = velocity.ToString("##.00") + @" м/с";
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
			foreach (Year year in years)
			{
				if (year.num == Calendar.TodayDate.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != Calendar.TodayDate.Month) continue;
					foreach (Day day in month.days)
					{
						if (day.num != Calendar.TodayDate.Day) continue;
						temp += day.temp;
						press += day.press;
						velocity += day.windVelocity;
						days++;
					}
				}
			}
			temp /= days;
			press /= days;
			velocity /= days;
			lblTempDayForecast.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressDayForecast.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindDayForecast.Text = velocity.ToString("##.00") + @" м/с";
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
			foreach (Year year in years)
			{
				if (year.num == Calendar.TodayDate.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != Calendar.TodayDate.Month) continue;
					foreach (Day day in month.days)
					{
						temp += day.temp;
						press += day.press;
						velocity += day.windVelocity;
						days++;
					}
				}
			}
			temp /= days;
			press /= days;
			velocity /= days;
			lblTempMonthForecast.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressMonthForecast.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindMonthForecast.Text = velocity.ToString("##.00") + @" м/с";
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
			foreach (Year year in years)
			{
				if (year.num == Calendar.TodayDate.Year) continue;
				foreach (Month month in year.months)
				{
					foreach (Day day in month.days)
					{
						temp += day.temp;
						press += day.press;
						velocity += day.windVelocity;
						days++;
					}
				}
			}
			temp /= days;
			press /= days;
			velocity /= days;
			lblTempYearForecast.Text = (temp > 0.0 ? "+" : "") + temp.ToString("##.00") + @"°С";
			lblPressYearForecast.Text = press.ToString("###.00") + @" мм.рт.ст.";
			lblWindYearForecast.Text = velocity.ToString("##.00") + @" м/с";
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
				if (years[i].num == Calendar.SelectionRange.Start.Year) continue;
				for (int j = 0; j < years[i].months.Length; j++)
				{
					if (years[i].months[j].num != Calendar.SelectionRange.Start.Month) continue;
					for (int k = 0; k < years[i].months[j].days.Length; k++)
					{
						if (years[i].months[j].days[k].num != Calendar.SelectionRange.Start.Day) continue;
						if (years[i].months[j].days[k].temp > max)
						{
							max = years[i].months[j].days[k].temp;
							maxi = i; maxj = j; maxk = k;
						}
						if (years[i].months[j].days[k].temp >= min) continue;
						min = years[i].months[j].days[k].temp;
						mini = i; minj = j; mink = k;
					}
				}
			}
			lblTempDayMin.Text = (years[mini].months[minj].days[mink].temp > 0.0 ? "+" : "") + years[mini].months[minj].days[mink].temp + @"°С";
			lblPressDayMin.Text = years[mini].months[minj].days[mink].press.ToString();
			lblWindDayMin.Text = years[mini].months[minj].days[mink].windVelocity + @" м/с";
			lblTempDayMax.Text = (years[maxi].months[maxj].days[maxk].temp > 0.0 ? "+" : "") + years[maxi].months[maxj].days[maxk].temp + @"°С";
			lblPressDayMax.Text = years[maxi].months[maxj].days[maxk].press.ToString();
			lblWindDayMax.Text = years[maxi].months[maxj].days[maxk].windVelocity + @" м/с";
		}

		private void GetMonthMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double mintemp = 1000.0, minpress = 0.0, minvelocity = 0.0;
			double maxtemp = -1000.0, maxpress = 0.0, maxvelocity = 0.0;
			foreach (Year year in years)
			{
				if (year.num == Calendar.SelectionRange.Start.Year) continue;
				foreach (Month month in year.months)
				{
					if (month.num != Calendar.SelectionRange.Start.Month) continue;
					double temp = 0.0;
					double press = 0.0;
					double velocity = 0.0;
					foreach (Day day in month.days)
					{
						temp += (double)day.temp / month.days.Length;
						press += (double)day.press / month.days.Length;
						velocity += (double)day.windVelocity / month.days.Length;
					}
					if (temp > maxtemp)
					{
						maxtemp = temp;
						maxpress = press; maxvelocity = velocity;
					}
					if (!(temp < mintemp)) continue;
					mintemp = temp;
					minpress = press; minvelocity = velocity;
				}
			}
			lblTempMonthMin.Text = (mintemp > 0.0 ? "+" : "") + mintemp.ToString("##.00") + @"°С";
			lblPressMonthMin.Text = minpress.ToString("###.00");
			lblWindMonthMin.Text = minvelocity.ToString("##.00") + @" м/с";
			lblTempMonthMax.Text = (maxtemp > 0.0 ? "+" : "") + maxtemp.ToString("##.00") + @"°С";
			lblPressMonthMax.Text = maxpress.ToString("###.00");
			lblWindMonthMax.Text = maxvelocity.ToString("##.00") + @" м/с";
		}

		private void GetYearMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			double mintemp = 1000.0, minpress = 0.0, minvelocity = 0.0;
			double maxtemp = -1000.0, maxpress = 0.0, maxvelocity = 0.0;
			foreach (Year year in years)
			{
				if (year.num == Calendar.SelectionRange.Start.Year) continue;
				double temp = 0.0;
				double press = 0.0;
				double velocity = 0.0;
				int days = 0;
				foreach (Month month in year.months)
				{
					foreach (Day day in month.days)
					{
						temp += day.temp;
						press += day.press;
						velocity += day.windVelocity;
						days++;
					}
				}
				temp /= days;
				press /= days;
				velocity /= days;
				if (temp > maxtemp)
				{
					maxtemp = temp;
					maxpress = press; maxvelocity = velocity;
				}
				if (!(temp < mintemp)) continue;
				mintemp = temp;
				minpress = press; minvelocity = velocity;
			}
			lblTempYearMin.Text = (mintemp > 0.0 ? "+" : "") + mintemp.ToString("##.00") + @"°С";
			lblPressYearMin.Text = minpress.ToString("###.00");
			lblWindYearMin.Text = minvelocity.ToString("##.00") + @" м/с";
			lblTempYearMax.Text = (maxtemp > 0.0 ? "+" : "") + maxtemp.ToString("##.00") + @"°С";
			lblPressYearMax.Text = maxpress.ToString("###.00");
			lblWindYearMax.Text = maxvelocity.ToString("##.00") + @" м/с";
		}

		private void GetAbsoluteMinMaxInfo()
		{
			if (years.Length == 0)
			{
				return;
			}
			int mintemp = 1000, minpress = 1000, minvelocity = 1000;
			int maxtemp = -1000, maxpress = -1000, maxvelocity = -1000;
			foreach (Year year in years)
			{
				foreach (Month month in year.months)
				{
					foreach (Day day in month.days)
					{
						if (year.num == Calendar.TodayDate.Year || month.num == Calendar.TodayDate.Month ||
						    day.num == Calendar.TodayDate.Day) continue;
						if (day.temp > maxtemp)
						{
							maxtemp = day.temp;
						}
						if (day.temp < mintemp)
						{
							mintemp = day.temp;
						}
						if (day.press > maxpress)
						{
							maxpress = day.press;
						}
						if (day.press < minpress)
						{
							minpress = day.press;
						}
						if (day.windVelocity > maxvelocity)
						{
							maxvelocity = day.windVelocity;
						}
						if (day.windVelocity < minvelocity)
						{
							minvelocity = day.windVelocity;
						}
					}
				}
			}
			lblMinTemp.Text = (mintemp > 0.0 ? "+" : "") + mintemp + @"°С";
			lblMinPress.Text = minpress.ToString();
			lblMinWind.Text = minvelocity + @" м/с";
			lblMaxTemp.Text = (maxtemp > 0.0 ? "+" : "") + maxtemp + @"°С";
			lblMaxPress.Text = maxpress.ToString();
			lblMaxWind.Text = maxvelocity + @" м/с";
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			Open();
			foreach (Year year in years)
			{
				cbDayInYear.Items.Add(year.num);
				cbMonthInYear.Items.Add(year.num);
				cbYearInYear.Items.Add(year.num);
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
				//ignored
			}
			cbWind.Text = time[4].windDirection.ToLower();
			int clear = 0, mainlyCloud = 0;
			bool rain = false, snow = false, storm = false, hail = false;
			for (int i = 2; i < 7; i++)
			{
				if (time[i].cloud.ToLower().Contains("ясно"))
				{
					clear++;
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
			if (clear > 2 && mainlyCloud == 0)
			{
				cbCloud.Text = @"ясно";
			}
			else
			{
				if (mainlyCloud > 2 && clear == 0)
				{
					cbCloud.Text = @"пасмурно";
				}
				else
				{
					cbCloud.Text = @"облачно";
				}
			}
			if (!rain && !snow && !storm && !hail)
			{
				cbPrec.Text = "";
			}
			if (rain && !snow && !storm && !hail)
			{
				cbPrec.Text = @"дождь";
			}
			if (!rain && snow && !storm)
			{
				cbPrec.Text = @"снег";
			}
			if (!rain && !snow && storm && !hail)
			{
				cbPrec.Text = @"гроза";
			}
			if (!rain && !snow && !storm && hail)
			{
				cbPrec.Text = @"град";
			}
			if (rain && snow && !storm && !hail)
			{
				cbPrec.Text = @"дождь со снегом";
			}
			if (rain && !snow && storm && !hail)
			{
				cbPrec.Text = @"дождь с грозой";
			}
		}

		private string GetHtml(string adress)
		{
			WebRequest.DefaultWebProxy = new WebProxy();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(adress);
			request.Proxy = null;
			request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
			request.Headers.Add("Accept-Language: ru-Ru,ru;q=0.5");
			request.Headers.Add("Accept-Charset: Windows-1251,utf-8;q=0.7,*;q=0.7");
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				string encoding = response.Headers["Content-Type"].Split(new[] { "charset=" }, StringSplitOptions.RemoveEmptyEntries)[1];
				StringBuilder sb = new StringBuilder();
				byte[] buf = new byte[8192];
				int b;
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
			string htmlCode = GetHtml("https://www.gismeteo.ru/weather-kirov-4292/");
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
				divWind = doc.DocumentNode.SelectNodes("//*[contains(@class,'widget__row widget__row_table')]");
				divPress = doc.DocumentNode.SelectNodes("//*[contains(@class,'js_pressure pressureline w_pressure')]");
			}
			catch
			{
				// ignored
			}

			Time[] time = new Time[8];
			for (int i = 0; i < 8; i++)
			{
				time[i] = new Time
				{
					title = $"{divTime?[0].ChildNodes[i].ChildNodes[0].FirstChild.InnerText}:{divTime?[0].ChildNodes[i].ChildNodes[0].LastChild.InnerText}",
					cloud = ConvertCloud(divCloud?[0].ChildNodes[i].ChildNodes[0].Attributes["data-text"].Value),
					temp = divTemp?[0].ChildNodes[0].ChildNodes[i + 1].Attributes["data-value"].Value,
					windVelocity = divWind?[0].ChildNodes[i].ChildNodes[0].ChildNodes[0].Attributes["data-value"].Value,
					windDirection = ConvertWindDirection(divWind?[0].ChildNodes[i].ChildNodes[0].ChildNodes[2].InnerText),
					press = divPress?[0].ChildNodes[0].ChildNodes[i + 1].Attributes["data-value"].Value
				};
			}

			WeatherToForm(time);
		}

		private string ConvertCloud(string cloud)
		{
			Regex regex = new Regex("\n            <nobr>(\\w*)</nobr>\n                    ");
			return regex.Match(cloud.ToLower()).Groups[1].Value;
		}

		private string ConvertWindDirection(string windDirection)
		{
			Regex regex = new Regex("\n                    (\\w*)\n            ");
			string newWindDirection = regex.Match(windDirection.ToLower()).Groups[1].Value;
			if (newWindDirection == "штиль")
			{
				newWindDirection = "ш";
			}
			return newWindDirection;
		}
	}
}