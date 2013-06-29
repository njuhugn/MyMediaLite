// Copyright (C) 2013 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyMediaLite.Data
{
	public class Interactions : IInteractions
	{
		public int Count { get { return interaction_list.Count; } }

		public int MaxUserID { get; private set; }
		public int MaxItemID { get; private set; }
		public DateTime EarliestDateTime { get; private set; }
		public DateTime LatestDateTime { get; private set; }
		public IList<int> Users { get { return new List<int>(user_set); } }
		public IList<int> Items { get { return new List<int>(item_set); } }

		public IInteractionReader Random
		{
			get {
				// TODO synchronize
				if (random_interaction_list == null)
				{
					random_interaction_list = interaction_list.ToArray();
					random_interaction_list.Shuffle();
				}
				return new InteractionReader(random_interaction_list, user_set, item_set);
			}
		}
		IList<IInteraction> random_interaction_list;
		// TODO change protection level
		public IList<IInteraction> RandomInteractionList { get { return random_interaction_list; } }

		
		public IInteractionReader Sequential
		{
			get {
				return new InteractionReader(interaction_list, user_set, item_set);
			}
		}

		// IInteractionReader Chronological { get; }
		public IInteractionReader ByUser(int user_id) { throw new NotImplementedException(); }
		public IInteractionReader ByItem(int item_id) { throw new NotImplementedException(); }

		public RatingScale RatingScale { get; private set; }

		public bool HasRatings { get; private set; }
		public bool HasDateTimes { get; private set; }
		
		private IList<IInteraction> interaction_list;
		private ISet<int> user_set;
		private ISet<int> item_set;

		/// <summary>
		/// Initializes a new instance of the <see cref="MyMediaLite.Data.Interactions`1"/> class, taking a list of interactions
		/// </summary>
		/// <param name='interaction_list'>a list of interactions</param>
		public Interactions(IList<IInteraction> interaction_list)
		{
			this.interaction_list = interaction_list; // TODO consider defensive copy

			user_set = new HashSet<int>(from e in interaction_list select e.User);
			item_set = new HashSet<int>(from e in interaction_list select e.Item);
			MaxUserID = user_set.Max();
			MaxItemID = item_set.Max();

			if (HasRatings)
			{
				var ratings = (from e in interaction_list select e.Rating).Distinct();
				RatingScale = new RatingScale(ratings.ToList());
			}
			if (HasDateTimes)
			{
				EarliestDateTime = (from e in interaction_list select e.DateTime).Min();
				LatestDateTime = (from e in interaction_list select e.DateTime).Max();
			}
		}

		// TODO updates: batch interface, use immutable data structures in the background

		static public readonly char[] DEFAULT_SEPARATORS = new char[]{ '\t', ' ', ',' };

		// TODO move to different file
		static public Interactions FromFile(
			TextReader reader, IMapping user_mapping = null, IMapping item_mapping = null,
			int user_pos = 0, int item_pos = 1,
			char[] separators = null, bool ignore_first_line = false)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			if (ignore_first_line)
				reader.ReadLine();
			if (separators == null)
				separators = DEFAULT_SEPARATORS;

			int min_num_fields = Math.Max(user_pos, item_pos) + 1;

			var interaction_list = new List<IInteraction>();
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] tokens = line.Split(separators);

				if (tokens.Length < min_num_fields)
					throw new FormatException("Expected at least 2 columns: " + line);

				try
				{
					int user_id = user_mapping.ToInternalID(tokens[user_pos]);
					int item_id = item_mapping.ToInternalID(tokens[item_pos]);
					interaction_list.Add(new SimpleInteraction(user_id, item_id));
				}
				catch (Exception)
				{
					throw new FormatException(string.Format("Could not read line '{0}'", line));
				}
			}
			return new Interactions(interaction_list);
		}

		// TODO move to different file
		static public Interactions FromFile(
			TextReader reader, IMapping user_mapping = null, IMapping item_mapping = null,
			int user_pos = 0, int item_pos = 1, int rating_pos = 2, int datetime_pos = 3,
			char[] separators = null, bool ignore_first_line = false)
		{
			var interaction_list = new List<IInteraction>();
			return new Interactions(interaction_list);
			// TODO implement
		}

	}
}
