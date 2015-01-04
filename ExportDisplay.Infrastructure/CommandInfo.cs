using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ExportDisplay.Infrastructure
{
	public class CommandInfo
	{
		public enum CommandTypeOption { MouseMove };
		public CommandTypeOption CommandType { get; set; }
		public string Data { get; set; }

		public CommandInfo(CommandTypeOption type, string data)
		{
			CommandType = type;
			Data = data;
		}

		public override string ToString()
		{
			return CommandType + "|" + Data;
		}

		public static CommandInfo Parse(string input)
		{
			string[] parts = input.Split('|');
			if (parts.Length != 2)
			{
				return null;
			}
			CommandTypeOption type = (CommandTypeOption)Enum.Parse(typeof(CommandTypeOption), parts[0]);
			string data = parts[1];
			return new CommandInfo(type, data);
		}
	}

	public class CommandInfoCollection
	{
		private readonly Queue<CommandInfo> _cmds = new Queue<CommandInfo>();
		public string SerializeCommandStack()
		{
			StringBuilder bld = new StringBuilder();
			lock (_cmds)
			{
				foreach (CommandInfo cmd in _cmds)
				{
					bld.Append(cmd.ToString());
					bld.Append(";");
				}
				_cmds.Clear();
			}

			return bld.ToString();
		}
		public void DeserializeCommandStack(string input)
		{
			lock (_cmds)
			{
				string[] parts = input.Split(';');
				foreach (string part in parts)
				{
					string trimmedPart = part.Trim();
					if (!string.IsNullOrEmpty(trimmedPart))
					{
						CommandInfo cmd = CommandInfo.Parse(trimmedPart);
						if (cmd != null)
						{
							_cmds.Enqueue(cmd);
						}
					}
				}
			}
		}

		public void Add(CommandInfo.CommandTypeOption type, string data)
		{
			CommandInfo cmd = new CommandInfo(type, data);
			Add(cmd);
		}

		public void Add(CommandInfo cmd)
		{
			lock (_cmds)
			{
				_cmds.Enqueue(cmd);
			}
		}

		public CommandInfo GetNextCommand()
		{
			CommandInfo cmd = null;
			lock (_cmds)
			{
				try
				{
					cmd = _cmds.Dequeue();
				}
				catch
				{
					// Do something with the exception
				};
			}

			return cmd;
		}
	}

	public static class Command
	{
		public static void Execute(CommandInfo cmd)
		{
			if (cmd.CommandType == CommandInfo.CommandTypeOption.MouseMove)
			{
				MouseMove(cmd.Data);
			}
		}

		private static void MouseMove(string data)
		{
			string[] parts = data.Split(',');
			if (parts.Length != 2)
			{
				return;
			}
			int cursorX = int.Parse(parts[0]);
			int cursorY = int.Parse(parts[1]);

			Cursor.Position = new System.Drawing.Point(cursorX, cursorY);
		}
	}
}
