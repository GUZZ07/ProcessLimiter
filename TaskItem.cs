using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Terraria;
using Terraria.ID;

namespace ProcessLimiter
{
	public struct TaskItem
	{
		private static short MaxID = ItemID.Count;

		[JsonConverter(typeof(TaskItemID.IDConvert))]
		public TaskItemID ID;
		public int Stack;
		public byte Prefix;
		public TaskItem(TaskItemID id, int stack = 1, int prefix = 0)
		{
			ID = id;
			Stack = stack;
			Prefix = (byte)prefix;

			if (id >= MaxID)
			{
				ID = ItemID.Gel;
				Stack = 1;
				Prefix = 0;
			}
		}
		public TaskItem(Item item) : this((short)item.type, item.stack, item.prefix)
		{

		}
		public bool Match(Item item)
		{
			bool flag = true;
			flag &= ID == item.type;
			flag &= Stack <= item.stack;
			if (Prefix != 0)
			{
				flag &= Prefix == item.prefix;
			}
			return flag;
		}
		public override string ToString()
		{
			if (Prefix == 0)
			{
				return string.Format("[i/s{0}:{1}]", Stack, ID);
			}
			else
			{
				return string.Format("[i/p{0}:{1}]", Prefix, ID);
			}
		}


		public static implicit operator TaskItem(TaskItemID value)
		{
			return new TaskItem(value);
		}
		public static implicit operator TaskItem(short value)
		{
			return new TaskItem(value);
		}
		public static implicit operator TaskItem((TaskItemID ID, int Stack) value)
		{
			return new TaskItem(value.ID, value.Stack);
		}
		public static implicit operator TaskItem((TaskItemID ID, int Stack, int Prefix) value)
		{
			return new TaskItem(value.ID, value.Stack, value.Prefix);
		}
	}
	public struct TaskItemID
	{
		#region Statics
		public static class EvilItems
		{
			public static class Keys
			{
				/// <summary>
				/// 腐肉 / 椎骨
				/// </summary>
				public const string Material1 = "EvilItems.Material_1";
				/// <summary>
				/// 咒火 / 脓血
				/// </summary>
				public const string Material2 = "EvilItems.Material_2";
				/// <summary>
				/// 蘑菇
				/// </summary>
				public const string Mushroom = "EvilItems.Mushroom";
				/// <summary>
				/// 腐化者 / 跳跳虫旗帜
				/// </summary>
				public const string MostEnemyBanner = "EvilItems.EnemyBanner";
				/// <summary>
				/// 地牢箱子钥匙
				/// </summary>
				public const string EnvironmentKey = "EvilItems.EnvironmentKey";
				/// <summary>
				/// 对应地牢箱子物品
				/// </summary>
				public const string DungeonWeapon = "EvilItems.DungeonWeapon";
				/// <summary>
				/// 虫子 / 脑子召唤物
				/// </summary>
				public const string BossSummoner = "EvilItems.BossSummoner";
			}

			public static bool HasKey(string key)
			{
				return items.ContainsKey(key);
			}

			public static short GetValue(string key)
			{
				return items[key];
			}

			public static string GetKey(short value)
			{
				return items.Where(pair => pair.Value == value).First().Key;
			}

			private static Dictionary<string, short> items;

