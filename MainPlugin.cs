using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ProcessLimiter
{
	[ApiVersion(2, 1)]
	public class MainPlugin : TerrariaPlugin
	{
		public static MainPlugin Instance { get; private set; }

		public const string SavePath = "tshock/Tasks.json";
		public const string IntroductionPath = "tshock/TaskIntroduction.txt";

		public override string Author => "1413";
		public override Version Version => typeof(MainPlugin).Assembly.GetName().Version;
		public override string Name => "ProcessLimiter";
		public override string Description => "通过任务限制进度";

		public string ItemIDs { get; set; }
		public ProcessData Process { get; private set; }

		public MainPlugin(Main game) : base(game)
		{

		}
		public override void Initialize()
		{
			Instance = this;
			if (!File.Exists(IntroductionPath))
			{
				using var file = new FileStream(IntroductionPath, FileMode.Create);
				using var writer = new StreamWriter(file);
				writer.WriteLine(TextResource.AboutTasks_json);
				writer.WriteLine("\n\n\n\n\n\n\n\n\n\n");
				writer.WriteLine(ItemIDs);
			}
			ItemIDs = string.Empty;
			

			ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
			Commands.ChatCommands.Add(new Command("processlimiter.use", TaskCmd, "task", "tsks"));
		}
		public void SaveData()
		{
			File.WriteAllText(SavePath, Process.Serialize());
		}

		private void OnUpdate(object args)
		{
			if (Process == null)
			{
				LoadProcess();
			}
			Process.Update();
		}

		private void LoadProcess()
		{
			if (File.Exists(SavePath))
			{
				var text = File.ReadAllText(SavePath);
				Process = ProcessData.Deserialize(text);
			}
			else
			{
				Process = new ProcessData();
				SaveData();
			}
			Process.Load();
		}
		
		private void TaskCmd(CommandArgs args)
		{
			if (Process == null)
			{
				LoadProcess();
			}
			var player = args.Player;
			string option;
			if(args.Parameters.Count ==0)
			{
				option = string.Empty;
			}
			else
			{
				option = args.Parameters[0].ToLower();
			}
			switch(option)
			{
				case "check":
					Process.TryCheck(player);
					break;
				case "list":
					Process.ShowTo(player);
					break;
				default:
					player.SendInfoMessage("/task check    完成当前任务");
					player.SendInfoMessage("/task list     查看当前任务");
					break;
			}
		}
	}
}
