using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using TShockAPI;

namespace ProcessLimiter
{
	public class ProcessData
	{
		public Task[] Tasks { get; set; }
		public int Current { get; set; }
		internal Task CurrentTask => Current >= Tasks.Length ? null : Tasks[Current];

		private int timer;

		public ProcessData()
		{
			Tasks = new[]
			{
				new Task
				{
					ID = 1,
					Name="模板任务",
					ItemsRequired = new TaskItem[] { (ItemID.Wood, 20 ), ItemID.Gel, (ItemID.Torch, 20) },
					ItemsToReward = new TaskItem[] { (ItemID.FallenStar, 10) },
					ItemsBanned = new TaskItem[] { (ItemID.Starfury,1), (ItemID.Minishark,1) },
					NPCsBanned = new int[] { NPCID.EyeofCthulhu }
				}
			};
		}

		public void Load()
		{
			foreach(var task in Tasks)
			{
				task.Load();
			}
		}

		public void Update()
		{
			timer++;
			if (timer % 60 == 0)
			{
				for (int i = Current; i < Tasks.Length; i++)
				{
					foreach (var player in TShock.Players)
					{
						if (player?.Active != true)
						{
							continue;
						}
						Tasks[i].UpdatePlayer(player, timer % 180 == 0);
					}
				}
			}
			for (int i = Current; i < Tasks.Length; i++)
			{
				Tasks[i].Update();
			}
		}

		public void ShowTo(TSPlayer player)
		{
			var task = CurrentTask;
			if (task == null)
			{
				player.SendInfoMessage("当前任务已完结");
			}
			else
			{
				player.SendInfoMessage(task.Introduction);
			}
		}

		public bool TryCheck(TSPlayer player)
		{
			var task = CurrentTask;
			if (task == null)
			{
				player.SendInfoMessage("当前任务已完结");
				return false;
			}
			if (task.Check(player))
			{
				Current++;
				MainPlugin.Instance.SaveData();
				return true;
			}
			return false;
		}

		public static ProcessData Deserialize(string value)
		{
			return JsonConvert.DeserializeObject<ProcessData>(value);
		}
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
	}
}
