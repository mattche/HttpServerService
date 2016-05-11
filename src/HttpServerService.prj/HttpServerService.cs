using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using HttpServerCore;

namespace HttpServerService
{
	/// <summary>Класс, предоставляющий функциональность Windows-службы, 
	/// обрабатывающей поступающие на локальный узел HTTP-запросы.</summary>
	public partial class HttpServerService : ServiceBase
	{
		#region .ctor

		/// <summary>Создаёт <see cref="HttpServerService"/>.</summary>
		public HttpServerService()
		{
			InitializeComponent();

			ServiceName = "HttpServerService";

			// Настройка логгирования
			if (!EventLog.SourceExists(ServiceName))
				EventLog.CreateEventSource(ServiceName, _eventLog.Log);
			_eventLog.Source = ServiceName;
			_eventLog.EnableRaisingEvents = true;
			AutoLog = false;
		}

		#endregion

		#region Methods

		/// <summary>Определяет действия, выполняемые при запуске службы.</summary>
		/// <param name="args">Аргументы командной строки</param>
		protected override void OnStart(string[] args)
		{
			string host;
			int port;

			// Считывание настроек из App.config
			try
			{
				host = ConfigurationManager.AppSettings["host"];
				port = int.Parse(ConfigurationManager.AppSettings["port"]);
			}
			catch
			{
				_eventLog.WriteEntry("Не удалось прочесть настройки из файла конфигурации.", 
					EventLogEntryType.Warning);
				host = Validation.DefaultHost;
				port = Validation.DefaultPort;
			}

			try
			{
				_server = new HttpServer(host, port);

				_server.ServerLogEvent += OnServerLogEvent;

				_backgroundThread = new Thread(_server.Start);
				_backgroundThread.IsBackground = true;
				_backgroundThread.Name = "HttpServer_thread";
				_eventLog.WriteEntry("Служба успешно запущена.", EventLogEntryType.Information);
				_backgroundThread.Start();
			}

			catch (ArgumentException ex)
			{
				_eventLog.WriteEntry(string.Format("Возникла ошибка: {0}.", ex.Message), 
					EventLogEntryType.Error);
			}
			catch (PlatformNotSupportedException ex)
			{
				_eventLog.WriteEntry(string.Format("Данная операционная система не поддерживается. " +
					"Возникла ошибка: {0}.", ex.Message), EventLogEntryType.Error);
			}
			catch (Exception ex)
			{
				_eventLog.WriteEntry(string.Format("Возникла ошибка: {0}.", ex.Message), 
					EventLogEntryType.Error);
			}
		}

		/// <summary>Определяет действия, выполняемые при останове службы.</summary>
		protected override void OnStop()
		{
			_server.ServerLogEvent -= OnServerLogEvent;
			_backgroundThread.Abort();
			_eventLog.WriteEntry("Служба успешно остановлена.", EventLogEntryType.Information);
		}

		#endregion

		#region Handlers

		/// <summary>Обрабатывает событие лога объекта <see cref="HttpServer"/>.</summary>
		/// <param name="sender">Отправитель</param>
		/// <param name="msg">Сообщение лога</param>
		private void OnServerLogEvent(object sender, string msg)
		{
			_eventLog.WriteEntry(msg, EventLogEntryType.Information);
		}

		#endregion

		#region Data

		/// <summary>Объект фонового рабочего потока.</summary>
		private Thread _backgroundThread;
		/// <summary>Объект HTTP-сервера.</summary>
		private HttpServer _server;

		#endregion
	}
}
