using System;
using System.IO;

namespace SaveReader
{
	class MainClass
	{
		public static void Main(string[] args) //Run the application using CMD
		{
			ConsoleColor c = Console.ForegroundColor;
			if (args.Length == 0)
			{
				Console.WriteLine("Please put a file path.");
			}
			else
			{
				try
				{
					var f = new BinaryReader(new FileStream(args[0], FileMode.Open));
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("inElection=");
					Console.ForegroundColor = c;
					Console.WriteLine(f.ReadBoolean());
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("electionRoundTwo=");
					Console.ForegroundColor = c;
					Console.WriteLine(f.ReadBoolean());
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("prefix=");
					Console.ForegroundColor = c;
					Console.WriteLine(f.ReadString());
					int descCount = f.ReadInt32();
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("desc.Length=");
					Console.ForegroundColor = c;
					Console.WriteLine(descCount);
					for (int i = 0; i < descCount; i++)
					{
						ulong t1 = f.ReadUInt64();
						string t2 = f.ReadString();
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write($"desc[{i}].Key=");
						Console.ForegroundColor = c;
						Console.WriteLine(t1);
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write($"desc[{i}].Value=");
						Console.ForegroundColor = c;
						Console.WriteLine(t2);
					}
					int votesCount = f.ReadInt32();
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("votes.Length=");
					Console.ForegroundColor = c;
					Console.WriteLine(votesCount);
					for (int i = 0; i < votesCount; i++)
					{
						ulong t1 = f.ReadUInt64();
						uint t2 = f.ReadUInt32();
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write($"votes[{i}].Key=");
						Console.ForegroundColor = c;
						Console.WriteLine(t1);
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write($"votes[{i}].Value=");
						Console.ForegroundColor = c;
						Console.WriteLine(t2);
					}
					int votedCount = f.ReadInt32();
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("voted.Length=");
					Console.ForegroundColor = c;
					Console.WriteLine(votedCount);
					for (int i = 0; i < votedCount; i++)
					{
						ulong t1 = f.ReadUInt64();
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write($"voted[{i}]=");
						Console.ForegroundColor = c;
						Console.WriteLine(t1);
					}
					f.Close();
				}
				catch (Exception)
				{
					Console.WriteLine("Couldn't read the save.");
				}
			}
		}
	}
}
