﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Consol
{
    /// <summary>
    /// Collection of utility functions to find players, trim text, and do basic tasks.
    /// </summary>
    internal static class Util
    {
        private static readonly Regex s_tagStripPattern = new Regex(@"<((?:b)|(?:i)|(?:size)|(?:color)|(?:quad)|(?:material)).*?>(.*?)<\/\1>");

        /// <summary>
        /// Find a <see cref="Player"/> by their name. Case insensitive, and allows partial matches.
        /// </summary>
        /// <param name="name">Name of the player to lookup, or the start of their name to try for a partial match.</param>
        /// <param name="foundMultiple"><see langword="true"/> if there were multiple matches for the query, <see langword="false"/> if not.</param>
        /// <returns>
        /// <see cref="Player"/> found by the search, or <see langword="null"/> if no player with that name could be found or there were
        /// too many matches found.
        /// </returns>
        public static Player GetPlayerByName(string name, out bool foundMultiple)
        {
            foundMultiple = false;

            try
            {
                var query =
                    from player in Player.GetAllPlayers()
                    where player.GetPlayerName().ToLower().Simplified().StartsWith(name.ToLower())
                    select player;

                if (query.Count() > 1)
                {
                    // If there were multiple matches (e.g. two players named "Ben" and "Benjamin"), then try
                    // to find the exact match. If there's no exact match, the intent is unclear and we shouldn't process it.
                    foreach (Player player in query)
                    {
                        if (player.GetPlayerName().ToLower().Simplified().Equals(name.ToLower()))
                            return player;
                    }

                    foundMultiple = true;
                    return null;
                }

                return query.First();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Find a <see cref="Player"/> by their ID number.
        /// </summary>
        /// <param name="id">Player ID to find.</param>
        /// <returns><see cref="Player"/> object with the ID number <paramref name="id"/>.</returns>
        public static Player GetPlayerByID(long id)
        {
            try
            {
                return (from player in Player.GetAllPlayers() where player.GetPlayerID() == id select player).First();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Trims leading and trailing spaces, and collapses repeating spaces to a single space.
        /// </summary>
        /// <param name="value"><see langword="string"/> to clean up.</param>
        /// <returns>Simplified version of the string.</returns>
        public static string Simplified(this string value)
        {
            return Regex.Replace(value.Trim(), @"\s{2,}", " ");
        }

        /// <summary>
        /// Converts a text value to the corresponding type and returns it as a generic <see langword="object"/>.
        /// </summary>
        /// <param name="value">Text to convert to some object.</param>
        /// <param name="toType"><see cref="Type"/> value to convert to.</param>
        /// <returns>An <see langword="object"/> reference to the converted type.</returns>
        public static object StringToObject(string value, Type toType)
        {
            try
            {
                if (toType == typeof(Player))
                {
                    if (long.TryParse(value, out long id))
                    {
                        return GetPlayerByID(id);
                    }
                    else
                    {
                        Player player = GetPlayerByName(value, out bool foundMultiple);

                        if (player != null)
                            return player;
                        else if (foundMultiple)
                        {
                            Logger.Error($"Found multiple players with the name '{value}'", true);
                            return null;
                        }
                        else
                        {
                            Logger.Error($"No player named '{value}'", true);
                            return null;
                        }
                    }
                }
                else if (toType == typeof(string))
                    return value;
                else if (toType == typeof(bool))
                {
                    if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        return true;
                    else
                        return false;
                }

                return Convert.ChangeType(value, toType);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to convert '{value}' to type '{toType}': {e.Message}", true);
                return null;
            }
        }

        /// <summary>
        /// Translates a <see cref="Type"/> to a nice user-friendly name.
        /// </summary>
        /// <param name="type"><see cref="Type"/> to translate</param>
        /// <returns><see langword="string"/> containing the type name.</returns>
        public static string GetSimpleTypeName(Type type)
        {
            switch (type.Name)
            {
                case nameof(Int32):
                case nameof(Int64):
                {
                    return "Number";
                }
                case nameof(UInt32):
                case nameof(UInt64):
                {
                    return "(+)Number";
                }
                case nameof(Single):
                case nameof(Double):
                {
                    return "Decimal";
                }
                default:
                    return type.Name;
            }
        }

        /// <summary>
        /// Strips away any markdown tags such as b, i, color, etc. from Text label input.
        /// </summary>
        /// <param name="input">Text to sanitize.</param>
        /// <returns>A <see langword="string"/> containing the sanitized text.</returns>
        public static string StripTags(string input)
        {
            return s_tagStripPattern.Replace(input, (Match match) =>
            {
                return match.Groups[2].Value;
            });
        }

        /// <summary>
        /// Extension method to convert a list object to a nicely formatted string, since <see cref="List{T}.ToString"/> isn't helpful.
        /// </summary>
        /// <typeparam name="T">List type.</typeparam>
        /// <param name="input">List to convert to text.</param>
        /// <returns>A formatted <see langword="string"/> of the list's contents and type.</returns>
        public static string AsText<T>(this List<T> input)
        {
            string value = $"List<{typeof(T).Name}>(";

            foreach (var item in input)
                value += item.ToString() + ",";

            value = value.Remove(value.Length - 1);
            value += ")";

            return value;
        }
    }
}