			static EvilItems()
			{
				items = new Dictionary<string, short>()
				{
					[Keys.Material1] = ItemID.RottenChunk,
					[Keys.Mushroom] = ItemID.VileMushroom,
					[Keys.Material2] = ItemID.CursedFlame,
					[Keys.MostEnemyBanner] = ItemID.CorruptorBanner,
					[Keys.EnvironmentKey] = ItemID.CorruptionKey,
					[Keys.DungeonWeapon] = ItemID.ScourgeoftheCorruptor,
					[Keys.BossSummoner] = ItemID.WormFood
				};
				if (WorldGen.crimson)
				{
					items[Keys.Material1] = ItemID.Vertebrae;
					items[Keys.Mushroom] = ItemID.ViciousMushroom;
					items[Keys.Material2] = ItemID.Ichor;
					items[Keys.MostEnemyBanner] = ItemID.HerplingBanner;
					items[Keys.EnvironmentKey] = ItemID.CrimsonKey;
					items[Keys.DungeonWeapon] = ItemID.VampireKnives;
					items[Keys.BossSummoner] = ItemID.BloodySpine;
				}

			}
		}
		public static Dictionary<short, string> Map { get; private set; }
		public static Dictionary<string, short> UnMap { get; private set; }
		#endregion
		#region Convert
		public class IDConvert : JsonConverter
		{
			static IDConvert()
			{
				var Literals = typeof(ItemID).GetFields().Where(field => field.IsLiteral);

				Map = new Dictionary<short, string>(Literals.Count());
				UnMap = new Dictionary<string, short>(Literals.Count(), StringComparer.OrdinalIgnoreCase );

				short value;

				foreach (var literal in Literals)
				{
					value = (short)literal.GetRawConstantValue();
					Map.Add(value, literal.Name);
					UnMap.Add(literal.Name, value);
				}

				int Count = UnMap[nameof(ItemID.Count)];
				{
					string introduce = @"关于为什么会有这东西的存在:
	由于直接使用数字表示物品ID, 会导致可读性不高, 不便修改
	而且某些邪恶物品是成对的, 无法使用数字表示, 于是就用Key来表示
	Key列表如下所示



";
					StringBuilder SB = new StringBuilder(introduce, introduce.Length + Count * 30);

					SB.AppendLine("None: 0 // 不对应任何物品");

					for (short i = 1; i < Count; i++)
					{
						SB.AppendFormat("{0}: {1} // {2}", Map[i], i, Lang.GetItemName(i));
						SB.AppendLine();
					}

					SB.AppendLine(EvilItems.Keys.Material1 + ": 腐肉 / 椎骨");
					SB.AppendLine(EvilItems.Keys.Material2 + ": 咒火 / 脓血");
					SB.AppendLine(EvilItems.Keys.Mushroom + ": 当前世界对应邪恶蘑菇");
					SB.AppendLine(EvilItems.Keys.MostEnemyBanner + ": 腐化者 / 跳跳兽旗帜");
					SB.AppendLine(EvilItems.Keys.EnvironmentKey + ": 腐化 / 血腥钥匙");
					SB.AppendLine(EvilItems.Keys.DungeonWeapon + ": 腐蚀咒怨 / 吸血飞刀");
					SB.AppendLine(EvilItems.Keys.BossSummoner + ": 虫子 / 脑子召唤物");
					if (MainPlugin.Instance != null)
					{
						MainPlugin.Instance.ItemIDs = SB.ToString();
					}
				}
			}
			public override bool CanConvert(Type type)
			{
				return true;
			}
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				string id = (string)reader.Value;
				if (short.TryParse(id, out short result))
				{
					return (TaskItemID)result;
				}
				else if (EvilItems.HasKey(id))
				{
					return new TaskItemID
					{
						ID = EvilItems.GetValue(id),
						Specials = ItemIDType.Key
					};
				}
				try
				{
					return (TaskItemID)UnMap[id];
				}
				catch (KeyNotFoundException)
				{
					Console.WriteLine(id);
					throw;
				}
			}
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var id = (TaskItemID)value;
				if (id.ByKey)
				{
					writer.WriteValue(EvilItems.GetKey(id));
				}
				else
				{
					try
					{
						writer.WriteValue(Map[id]);
					}
					catch (KeyNotFoundException e)
					{
						Console.WriteLine(id);
						Console.WriteLine(e);
					}
				}
			}
		}
		#endregion
		public bool ByKey => Specials == ItemIDType.Key;
		public ItemIDType Specials;
		public short ID;
		public override string ToString()
		{
			if (Specials == ItemIDType.Key)
			{
				return Map[ID] + "(" + ID.ToString() + ")";
			}
			return ID.ToString();
		}

		public static implicit operator short(TaskItemID id)
		{
			return id.ID;
		}
		public static implicit operator TaskItemID(short id)
		{
			return new TaskItemID
			{
				ID = id
			};
		}
	}
	public enum ItemIDType : byte
	{
		Number,
		Key
	}
}
