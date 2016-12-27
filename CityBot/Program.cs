using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Discord;

namespace CityBot
{
	class Program
	{
		static void Main() => new Program().Start();

		static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		bool inElection;
		bool electionRoundTwo;

		DiscordClient client;
		List<User> candidates = new List<User>();
		Dictionary<ulong, string> description = new Dictionary<ulong, string>();
		Dictionary<ulong, uint> votes = new Dictionary<ulong, uint>();
		Dictionary<ulong, DateTime> presidents = new Dictionary<ulong, DateTime>();

		Dictionary<int, string> rules = new Dictionary<int, string>();
		List<bool> isRuleAdminOnly = new List<bool>();
		Message ruleMsg;
		Channel ruleChannel;

		List<ulong> voted = new List<ulong>();
		ulong candidateRole = 238064642859991043;
		ulong presidentRole = 237394706940690432;
		ulong adminRole = 237414536389459968;
		ulong announcementChannel = 237391208492564481;
		ulong server = 229662782356848640;

		string prefix = "$";

		void Start()
		{
			client = new DiscordClient();
			client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");
			client.MessageReceived += async (s, e) =>
			{
				if (e.Message.Text.StartsWith(prefix, StringComparison.CurrentCulture))
				{
					try
					{
						var cmd = new string(e.Message.RawText.ToLower().Split(' ')[0].Skip(1).ToArray());
						string var;
						try
						{
							string[] t = e.Message.RawText.Split(' ').Skip(1).ToArray();
							var = string.Join(" ", t);
						}
						catch
						{
							var = null;
						}
						if (!e.Channel.IsPrivate)
						{
							if (cmd == "citybot")
							{
								await e.Channel.SendMessage($"I'm a bot that handles the main function of City Island. If you want to see the available commands, type `{prefix}help`.\nCreated by Julien12150.");
							}
							else if (cmd == "help")
							{
								await e.User.SendMessage("List of commands: ```\n" +
														 $"{prefix}candidates    - Show a list of candidates on this server.\n" +
														 $"{prefix}becomecddt    - Become a candidate, with a optional descrption.\n" +
														 $"{prefix}changedesc    - If you are a candidate, you can change your description.\n" +
														 $"{prefix}electionstats - Check the number of votes on each candidates during a election.\n" +
														 $"{prefix}vote          - Vote one of the candidate during a election. (Should be used in a DM)\n" +
														 $"{prefix}president     - Show the description about the current president.\n" +
														 $"{prefix}presidentlist - Show the list of every presidents that were president on this server.```");
								await e.Channel.SendMessage("Here you go, check your private messages.");
							}
							else if (cmd == "candidates")
							{
								RefreshCandidates(e.Server);
								foreach (User u in candidates)
								{
									string desc = "No description";
									foreach (KeyValuePair<ulong, string> id in description)
									{
										if (u.Id == id.Key)
										{
											if (!string.IsNullOrWhiteSpace(id.Value))
												desc = id.Value;
											break;
										}
									}
									await e.Channel.SendMessage($"{u.Name}: {desc}");
								}
							}
							else if (cmd == "becomecddt")
							{
								if (!e.User.HasRole(e.Server.GetRole(candidateRole)))
								{
									if (!inElection)
									{
										await e.User.AddRoles(e.Server.GetRole(candidateRole));
										if (!string.IsNullOrWhiteSpace(var))
											description.Add(e.User.Id, var);
										await e.Channel.SendMessage("Congratulation, you are now a candidate.");
										Save();
									}
									else
										await e.Channel.SendMessage("We are very sorry, but we are currently in a election");
								}
								else
								{
									await e.Channel.SendMessage($"You are already a candidate. If you want to change your description, please use the command `{prefix}changedesc`");
								}
							}
							else if (cmd == "changedesc")
							{
								if (e.User.HasRole(e.Server.GetRole(candidateRole)) || e.User.HasRole(e.Server.GetRole(presidentRole)))
								{
									if (var != null)
									{
										if (description.ContainsKey(e.User.Id))
										   description[e.User.Id] = var;
										else
										{
											description.Remove(e.User.Id);
											description.Add(e.User.Id, var);
										}
										await e.Channel.SendMessage("Done.");
										Save();
									}
									else
									{
										await e.Channel.SendMessage("No description to change into.");
									}
								}
								else
								{
									await e.Channel.SendMessage("You are not a candidate.");
								}
							}
							else if (cmd == "election")
							{
								if (e.User.ServerPermissions.ManageServer)
								{
									if (var == "1")
									{
										RefreshCandidates(e.Server);
										voted.Clear();
										foreach (User u in candidates)
										{
											votes.Add(u.Id, 0);
										}
										if (votes.Count > 2)
										{
											inElection = true;
											await e.Server.GetChannel(announcementChannel).SendMessage($"We are happy to announce that the election are starting now. To vote, please type `{prefix}vote` into my private messages to vote. You cannot vote twice, so make sure to make a good decision.");
											await e.Server.GetChannel(announcementChannel).SendMessage("Later, round two will starts, and you will vote again and choose between the two most voted candidate.");
										}
										else if (votes.Count == 2)
										{
											inElection = true;
											electionRoundTwo = true;
											await e.Channel.SendMessage($"Warning: Only 2 candidates, skipping to round 2. Please type `{prefix}election 3` later.");
											await e.Server.GetChannel(announcementChannel).SendMessage($"We are happy to announce that the election are starting now. To vote, please type `{prefix}vote` into my private messages to vote. You cannot vote twice, so make sure to make a good decision.");
											await e.Server.GetChannel(announcementChannel).SendMessage($"For some reason, there was only two candidates. Tommorow, a new president will come.");
										}
										else if (votes.Count == 1)
										{
											await e.Channel.SendMessage($"Warning: Only 1 candidate, skipping to end.");
											User u = candidates[0];
											await u.RemoveRoles(e.Server.GetRole(candidateRole));
											await u.AddRoles(e.Server.GetRole(presidentRole));
											User p = GetPresident(e.Server);
											if (p != null)
												await p.RemoveRoles(e.Server.GetRole(presidentRole));
											presidents.Add(u.Id, DateTime.UtcNow);
											voted.Clear();
											votes.Clear();
											await e.Server.GetChannel(announcementChannel).SendMessage($"For some reason, there was only one candidate. <@{u}> will president of this server for two weeks, and then the next election will starts.");
										}
										else if (votes.Count == 0)
										{
											await e.Channel.SendMessage($"Warning: Only 0 candidate, election canceled.");
											await e.Server.GetChannel(announcementChannel).SendMessage($"For some reason, there was no candidate at all, so the current president will still be president.");
										}
										Save();
									}
									else if (var == "2")
									{
										if (inElection)
										{
											RefreshCandidates(e.Server);
											electionRoundTwo = true;

											var twoBest = votes.OrderByDescending(pair => pair.Value).Take(2).ToDictionary(pair => pair.Key, pair => pair.Value);
											var lost = votes.OrderBy(pair => pair.Value).Take(votes.Count - 2).ToDictionary(pair => pair.Key, pair => pair.Value);
											foreach (ulong id in lost.Keys)
											{
												await e.Server.GetUser(id).RemoveRoles(e.Server.GetRole(candidateRole));
											}
											voted.Clear();
											votes.Clear();
											foreach (ulong id in twoBest.Keys)
											{
												votes.Add(id, 0);
											}
											RefreshCandidates(e.Server);

											await e.Server.GetChannel(announcementChannel).SendMessage($"Round two is starting right now. Type `{prefix}vote` in my private messages to vote between the two most voted peoples.");
											Save();
										}
										else
											await e.Channel.SendMessage("Type `{prefix}election 1` first.");
									}
									else if (var == "3")
									{
										if (inElection && electionRoundTwo)
										{
											RefreshCandidates(e.Server);
											inElection = false;
											electionRoundTwo = false;

											var best = votes.OrderByDescending(pair => pair.Value).Take(1).ToDictionary(pair => pair.Key, pair => pair.Value);
											var lost = votes.OrderBy(pair => pair.Value).Take(1).ToDictionary(pair => pair.Key, pair => pair.Value);
											foreach (User u in candidates)
											{
												await u.RemoveRoles(e.Server.GetRole(candidateRole));
											}
											User p = GetPresident(e.Server);
											if (p != null)
												await p.RemoveRoles(e.Server.GetRole(presidentRole));
											presidents.Add(best.ToArray()[0].Key, DateTime.UtcNow);
											await e.Server.GetUser(best.ToArray()[0].Key).AddRoles(e.Server.GetRole(presidentRole));
											voted.Clear();
											votes.Clear();
											await e.Server.GetChannel(announcementChannel).SendMessage($"Congratulation to <@{best.ToArray()[0].Key}> for winning this election! He's becoming president of this server for two weeks, and then the next election will starts.");
											Save();
										}
										else
											await e.Channel.SendMessage($"Type `{prefix}election 2` first.");
									}
									else
									{
										await e.Channel.SendMessage("You have not used the election command correctly. Syntax: ```\n" +
																	$"{prefix}election 1 - Starts the election\n" +
																	$"{prefix}election 2 - Starts the round two of the election\n" +
																	$"{prefix}election 3 - Ends the election, and changes the president```");
									}
								}
							}
							else if (cmd == "president")
							{
								User u = GetPresident(e.Server);
								if (u != null)
								{
									try
									{
										string desc = "No description";
										if (!string.IsNullOrWhiteSpace(description[u.Id]))
											desc = description[u.Id];
									}
									catch (Exception) { }
									await e.Channel.SendMessage($"The president:\n{u.Name}: {description[u.Id]}");
								}
								else
									await e.Channel.SendMessage($"There is currently no president. If you want to see the list of all previous presidents, type `{prefix}presidentlist`.");
							}
							else if (cmd == "presidentlist")
							{
								foreach (KeyValuePair<ulong, DateTime> kvp in presidents)
								{
									string desc = "No description";
									try
									{
										if (!string.IsNullOrWhiteSpace(description[kvp.Key]))
											desc = description[kvp.Key];
									}
									catch (Exception) { }
									await e.Channel.SendMessage($"{e.Server.GetUser(kvp.Key).Name} ({kvp.Value.ToString("MMM d, yyy h:mm:ss tt")}): {desc}");
								}
							}
							else if (cmd == "electionstats")
							{
								if (!inElection)
									await e.Channel.SendMessage("There is no election.");
								else
								{
									string msg = "```\n";
									foreach (KeyValuePair<ulong, uint> kvp in votes.OrderByDescending(pair => pair.Value))
									{
										if (kvp.Value <= 1)
											msg += $"{e.Server.GetUser(kvp.Key)}: {kvp.Value} vote\n";
										else
											msg += $"{e.Server.GetUser(kvp.Key)}: {kvp.Value} votes\n";
									}
									msg += "```";
									await e.Channel.SendMessage(msg);
								}
							}
							else if (cmd == "vote")
							{
								if (!string.IsNullOrWhiteSpace(var))
								{
									await e.Message.Delete();
									await e.User.SendMessage("Please vote here, not in the server.");
								}
							}
							else if (cmd == "setrule")
							{
								if (var.Split(' ').Length == 2)
								{
									if (e.User.HasRole(e.Server.GetRole(presidentRole)))
									{
										int i = int.Parse(var.Split(' ')[0]);
										if (i > rules.ToArray().Length)
										{
											int ii = prefix.Length + i.ToString().Length + 9;
											rules.Add(i, var.Substring(ii, var.Length - ii));
											isRuleAdminOnly.Add(false);
										}
										else
										{
											if (isRuleAdminOnly[i])
											{
												await e.Channel.SendMessage("You are not allowed to edit this rule.");
											}
											else
											{
												int ii = prefix.Length + i.ToString().Length + 9;
												rules.Remove(i);
												isRuleAdminOnly.RemoveAt(i);
												rules.Add(i, var.Substring(ii, var.Length - ii));
												isRuleAdminOnly.Add(false);
											}
										}
									}
									else if(e.User.HasRole(e.Server.GetRole(adminRole)))
									{
										int i = int.Parse(var.Split(' ')[0]);
										if (i > rules.ToArray().Length)
										{
											int ii = prefix.Length + i.ToString().Length + 9;
											rules.Add(i, var.Substring(ii, var.Length - ii));
											isRuleAdminOnly.Add(false);
											if(ruleMsg != null)
												await ruleMsg.Edit(MakeRuleMsg());
										}
										else
										{
											if (isRuleAdminOnly[i])
											{
												int ii = prefix.Length + i.ToString().Length + 9;
												rules.Remove(i);
												isRuleAdminOnly.RemoveAt(i);
												rules.Add(i, var.Substring(ii, var.Length - ii));
												isRuleAdminOnly.Add(true);
											}
											else
											{
												int ii = prefix.Length + i.ToString().Length + 9;
												rules.Remove(i);
												isRuleAdminOnly.RemoveAt(i);
												rules.Add(i, var.Substring(ii, var.Length - ii));
												isRuleAdminOnly.Add(false);
											}
											if(ruleMsg != null)
												await ruleMsg.Edit(MakeRuleMsg());
										}
									}
								}
								else
								{
									await e.Channel.SendMessage($"Correct syntax: `{prefix}setrule <number> <rule description>`");
								}
							}
							else if(cmd == "putrulemsg")
							{
								if (e.User.HasRole(e.Server.GetRole(adminRole)))
								{
									await ruleMsg.Delete();
									ruleChannel = e.Channel;
									ruleMsg = await e.Channel.SendMessage(MakeRuleMsg());
								}
							}
						}
						else
						{
							if (cmd == "vote")
							{
								RefreshCandidates(client.GetServer(server));
								if (inElection)
								{
									if (!voted.Contains(e.User.Id))
									{
										if (string.IsNullOrWhiteSpace(var))
										{
											await e.Channel.SendMessage("Here is the list of people you can vote to:");
											string msg = "```\n";
											foreach (User u in candidates)
											{
												if (u.Discriminator >= 1000)
													msg += $"{prefix}vote {u.Name}#{u.Discriminator}\n";
												else if (u.Discriminator >= 100)
													msg += $"{prefix}vote {u.Name}#0{u.Discriminator}\n";
												else if (u.Discriminator >= 10)
													msg += $"{prefix}vote {u.Name}#00{u.Discriminator}\n";
												else if (u.Discriminator >= 1)
													msg += $"{prefix}vote {u.Name}#000{u.Discriminator}\n";
											}
											msg += "```";
											await e.Channel.SendMessage(msg);
										}
										else
										{
											try
											{
												string Name = var.Split('#')[0];
												ushort Discriminator = ushort.Parse(var.Split('#')[1]);

												bool exist = false;
												foreach (User u in candidates)
												{
													if (Name == u.Name && Discriminator == u.Discriminator)
													{
														exist = true;
														votes[u.Id]++;
														voted.Add(e.User.Id);
														await e.Channel.SendMessage("Vote recorded, thanks for voting.");
														Save();
													}
												}
												if (!exist)
													throw new Exception();
											}
											catch (Exception)
											{
												await e.Channel.SendMessage("Please check if you didn't mispelled anything, or if you are voting someone that isn't a candidate or doesn't exist.");
											}
										}
									}
									else
									{
										await e.Channel.SendMessage("Sorry, but you already have voted.");
									}
								}
								else
								{
									await e.Channel.SendMessage("There is no election.");
								}
							}
						}
					}
					catch (Exception ex)
					{
						await e.Channel.SendMessage($"Warning, a exception has been thrown.");
						try
						{
							await e.Channel.SendMessage($"```{ex.ToString()}```");
						}
						catch (Exception)
						{
							await e.Channel.SendMessage($"The error message is too long to display, please contact Julien12150 for this exception.");
							Console.WriteLine(ex);
						}
					}
				}
			};
			client.ExecuteAndWait(async () =>
			{
				string token = File.ReadAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/token.txt");
				await client.Connect(token, TokenType.Bot);
				if (client.CurrentUser.Id != 249304387506274306)
				{
					Console.WriteLine("ID do not match, shutting down.");
					await client.Disconnect();
				}
				else
				{
					client.SetStatus(UserStatus.Online);
					client.SetGame("$citybot");
					Open();
				}
				while (true)
				{
					ConsoleKey k = Console.ReadKey(true).Key;
					if (k == ConsoleKey.Escape)
					{
						client.SetStatus(UserStatus.Invisible);
						await client.Disconnect();
						Save();
						return;
					}
				}
			});
		}
		User GetPresident(Server s)
		{
			User us = null;
			foreach (User u in s.Users)
			{
				if (u.HasRole(u.Server.GetRole(presidentRole)))
				{
					us = u;
				}
			}
			return us;
		}
		void RefreshCandidates(Server s)
		{
			candidates.Clear();
			foreach (User u in s.Users)
			{
				if (u.HasRole(u.Server.GetRole(candidateRole)))
				{
					candidates.Add(u);
				}
			}
		}
		string MakeRuleMsg()
		{
			string s = "";
			foreach(KeyValuePair<int, string> kvp in rules)
			{
				s += $"{kvp.Key}. {kvp.Value}";
			}
			return s;
		}
		void Save()
		{
			Console.WriteLine("Saving...");
			var f = new BinaryWriter(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/save.dat", FileMode.Create));
			f.Write(inElection);
			f.Write(electionRoundTwo);
			f.Write(prefix);
			f.Write(description.Count);
			foreach (KeyValuePair<ulong, string> kvp in description)
			{
				f.Write(kvp.Key);
				f.Write(kvp.Value);
			}
			f.Write(votes.Count);
			foreach (KeyValuePair<ulong, uint> kvp in votes)
			{
				f.Write(kvp.Key);
				f.Write(kvp.Value);
			}
			f.Write(voted.Count);
			foreach (ulong u in voted)
			{
				f.Write(u);
			}
			f.Flush();
			f.Close();
			var p = new StreamWriter(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/presidents.txt", FileMode.Create));
			foreach (KeyValuePair<ulong, DateTime> kvp in presidents)
			{
				p.WriteLine($"{kvp.Value},{(ulong)(kvp.Value - UnixEpoch).TotalSeconds}");
			}
			p.Flush();
			p.Close();
			Console.WriteLine("Done.");
			var r = new StreamWriter(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/rules.txt", FileMode.Create));
			if(ruleMsg != null)
				r.WriteLine(ruleMsg.Id);
			else
				r.WriteLine("n");
			if(ruleChannel != null)
				r.WriteLine(ruleChannel.Id);
			else
				r.WriteLine("n");
			for(int i = 0; i < rules.ToArray().Length; i++)
			{
				r.WriteLine($"{rules.Keys.ElementAt(i)}¥{isRuleAdminOnly[i]}¥{rules.Values.ElementAt(i)}");
			}
			r.Flush();
			r.Close();
		}
		void Open()
		{
			try
			{
				var f = new BinaryReader(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/save.dat", FileMode.Open));
				inElection = f.ReadBoolean();
				electionRoundTwo = f.ReadBoolean();
				prefix = f.ReadString();
				int descCount = f.ReadInt32();
				var descT = new Dictionary<ulong, string>();
				for (int i = 0; i < descCount; i++)
				{
					ulong t1 = f.ReadUInt64();
					string t2 = f.ReadString();
					Console.WriteLine($"{t1}: {t2}");
					descT.Add(t1, t2);
				}
				int votesCount = f.ReadInt32();
				var votesT = new Dictionary<ulong, uint>();
				for (int i = 0; i < votesCount; i++)
				{
					ulong t1 = f.ReadUInt64();
					uint t2 = f.ReadUInt32();
					votesT.Add(t1, t2);
					Console.WriteLine($"{t1}: {t2} votes");
				}
				int votedCount = f.ReadInt32();
				var votedT = new List<ulong>();
				for (int i = 0; i < votedCount; i++)
				{
					ulong t1 = f.ReadUInt64();
					votedT.Add(t1);
					Console.WriteLine($"{t1} voted");
				}
				description = descT;
				votes = votesT;
				voted = votedT;
				f.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't read the main save.");
				Console.WriteLine(e);
			}
			try
			{
				var p = new StreamReader(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/presidents.txt", FileMode.Open));
				string[] ps = p.ReadToEnd().Split('\n');
				foreach (string s in ps)
				{
					try
					{
						presidents.Add(ulong.Parse(s.Split(',')[0]), UnixEpoch.AddSeconds(ulong.Parse(s.Split(',')[1])));
					}
					catch (Exception) { }
				}
				p.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't read the presidents save.");
				Console.WriteLine(e);
			}
			try
			{
				var r = new StreamReader(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Julien12150/CityBot/rules.txt", FileMode.Open));
				string[] rs = r.ReadToEnd().Split('\n');
				try
				{
					ruleChannel = client.GetServer(server).GetChannel(ulong.Parse(rs[1]));
					ruleMsg = ruleChannel.GetMessage(ulong.Parse(rs[0]));
				}
				catch(Exception){}
				for(int i = 2; i < rs.Length; i++)
				{
					string s = rs[i];
					try
					{
						rules.Add(int.Parse(s.Split('¥')[0]), s.Split('¥')[2]);
						isRuleAdminOnly.Add(bool.Parse(s.Split('¥')[1]));
					}
					catch(Exception){ }
				}
				r.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Couldn't read the rules save.");
				Console.WriteLine(e);
			}
		}
		static string GetTimestamp(DateTime value)
		{
			return value.ToString("yyyyMMddHHmmss");
		}
	}
}
