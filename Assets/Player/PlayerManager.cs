﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace RTS
{		
		public static class PlayerManager
		{
				private struct PlayerDetails
				{
						private string name;
						private int avatar;
						public PlayerDetails (string name, int avatar)
						{
								this.name = name;
								this.avatar = avatar;
						}
						public string Name { get { return name; } }
						public int Avatar { get { return avatar; } }
				}
				private static List<PlayerDetails> players = new List<PlayerDetails> ();
				private static PlayerDetails currentPlayer;
				private static Texture2D[] avatars;
				
				public static void SetAvatarTextures (Texture2D[] avatarTextures)
				{
						avatars = avatarTextures;
				}
				
				public static Texture2D GetPlayerAvatar ()
				{
						if (currentPlayer.Avatar >= 0 && currentPlayer.Avatar < avatars.Length)
								return avatars [currentPlayer.Avatar];
						return null;
				}
				
				public static void SelectPlayer (string name, int avatar)
				{
						//check to see if player already exists
						bool playerExists = false;
						foreach (PlayerDetails player in players) {
								if (player.Name == name) {
										currentPlayer = player;
										playerExists = true;
								}
						}
						if (!playerExists) {
								PlayerDetails newPlayer = new PlayerDetails (name, avatar);
								players.Add (newPlayer);
								currentPlayer = newPlayer;
								Directory.CreateDirectory ("SavedGames" + Path.DirectorySeparatorChar + name);
						}
						Save ();
				}
				
				public static string GetPlayerName ()
				{
						return currentPlayer.Name == "" ? "Unknown" : currentPlayer.Name;
				}
				
				public static void Save ()
				{
						JsonSerializer serializer = new JsonSerializer ();
						serializer.NullValueHandling = NullValueHandling.Ignore;
						using (StreamWriter sw = new StreamWriter("SavedGames" + Path.DirectorySeparatorChar + "Players.json")) {
								using (JsonWriter writer = new JsonTextWriter(sw)) {
										writer.WriteStartObject ();
										writer.WritePropertyName ("Players");
										writer.WriteStartArray ();
										foreach (PlayerDetails player in players)
												SavePlayer (writer, player);
										writer.WriteEndArray ();
										writer.WriteEndObject ();
								}
						}
				}
				
				private static void SavePlayer (JsonWriter writer, PlayerDetails player)
				{
						writer.WriteStartObject ();
						
						writer.WritePropertyName ("Name");
						writer.WriteValue (player.Name);
						writer.WritePropertyName ("Avatar");
						writer.WriteValue (player.Avatar);
						
						writer.WriteEndObject ();
				}
				
				public static void Load ()
				{
						players.Clear ();
						
						string filename = "SavedGames" + Path.DirectorySeparatorChar + "Players.json";
						if (File.Exists (filename)) {
								//read file
								string input;
								using (StreamReader reader  = new StreamReader(filename)) {
										input = reader.ReadToEnd ();
								}
								if (input != null) {
										//parse file contents
										using (JsonTextReader reader = new JsonTextReader(new StringReader(input))) {
												while (reader.Read()) {
														if (reader.Value != null) {
																if (reader.TokenType == JsonToken.PropertyName) {
																		if ((string)reader.Value == "Players")
																				LoadPlayers (reader);
																}
														}
												}
										}
								}
						}
				}
				
				private static void LoadPlayers (JsonTextReader reader)
				{
						while (reader.Read()) {
								if (reader.TokenType == JsonToken.StartObject)
										LoadPlayer (reader);
								else if (reader.TokenType == JsonToken.EndArray)
										return;
						}
				}
				
				private static void LoadPlayer (JsonTextReader reader)
				{
						string currValue = "";
						string name = "";
						int avatar = 0;
						
						while (reader.Read()) {
								if (reader.Value != null) {
										if (reader.TokenType == JsonToken.PropertyName) {
												currValue = (string)reader.Value;
										} else {
												switch (currValue) {
												case "Name":
														name = (string)reader.Value;
														break;
												case "Avatar":
														avatar = (int)(System.Int64)reader.Value;
														break;
												default:
														break;
												}
										}
								} else {
										if (reader.TokenType == JsonToken.EndObject) {
												players.Add (new PlayerDetails (name, avatar));
												return;
										}
								}
						}
				}
		}
}