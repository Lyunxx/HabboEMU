using Plus.HabboHotel.Groups.Structs;
using Plus.Messages;
using Plus.Messages.Parsers;
using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Groups
{
    /// <summary>
    /// Class Guild.
    /// </summary>
    internal class Guild
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        /// The name
        /// </summary>
        internal string Name;

        /// <summary>
        /// The description
        /// </summary>
        internal string Description;

        /// <summary>
        /// The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        /// The badge
        /// </summary>
        internal string Badge;

        /// <summary>
        /// The state
        /// </summary>
        internal uint State;

        /// <summary>
        /// The admin only deco
        /// </summary>
        internal uint AdminOnlyDeco;

        /// <summary>
        /// The create time
        /// </summary>
        internal int CreateTime;

        /// <summary>
        /// The creator identifier
        /// </summary>
        internal uint CreatorId;

        /// <summary>
        /// The colour1
        /// </summary>
        internal int Colour1;

        /// <summary>
        /// The colour2
        /// </summary>
        internal int Colour2;

        /// <summary>
        /// The members
        /// </summary>
        internal Dictionary<uint, GroupUser> Members;

        /// <summary>
        /// The admins
        /// </summary>
        internal Dictionary<uint, GroupUser> Admins;

        /// <summary>
        /// The requests
        /// </summary>
        internal List<uint> Requests;

        /// <summary>
        /// The has forum
        /// </summary>
        internal bool HasForum;

        /// <summary>
        /// The forum name
        /// </summary>
        internal string ForumName;

        /// <summary>
        /// The forum description
        /// </summary>
        internal string ForumDescription;

        /// <summary>
        /// The forum messages count
        /// </summary>
        internal uint ForumMessagesCount;

        /// <summary>
        /// The forum score
        /// </summary>
        internal double ForumScore;

        /// <summary>
        /// The forum last poster identifier
        /// </summary>
        internal uint ForumLastPosterId;

        /// <summary>
        /// The forum last poster name
        /// </summary>
        internal string ForumLastPosterName;

        /// <summary>
        /// The forum last poster timestamp
        /// </summary>
        internal int ForumLastPosterTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="Guild"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="badge">The badge.</param>
        /// <param name="create">The create.</param>
        /// <param name="creator">The creator.</param>
        /// <param name="colour1">The colour1.</param>
        /// <param name="colour2">The colour2.</param>
        /// <param name="members">The members.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="admins">The admins.</param>
        /// <param name="state">The state.</param>
        /// <param name="adminOnlyDeco">The admin only deco.</param>
        /// <param name="hasForum">if set to <c>true</c> [has forum].</param>
        /// <param name="forumName">Name of the forum.</param>
        /// <param name="forumDescription">The forum description.</param>
        /// <param name="forumMessagesCount">The forum messages count.</param>
        /// <param name="forumScore">The forum score.</param>
        /// <param name="forumLastPosterId">The forum last poster identifier.</param>
        /// <param name="forumLastPosterName">Name of the forum last poster.</param>
        /// <param name="forumLastPosterTimestamp">The forum last poster timestamp.</param>
        internal Guild(uint id, string name, string desc, uint roomId, string badge, int create, uint creator,
            int colour1, int colour2, Dictionary<uint, GroupUser> members, List<uint> requests,
            Dictionary<uint, GroupUser> admins, uint state, uint adminOnlyDeco, bool hasForum, string forumName,
            string forumDescription, uint forumMessagesCount, double forumScore, uint forumLastPosterId,
            string forumLastPosterName, int forumLastPosterTimestamp)
        {
            Id = id;
            Name = name;
            Description = desc;
            RoomId = roomId;
            Badge = badge;
            CreateTime = create;
            CreatorId = creator;
            Colour1 = ((colour1 == 0) ? 1 : colour1);
            Colour2 = ((colour2 == 0) ? 1 : colour2);
            Members = members;
            Requests = requests;
            Admins = admins;
            State = state;
            AdminOnlyDeco = adminOnlyDeco;
            HasForum = hasForum;
            ForumName = forumName;
            ForumDescription = forumDescription;
            ForumMessagesCount = forumMessagesCount;
            ForumScore = forumScore;
            ForumLastPosterId = forumLastPosterId;
            ForumLastPosterName = forumLastPosterName;
            ForumLastPosterTimestamp = forumLastPosterTimestamp;
        }

        /// <summary>
        /// Gets the forum last post time.
        /// </summary>
        /// <value>The forum last post time.</value>
        internal int ForumLastPostTime
        {
            get { return (Plus.GetUnixTimeStamp() - ForumLastPosterTimestamp); }
        }

        /// <summary>
        /// Forums the data message.
        /// </summary>
        /// <param name="requesterId">The requester identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage ForumDataMessage(uint requesterId)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumDataMessageComposer"));
            message.AppendInteger(Id);
            message.AppendString(Name);
            message.AppendString(Description);
            message.AppendString(Badge);
            message.AppendInteger(0);
            message.AppendInteger(0);
            message.AppendInteger(ForumMessagesCount);
            message.AppendInteger(0);
            message.AppendInteger(0);
            message.AppendInteger(ForumLastPosterId);
            message.AppendString(ForumLastPosterName);
            message.AppendInteger(ForumLastPostTime);
            message.AppendInteger(0);
            message.AppendInteger(1);
            message.AppendInteger(1);
            message.AppendInteger(2);
            message.AppendString("");
            message.AppendString((Members.ContainsKey(requesterId) ? "" : "not_member"));
            message.AppendString((Members.ContainsKey(requesterId) ? "" : "not_member"));
            message.AppendString((Admins.ContainsKey(requesterId) ? "" : "not_admin"));
            message.AppendString("");
            message.AppendBool(false);
            message.AppendBool(false);
            return message;
        }

        /// <summary>
        /// Serializes the forum root.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SerializeForumRoot(ServerMessage message)
        {
            message.AppendInteger(Id);
            message.AppendString(Name);
            message.AppendString("");
            message.AppendString(Badge);
            message.AppendInteger(0);
            message.AppendInteger((int)Math.Round(ForumScore));
            message.AppendInteger(ForumMessagesCount);
            message.AppendInteger(0);
            message.AppendInteger(0);
            message.AppendInteger(ForumLastPosterId);
            message.AppendString(ForumLastPosterName);
            message.AppendInteger(ForumLastPostTime);
        }

        /// <summary>
        /// Updates the forum.
        /// </summary>
        internal void UpdateForum()
        {
            if (!HasForum)
                return;
            using (var adapter = Plus.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery(
                    string.Format(
                        "UPDATE groups_data SET has_forum = '1', forum_name = @name , forum_description = @desc , forum_messages_count = @msgcount , forum_score = @score , forum_lastposter_id = @lastposterid , forum_lastposter_name = @lastpostername , forum_lastposter_timestamp = @lasttimestamp WHERE id ={0}",
                        Id));
                adapter.AddParameter("name", ForumName);
                adapter.AddParameter("desc", ForumDescription);
                adapter.AddParameter("msgcount", ForumMessagesCount);
                adapter.AddParameter("score", ForumScore.ToString());
                adapter.AddParameter("lastposterid", ForumLastPosterId);
                adapter.AddParameter("lastpostername", ForumLastPosterName);
                adapter.AddParameter("lasttimestamp", ForumLastPosterTimestamp);
                adapter.RunQuery();
            }
        }
    }
}