using System;
using System.ServiceProcess;
using System.Configuration;
using System.Net;
using HttpServerCore;

namespace HttpServerService
{
	/// <summary>Предоставляет точку входа приложения, а также методы работы с консолью.</summary>
	public static class Program
	{
		#region Methods 

		/// <summary>Главная точка входа для приложения.</summary>
		public static void Main()
		{
			if (!Environment.UserInteractive)
				using(var service = new HttpServerService())
					ServiceBase.Run(service);
			else
				RunInConsole();
		}

		/// <summary>Выполняет настройку и запуск HTTP-сервера в режиме консоли.</summary>
		private static void RunInConsole()
		{
			HttpServer server;
			string host;
			int port;

			try
			{
				host = ConfigurationManager.AppSettings["host"];
				port = int.Parse(ConfigurationManager.AppSettings["port"]);
			}
			catch
			{
				Console.WriteLine("Не удалось прочесть настройки из файла конфигурации.");
				host = Validation.DefaultHost;
				port = Validation.DefaultPort;
			}
			
			try
			{
				server = new HttpServer(host, port);
				server.ServerLogEvent += OnServerLogEvent;
				server.Start();
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine(ex.Message);
				Console.Read();
			}
			catch (HttpListenerException ex)
			{
				Console.WriteLine(string.Format("Возникла ошибка: {0}.", ex.Message));
				Console.WriteLine("Возможно, приложение заупущено не от имени администратора.");
				Console.Read();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Возникла ошибка: " + ex.Message);
				Console.Read();
			}
		}
		
		#endregion

		#region Handlers

		/// <summary>Обрабатывает событие лога HTTP-сервера.</summary>
		/// <param name="sender">Отправитель</param>
		/// <param name="msg">Сообщение</param>
		private static void OnServerLogEvent(object sender, string msg)
		{
			Console.WriteLine(msg);
		}

		#endregion
	}
}
