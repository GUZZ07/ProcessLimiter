using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace ProcessLimiter
{
	public class Task
	{
		[JsonProperty(PropertyName = "任务序号")]		public int ID { get; set; }
		[JsonProperty(PropertyName = "任务名")]			public string Name { get; set; }
		[JsonProperty(PropertyName = "所需物品")]		public TaskItem[] ItemsRequired { get; set; }
		[JsonProperty(PropertyName = "奖励物品")]		public TaskItem[] ItemsToReward { get; set; }
		[JsonProperty(PropertyName = "禁封物品")]		public TaskItemID[] ItemsBanned { get; set; }
		[JsonProperty(PropertyName = "禁止生成的生物")]	public int[] NPCsBanned { get; set; }
		[JsonProperty(PropertyName = "任务描述")]		public string CustomDescription { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Introduction { get; private set; }

		public Task()
		{

		}

		public Task(int id, string name, TaskItem[] required, TaskItem[] reward = null, TaskItemID[] banned = null, int[] npcBanned = null)
		{
			ID = id;
			Name = name;
			ItemsRequired = required;
			ItemsToReward = reward;
			ItemsBanned = banned;
			NPCsBanned = npcBanned;
		}

		public void Load()
		{
			SetIntroduction();
		}

		public void SetIntroduction()
		{
			var itemList = @"所需物品:
" + string.Join("", ItemsRequired);

			string rewardList = string.Empty;
			if (ItemsToReward != null && ItemsToReward.Length > 0)
			{
				rewardList = @"
物品奖励:
" + string.Join("", ItemsToReward);
			}

			string itemsBanned = string.Empty;
			if (ItemsBanned != null && ItemsBanned.Length > 0)
			{
				itemsBanned = @"
该任务封印的物品: 
" + string.Join("", ItemsBanned);
			}


			Introduction = $@"任务#{ID}——{Name}
""{CustomDescription ?? string.Empty}""
{itemList}{rewardList}{itemsBanned}";
		}

		public bool CheckItems(Item[] items)
		{
			for (int i = 0; i < ItemsRequired.Length; i++)
			{
				if (!ItemsRequired[i].Match(items[i]))
				{
					string item = new TaskItem(items[i]).ToString();
					TSPlayer.All.SendMessage($"所需物品{ItemsRequired[i]}与实际物品{item}不符，请检查顺序是否正确", 0, 0, 255);
					return false;
				}
				else
				{
					string item = new TaskItem(items[i]).ToString();
					TSPlayer.All.SendSuccessMessage($"所需物品{ItemsRequired[i]}与实际物品{item}相符");
				}
			}
			return true;
		}
		public bool CheckChest(Chest chest)
		{
			TSPlayer.All.SendInfoMessage("正在检查物品...");
			return CheckItems(chest.item);
		}
		public bool Check(TSPlayer checker)
		{
			if (checker.ActiveChest == -1)
			{
				checker.SendErrorMessage("请先打开一个箱子");
				return false;
			}
			var chest = Main.chest[checker.TPlayer.chest];
			if (CheckChest(chest))
			{
				EatChest(chest);
				RewardItems(chest);
				TSPlayer.All.SendSuccessMessage($"主线任务#{ID}已完成，奖励详见箱子");
				if (ItemsBanned != null && ItemsBanned.Length > 0)
				{
					var list = string.Join("", ItemsBanned);
					TSPlayer.All.SendSuccessMessage($"以下物品已解禁:\n{list}");
				}
				checker.SendData(PacketTypes.ChestOpen, "", -1);
				return true;
			}
			else
			{
				checker.SendErrorMessage("任务条件未达成，完成失败");
				return false;
			}
		}

		public void UpdatePlayer(TSPlayer player, bool broadCast = false)
		{
			if (ItemsBanned == null || ItemsBanned.Length == 0)
			{
				return;
			}
			#region Banned
			bool banned(int id)
			{
				foreach (var item in ItemsBanned)
				{
					if (item.ID == id)
					{
						return true;
					}
				}
				return false;
			}
			bool Isbanned(Item item) => banned(item.type);
			#endregion
			#region CheckItem
			bool CheckItem(Item item)
			{
				if (Isbanned(item))
				{
					var tag = new TaskItem(item);
					var reason = $"{tag}已被神秘力量封印\n完成任务#{ID}以解开封印";
					player.Disable(reason, DisableFlags.None);
					if (broadCast)
					{
						player.SendMessage(reason, 0, 0, 255);
					}
					return true;
				}
				return false;
			}
			#endregion
			var tplayer = player.TPlayer;
			if (CheckItem(tplayer.HeldItem))
			{
				return;
			}
			foreach (var item in tplayer.armor)
			{
				if (CheckItem(item))
				{
					break;
				}
			}
		}
		public void Update()
		{
			foreach (var npc in Main.npc)
			{
				if (!npc.active)
				{
					continue;
				}
				if (NPCsBanned.Contains(npc.type))
				{
					npc.active = false;
					TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npc.whoAmI);
				}
			}
		}

		public void RewardItems(Chest chest)
		{
			if (ItemsToReward == null || ItemsToReward.Length == 0)
			{
				return;
			}
			foreach(var item in ItemsToReward)
			{
				var chestItem = new Item();
				chestItem.netDefaults(item.ID);
				chestItem.stack = item.Stack;
				chestItem.prefix = item.Prefix;
				chest.AddItemToShop(chestItem);
			}
		}
		public static void EatChest(Chest chest)
		{
			chest.item.ForEach(item => item.netDefaults(0));
		}

	}
}
